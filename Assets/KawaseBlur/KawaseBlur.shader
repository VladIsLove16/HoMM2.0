Shader "Custom/RenderFeature/KawaseBlur"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "KawaseBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            SAMPLER(sampler_BlitTexture);

            float _offset;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
                float2 texelSize = _BlitTexture_TexelSize.xy;
                float2 diagonal = texelSize * _offset;

                half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).rgb;
                color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + diagonal).rgb;
                color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + float2(diagonal.x, -diagonal.y)).rgb;
                color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + float2(-diagonal.x, diagonal.y)).rgb;
                color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv - diagonal).rgb;

                return half4(color * 0.2h, 1.0h);
            }
            ENDHLSL
        }
    }
}
