using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class KawaseBlur : ScriptableRendererFeature
{
    [System.Serializable]
    public class KawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial;

        [Range(1, 15)]
        public int blurPasses = 1;

        [Range(1, 4)]
        public int downsample = 1;

        public bool copyToFramebuffer;
        public string targetName = "_MainTex";
    }

    private sealed class CustomRenderPass : ScriptableRenderPass
    {
        private sealed class PassData
        {
            internal TextureHandle sourceTexture;
            internal Material material;
            internal float offset;
        }

        private readonly ProfilingSampler _profilingSampler;
        private readonly string _profilerTag;

        private Material _blurMaterial;
        private int _passes;
        private int _downsample;
        private bool _copyToFramebuffer;
        private int _targetNameId;

        public CustomRenderPass(string profilerTag)
        {
            _profilerTag = profilerTag;
            _profilingSampler = new ProfilingSampler(profilerTag);
            requiresIntermediateTexture = true;
        }

        public void Setup(Material blurMaterial, int passes, int downsample, bool copyToFramebuffer, string targetName)
        {
            _blurMaterial = blurMaterial;
            _passes = Mathf.Max(1, passes);
            _downsample = Mathf.Max(1, downsample);
            _copyToFramebuffer = copyToFramebuffer;
            _targetNameId = string.IsNullOrWhiteSpace(targetName) ? -1 : Shader.PropertyToID(targetName);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_blurMaterial == null)
                return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogWarning($"{_profilerTag} skipped because the active target is the back buffer.");
                return;
            }

            var source = resourceData.activeColorTexture;
            if (!source.IsValid())
                return;

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.width = Mathf.Max(1, descriptor.width / _downsample);
            descriptor.height = Mathf.Max(1, descriptor.height / _downsample);
            descriptor.depthBufferBits = 0;
            descriptor.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;
            descriptor.msaaSamples = 1;

            var ping = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                descriptor,
                "_KawaseBlurPing",
                false,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp);

            var pong = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                descriptor,
                "_KawaseBlurPong",
                false,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp);

            AddBlurPass(renderGraph, source, ping, 1.5f, "KawaseBlur First");

            var currentSource = ping;
            var currentDestination = pong;

            for (var i = 1; i < _passes - 1; i++)
            {
                AddBlurPass(renderGraph, currentSource, currentDestination, 0.5f + i, $"KawaseBlur Iteration {i}");
                (currentSource, currentDestination) = (currentDestination, currentSource);
            }

            var finalDestination = _copyToFramebuffer
                ? resourceData.activeColorTexture
                : currentDestination;

            AddBlurPass(
                renderGraph,
                currentSource,
                finalDestination,
                0.5f + _passes - 1f,
                "KawaseBlur Final",
                setGlobalAfterPass: !_copyToFramebuffer && _targetNameId >= 0,
                globalTextureId: _targetNameId);
        }

        private void AddBlurPass(
            RenderGraph renderGraph,
            TextureHandle source,
            TextureHandle destination,
            float offset,
            string passName,
            bool setGlobalAfterPass = false,
            int globalTextureId = -1)
        {
            using var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData, _profilingSampler);

            builder.AllowGlobalStateModification(true);
            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

            if (setGlobalAfterPass)
                builder.SetGlobalTextureAfterPass(destination, globalTextureId);

            passData.sourceTexture = source;
            passData.material = _blurMaterial;
            passData.offset = offset;

            builder.SetRenderFunc(static (PassData data, RasterGraphContext context) =>
            {
                var cmd = context.cmd;
                RTHandle sourceTextureHandle = data.sourceTexture;
                var viewportScale = sourceTextureHandle.useScaling
                    ? new Vector2(
                        sourceTextureHandle.rtHandleProperties.rtHandleScale.x,
                        sourceTextureHandle.rtHandleProperties.rtHandleScale.y)
                    : Vector2.one;

                data.material.SetFloat("_offset", data.offset);
                Blitter.BlitTexture(cmd, sourceTextureHandle, viewportScale, data.material, 0);
            });
        }
    }

    public KawaseBlurSettings settings = new KawaseBlurSettings();

    private CustomRenderPass _scriptablePass;

    public override void Create()
    {
        _scriptablePass = new CustomRenderPass("KawaseBlur")
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurMaterial == null)
            return;

        _scriptablePass.renderPassEvent = settings.renderPassEvent;
        _scriptablePass.Setup(
            settings.blurMaterial,
            settings.blurPasses,
            settings.downsample,
            settings.copyToFramebuffer,
            settings.targetName);

        renderer.EnqueuePass(_scriptablePass);
    }
}
