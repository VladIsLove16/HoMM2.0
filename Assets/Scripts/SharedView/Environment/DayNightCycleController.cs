using System.Collections;
using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
public class DayNightCycleController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightProfileSO profile;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Runtime Options")]
    [SerializeField] private bool runInEditMode = true;
    [SerializeField] [Range(0.1f, 30f)] private float debugBlendDuration = 2f;

    [Header("State (Read Only)")]
    [ReadOnly] [Range(0f, 1f)] [SerializeField] private float timeOfDay01;
    [ReadOnly] [SerializeField] private LightingSnapshot currentSnapshot;

    private Material _runtimeSkybox;
    private Coroutine _blendRoutine;

    public float TimeOfDay01 => timeOfDay01;
    public LightingSnapshot CurrentSnapshot => currentSnapshot;
    public Light SunLight => sunLight;
    public Light MoonLight => moonLight;

    private void Awake()
    {
        InitializeSkybox();
        ResetTime();
        EvaluateAndApply();
    }

    private void OnEnable()
    {
        InitializeSkybox();
        EvaluateAndApply();
    }

    private void OnDestroy()
    {
        if (_runtimeSkybox != null && Application.isPlaying)
        {
            Destroy(_runtimeSkybox);
            _runtimeSkybox = null;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying && !runInEditMode)
            return;

        if (profile == null || sunLight == null)
            return;

        var delta = Application.isPlaying ? Time.deltaTime : Time.unscaledDeltaTime;
        AdvanceTime(delta);
    }

    public void ResetTime()
    {
        if (profile != null)
        {
            timeOfDay01 = Mathf.Repeat(profile.defaultStartTime, 1f);
        }
    }

    public void AdvanceTime(float deltaSeconds)
    {
        if (profile == null)
            return;

        var secondsPerDay = Mathf.Max(1f, profile.SecondsPerFullDay);
        timeOfDay01 = Mathf.Repeat(timeOfDay01 + deltaSeconds / secondsPerDay, 1f);
        EvaluateAndApply();
    }

    public void SetTimeNormalized(float normalizedValue, bool instant = true, float overrideBlendDuration = -1f)
    {
        normalizedValue = Mathf.Repeat(normalizedValue, 1f);

        if (_blendRoutine != null)
        {
            StopCoroutine(_blendRoutine);
            _blendRoutine = null;
        }

        if (instant || overrideBlendDuration == 0f)
        {
            timeOfDay01 = normalizedValue;
            EvaluateAndApply();
        }
        else
        {
            var duration = overrideBlendDuration > 0f ? overrideBlendDuration : debugBlendDuration;
            _blendRoutine = StartCoroutine(BlendTime(normalizedValue, duration));
        }
    }

    public void SetTimeHours(float hours, bool instant = true, float overrideBlendDuration = -1f)
    {
        SetTimeNormalized(hours / 24f, instant, overrideBlendDuration);
    }

    public void SnapToCurrentSettings()
    {
        EvaluateAndApply();
    }

    private IEnumerator BlendTime(float target, float duration)
    {
        float start = timeOfDay01;
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / Mathf.Max(0.01f, duration);
            timeOfDay01 = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, progress));
            EvaluateAndApply();
            yield return null;
        }

        timeOfDay01 = target;
        EvaluateAndApply();
        _blendRoutine = null;
    }

    private void EvaluateAndApply()
    {
        if (profile == null || sunLight == null)
            return;

        float t = timeOfDay01;

        var sunColor = profile.sunLightColor.Evaluate(t);
        var sunIntensity = Mathf.Max(0f, profile.sunLightIntensity.Evaluate(t));
        var moonColor = profile.moonLightColor.Evaluate(t);
        var moonIntensity = Mathf.Max(0f, profile.moonLightIntensity.Evaluate(t));
        var ambientColor = profile.ambientColor.Evaluate(t);
        var fogColor = profile.fogColor.Evaluate(t);
        var fogDensity = Mathf.Max(0f, profile.fogDensity.Evaluate(t));
        var skyColor = profile.skyColor.Evaluate(t);

        float sunAngle = (t * 360f) - 90f;

        sunLight.transform.rotation = Quaternion.Euler(sunAngle, profile.sunRotationOffsetY, 0f);
        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;

        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(sunAngle - 180f, profile.sunRotationOffsetY, 0f);
            moonLight.color = moonColor;
            moonLight.intensity = moonIntensity;
        }

        RenderSettings.sun = sunLight;
        RenderSettings.ambientLight = ambientColor;
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = profile.fogMode;
        RenderSettings.fogDensity = fogDensity;

        if (_runtimeSkybox != null)
        {
            if (_runtimeSkybox.HasProperty("_Tint"))
            {
                _runtimeSkybox.SetColor("_Tint", skyColor);
            }
            if (_runtimeSkybox.HasProperty("_SkyTint"))
            {
                _runtimeSkybox.SetColor("_SkyTint", skyColor);
            }
            if (_runtimeSkybox.HasProperty("_Exposure"))
            {
                _runtimeSkybox.SetFloat("_Exposure", profile.skyboxExposure.Evaluate(t));
            }
        }

        if (Application.isPlaying)
        {
            DynamicGI.UpdateEnvironment();
        }

        currentSnapshot = new LightingSnapshot
        {
            Time01 = t,
            SkyColor = skyColor,
            AmbientColor = ambientColor,
            FogColor = fogColor,
            FogDensity = fogDensity,
            SunColor = sunColor,
            SunIntensity = sunIntensity
        };
    }

    private void InitializeSkybox()
    {
        if (profile == null || profile.overrideSkybox == null)
            return;

        if (_runtimeSkybox == null)
        {
            _runtimeSkybox = Application.isPlaying
                ? Instantiate(profile.overrideSkybox)
                : new Material(profile.overrideSkybox);
        }

        if (RenderSettings.skybox != _runtimeSkybox)
        {
            RenderSettings.skybox = _runtimeSkybox;
        }
    }

#if UNITY_EDITOR
    [Button("Debug: Midnight (Instant)")]
    private void DebugMidnightInstant() => SetTimeNormalized(0f, true);

    [Button("Debug: Sunrise (Blend)")]
    private void DebugSunriseBlend() => SetTimeHours(6f, false);

    [Button("Debug: Noon (Blend)")]
    private void DebugNoonBlend() => SetTimeHours(12f, false);

    [Button("Debug: Sunset (Blend)")]
    private void DebugSunsetBlend() => SetTimeHours(18f, false);

    [Button("Debug: Advance 1 Hour")]
    private void DebugAdvanceHour() => AdvanceTime(profile != null ? profile.SecondsPerFullDay / 24f : 60f);
#endif

    [System.Serializable]
    public struct LightingSnapshot
    {
        [Range(0f, 1f)] public float Time01;
        public Color SkyColor;
        public Color AmbientColor;
        public Color FogColor;
        public float FogDensity;
        public Color SunColor;
        public float SunIntensity;
    }
}
