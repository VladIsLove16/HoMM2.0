using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class WeatherSystemController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DayNightCycleController dayNightController;
    [SerializeField] private Transform precipitationAnchor;
    [SerializeField] private WindZone windZone;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Profiles")]
    [SerializeField] private WeatherProfileBaseSO defaultWeather;
    [SerializeField] private List<WeatherProfileBaseSO> availableWeathers = new();

    [Header("Transition")]
    [SerializeField] [Range(0.5f, 30f)] private float transitionSeconds = 5f;

    [Header("Scheduling")]
    [SerializeField] private bool autoSchedule = true;
    [SerializeField] [Range(0.25f, 6f)] private float weatherIntervalHours = 3f;
    [SerializeField] private bool scheduleOnStart = true;

    [Header("State (Read Only)")]
    [ReadOnly] [SerializeField] private WeatherProfileBaseSO currentWeather;
    [ReadOnly] [SerializeField] private WeatherProfileBaseSO previousWeather;
    [ReadOnly] [SerializeField] private WeatherProfileBaseSO targetWeather;
    [ReadOnly] [SerializeField] [Range(0f, 1f)] private float transitionProgress = 1f;
    [ReadOnly] [SerializeField] [Range(0f, 1f)] private float currentIntensity = 1f;
    [ReadOnly] [SerializeField] [Range(0f, 1f)] private float previousIntensity = 1f;
    [SerializeField] [Range(0f, 1f)] private float targetIntensity = 1f;

    private Coroutine _ambienceRoutine;
    private Coroutine _thunderRoutine;
    [SerializeField] private ParticleSystem precipitationSystem;
    private ParticleSystem[] _precipitationSystems;
    private float[] _precipitationBaseRates;
    private readonly List<(WeatherProfileBaseSO profile, float weight)> _spawnWeights = new();
    private bool _schedulerInitialized;
    private float _intervalCursor;

    public WeatherProfileBaseSO ActiveWeather => currentWeather;

    private void Start()
    {
        InitializeDefaults();
        ApplyBlendedState();
    }

    private void OnDisable()
    {
        if (_thunderRoutine != null)
        {
            StopCoroutine(_thunderRoutine);
            _thunderRoutine = null;
        }
    }

    private void InitializeDefaults()
    {
        var startupWeather = defaultWeather != null ? defaultWeather : (availableWeathers.Count > 0 ? availableWeathers[0] : null);
        if (startupWeather == null)
            return;

        currentWeather = startupWeather;
        previousWeather = startupWeather;
        targetWeather = startupWeather;
        float startIntensity = SampleIntensity(startupWeather);
        currentIntensity = previousIntensity = targetIntensity = startIntensity;
        transitionProgress = 1f;
        RefreshPrecipitation(startupWeather, true);
        RefreshAudio(startupWeather, true);
        ApplyWind(startupWeather, startIntensity);
        UpdateThunderRoutine(startupWeather);
    }

    private void LateUpdate()
    {
        if (autoSchedule)
        {
            UpdateScheduling();
        }

        if (targetWeather == null)
            return;

        if (transitionProgress < 1f)
        {
            transitionProgress = Mathf.MoveTowards(transitionProgress, 1f, Time.deltaTime / Mathf.Max(0.1f, transitionSeconds));
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                currentWeather = targetWeather;
                previousWeather = currentWeather;
                previousIntensity = targetIntensity;
                currentIntensity = targetIntensity;
            }
        }

        ApplyBlendedState();
    }

    public void SetWeather(WeatherProfileBaseSO profile, bool instant = false)
    {
        if (profile == null)
        {
            Debug.LogWarning("[WeatherSystemController] Attempted to set a null weather profile.", this);
            return;
        }

        float sampledIntensity = SampleIntensity(profile);

        if (instant || currentWeather == null)
        {
            currentWeather = profile;
            previousWeather = profile;
            targetWeather = profile;
            previousIntensity = sampledIntensity;
            targetIntensity = sampledIntensity;
            currentIntensity = sampledIntensity;
            transitionProgress = 1f;
            ApplyWind(profile, sampledIntensity);
            RefreshPrecipitation(profile, true);
            RefreshAudio(profile, true);
            PlayEnterSfx(profile);
            UpdateThunderRoutine(profile);
            ApplyBlendedState();
            return;
        }

        PlayExitSfx(currentWeather);
        previousWeather = currentWeather;
        previousIntensity = currentIntensity;
        targetWeather = profile;
        targetIntensity = sampledIntensity;
        transitionProgress = 0f;
        ApplyWind(profile, sampledIntensity);
        RefreshPrecipitation(profile, false);
        RefreshAudio(profile, false);
        PlayEnterSfx(profile);
        UpdateThunderRoutine(profile);
    }

    private float IntervalNormalized => Mathf.Clamp(weatherIntervalHours / 24f, 0.0001f, 1f);

    private void UpdateScheduling()
    {
        if (!autoSchedule || dayNightController == null)
            return;

        float now = dayNightController.TimeOfDay01;

        if (!_schedulerInitialized)
        {
            _schedulerInitialized = true;
            _intervalCursor = now;

            if (scheduleOnStart)
            {
                var initial = ChooseWeatherForTime(now);
                if (initial != null)
                {
                    SetWeather(initial, true);
                }
            }
            return;
        }

        float interval = IntervalNormalized;
        float distance = now - _intervalCursor;
        if (distance < 0f)
            distance += 1f;

        while (distance >= interval)
        {
            distance -= interval;
            _intervalCursor = Mathf.Repeat(_intervalCursor + interval, 1f);
            var profile = ChooseWeatherForTime(_intervalCursor);
            if (profile != null)
            {
                SetWeather(profile, false);
            }
        }
    }

    private void ApplyBlendedState()
    {
        if (dayNightController == null)
            return;

        float blend = transitionProgress;
        var snapshot = dayNightController.CurrentSnapshot;
        var from = previousWeather != null ? previousWeather : defaultWeather;
        var to = targetWeather != null ? targetWeather : from;
        float fromIntensity = Mathf.Clamp01(previousIntensity);
        float toIntensity = Mathf.Clamp01(targetIntensity);

        var fogColorFrom = ResolveFogColor(from, snapshot, fromIntensity);
        var fogColorTo = ResolveFogColor(to, snapshot, toIntensity);
        var fogDensityFrom = ResolveFogDensity(from, snapshot, fromIntensity);
        var fogDensityTo = ResolveFogDensity(to, snapshot, toIntensity);
        RenderSettings.fogColor = Color.Lerp(fogColorFrom, fogColorTo, blend);
        RenderSettings.fogDensity = Mathf.Lerp(fogDensityFrom, fogDensityTo, blend);
        RenderSettings.fogMode = blend < 0.5f ? ResolveFogMode(from) : ResolveFogMode(to);

        var sun = dayNightController != null ? dayNightController.SunLight : null;
        if (sun != null)
        {
            var sunColorFrom = ResolveSunColor(from, snapshot, fromIntensity);
            var sunColorTo = ResolveSunColor(to, snapshot, toIntensity);
            var sunIntensityFrom = ResolveSunIntensity(from, snapshot, fromIntensity);
            var sunIntensityTo = ResolveSunIntensity(to, snapshot, toIntensity);
            sun.color = Color.Lerp(sunColorFrom, sunColorTo, blend);
            sun.intensity = Mathf.Lerp(sunIntensityFrom, sunIntensityTo, blend);
        }

        var ambientFrom = ResolveAmbientColor(from, snapshot, fromIntensity);
        var ambientTo = ResolveAmbientColor(to, snapshot, toIntensity);
        RenderSettings.ambientLight = Color.Lerp(ambientFrom, ambientTo, blend);

        var precipitationFrom = EvaluatePrecipitationStrength(from) * fromIntensity;
        var precipitationTo = EvaluatePrecipitationStrength(to) * toIntensity;
        UpdatePrecipitationEmission(Mathf.Lerp(precipitationFrom, precipitationTo, blend));
        currentIntensity = Mathf.Lerp(fromIntensity, toIntensity, blend);
    }

    private float SampleIntensity(WeatherProfileBaseSO profile)
    {
        return profile != null ? Mathf.Clamp01(profile.RollRandomIntensity()) : 1f;
    }

    private WeatherProfileBaseSO ChooseWeatherForTime(float normalizedTime)
    {
        _spawnWeights.Clear();
        float total = 0f;

        if (availableWeathers != null)
        {
            foreach (var profile in availableWeathers)
            {
                if (profile == null)
                    continue;
                float weight = Mathf.Clamp01(profile.EvaluateSpawnChance(normalizedTime));
                if (weight <= 0f)
                    continue;
                _spawnWeights.Add((profile, weight));
                total += weight;
            }
        }

        float fallback = Mathf.Max(0.0001f, 1f - Mathf.Clamp01(total));
        float roll = Random.value * (total + fallback);

        foreach (var entry in _spawnWeights)
        {
            if (roll <= entry.weight)
                return entry.profile;
            roll -= entry.weight;
        }

        if (roll <= fallback && defaultWeather != null)
            return defaultWeather;

        if (defaultWeather != null)
            return defaultWeather;

        return _spawnWeights.Count > 0 ? _spawnWeights[0].profile : currentWeather;
    }

    private static Color ResolveFogColor(WeatherProfileBaseSO profile, DayNightCycleController.LightingSnapshot snapshot, float intensity)
    {
        if (profile == null || !profile.overrideFog)
            return snapshot.FogColor;
        return Color.Lerp(snapshot.FogColor, profile.fogColor, Mathf.Clamp01(intensity));
    }

    private static float ResolveFogDensity(WeatherProfileBaseSO profile, DayNightCycleController.LightingSnapshot snapshot, float intensity)
    {
        if (profile == null || !profile.overrideFog)
            return snapshot.FogDensity;
        return Mathf.Lerp(snapshot.FogDensity, profile.fogDensity, Mathf.Clamp01(intensity));
    }

    private static FogMode ResolveFogMode(WeatherProfileBaseSO profile)
    {
        if (profile == null)
            return RenderSettings.fogMode;
        return profile.fogMode;
    }

    private static Color ResolveSunColor(WeatherProfileBaseSO profile, DayNightCycleController.LightingSnapshot snapshot, float intensity)
    {
        if (profile == null)
            return snapshot.SunColor;
        var tinted = MultiplyColor(snapshot.SunColor, profile.sunColorTint);
        return Color.Lerp(snapshot.SunColor, tinted, Mathf.Clamp01(intensity));
    }

    private static float ResolveSunIntensity(WeatherProfileBaseSO profile, DayNightCycleController.LightingSnapshot snapshot, float intensity)
    {
        if (profile == null)
            return snapshot.SunIntensity;
        float target = snapshot.SunIntensity * Mathf.Max(0f, profile.sunIntensityMultiplier);
        return Mathf.Lerp(snapshot.SunIntensity, target, Mathf.Clamp01(intensity));
    }

    private static Color ResolveAmbientColor(WeatherProfileBaseSO profile, DayNightCycleController.LightingSnapshot snapshot, float intensity)
    {
        if (profile == null)
            return snapshot.AmbientColor;
        var tinted = MultiplyColor(snapshot.AmbientColor, profile.ambientColorTint);
        return Color.Lerp(snapshot.AmbientColor, tinted, Mathf.Clamp01(intensity));
    }

    private static float EvaluatePrecipitationStrength(WeatherProfileBaseSO profile)
    {
        return profile != null && profile.spawnPrecipitation ? 1f : 0f;
    }

    private static Color MultiplyColor(Color original, Color tint)
    {
        return new Color(original.r * tint.r, original.g * tint.g, original.b * tint.b, 1f);
    }

    private void RefreshPrecipitation(WeatherProfileBaseSO profile, bool instant)
    {
        if (!EnsurePrecipitationCache())
        {
            if (profile != null && profile.spawnPrecipitation)
                Debug.LogWarning("[WeatherSystemController] Precipitation requested but no ParticleSystem assigned.", this);
            return;
        }

        if (profile == null || !profile.spawnPrecipitation)
        {
            TogglePrecipitation(false);
            return;
        }

        TogglePrecipitation(true);
        var initialStrength = EvaluatePrecipitationStrength(profile) * (instant ? Mathf.Clamp01(targetIntensity) : 0f);
        UpdatePrecipitationEmission(initialStrength);
    }

    private bool EnsurePrecipitationCache()
    {
        if (precipitationSystem == null)
            return false;

        if (_precipitationSystems != null && _precipitationSystems.Length > 0)
            return true;

        _precipitationSystems = precipitationSystem.GetComponentsInChildren<ParticleSystem>(true);
        if (_precipitationSystems == null || _precipitationSystems.Length == 0)
            return false;

        _precipitationBaseRates = new float[_precipitationSystems.Length];
        for (int i = 0; i < _precipitationSystems.Length; i++)
        {
            var emission = _precipitationSystems[i].emission;
            _precipitationBaseRates[i] = emission.rateOverTimeMultiplier;
        }

        return true;
    }

    private void TogglePrecipitation(bool enable)
    {
        if (_precipitationSystems == null)
            return;

        foreach (var ps in _precipitationSystems)
        {
            if (ps == null)
                continue;

            if (enable)
            {
                if (!ps.gameObject.activeSelf)
                    ps.gameObject.SetActive(true);
                if (!ps.isPlaying)
                    ps.Play();
            }
            else
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }

    private void UpdatePrecipitationEmission(float normalizedIntensity)
    {
        if (_precipitationSystems == null || _precipitationBaseRates == null)
            return;

        normalizedIntensity = Mathf.Clamp01(normalizedIntensity);
        for (int i = 0; i < _precipitationSystems.Length; i++)
        {
            var ps = _precipitationSystems[i];
            if (ps == null)
                continue;
            var emission = ps.emission;
            var baseRate = i < _precipitationBaseRates.Length ? _precipitationBaseRates[i] : emission.rateOverTimeMultiplier;
            emission.rateOverTimeMultiplier = baseRate * normalizedIntensity;
        }
    }

    private void RefreshAudio(WeatherProfileBaseSO profile, bool instant)
    {
        if (ambienceSource == null)
            return;

        if (_ambienceRoutine != null)
        {
            StopCoroutine(_ambienceRoutine);
            _ambienceRoutine = null;
        }

        if (instant)
        {
            ambienceSource.clip = profile != null ? profile.ambienceLoop : null;
            ambienceSource.volume = profile != null ? profile.ambienceVolume : 0f;
            if (ambienceSource.clip != null)
                ambienceSource.Play();
            else
                ambienceSource.Stop();
            return;
        }

        _ambienceRoutine = StartCoroutine(CrossFadeAmbience(profile));
    }

    private IEnumerator CrossFadeAmbience(WeatherProfileBaseSO profile)
    {
        if (ambienceSource == null)
            yield break;

        float duration = Mathf.Max(0.01f, transitionSeconds * 0.5f);
        float startVolume = ambienceSource.volume;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            ambienceSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        ambienceSource.volume = 0f;
        ambienceSource.clip = profile != null ? profile.ambienceLoop : null;
        if (ambienceSource.clip != null)
        {
            ambienceSource.Play();
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                ambienceSource.volume = Mathf.Lerp(0f, profile.ambienceVolume, t / duration);
                yield return null;
            }
            ambienceSource.volume = profile.ambienceVolume;
        }
        else
        {
            ambienceSource.Stop();
        }

        _ambienceRoutine = null;
    }

    private void ApplyWind(WeatherProfileBaseSO profile, float intensity)
    {
        if (windZone == null || profile == null || !profile.overrideWind)
            return;

        float multiplier = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(intensity));
        windZone.windMain = profile.windMain * multiplier;
        windZone.windTurbulence = profile.windTurbulence * multiplier;
    }

    private void PlayEnterSfx(WeatherProfileBaseSO profile)
    {
        if (sfxSource != null && profile != null && profile.enterSfx != null)
        {
            sfxSource.PlayOneShot(profile.enterSfx);
        }
    }

    private void PlayExitSfx(WeatherProfileBaseSO profile)
    {
        if (sfxSource != null && profile != null && profile.exitSfx != null)
        {
            sfxSource.PlayOneShot(profile.exitSfx);
        }
    }

    private void UpdateThunderRoutine(WeatherProfileBaseSO profile)
    {
        if (_thunderRoutine != null)
        {
            StopCoroutine(_thunderRoutine);
            _thunderRoutine = null;
        }

        if (profile is RainWeatherProfileSO rainProfile && rainProfile.thunderSfx != null)
        {
            _thunderRoutine = StartCoroutine(ThunderRoutine(rainProfile));
        }
    }

    private IEnumerator ThunderRoutine(RainWeatherProfileSO rainProfile)
    {
        while (targetWeather == rainProfile)
        {
            float intensity = Mathf.Clamp01(currentIntensity);
            float delay = rainProfile.EvaluateThunderDelay(intensity);
            yield return new WaitForSeconds(delay);

            if (targetWeather != rainProfile)
                break;

            if (Random.value <= rainProfile.EvaluateThunderChance(intensity))
            {
                rainProfile.PlayThunder(sfxSource != null ? sfxSource : ambienceSource);
            }
        }

        _thunderRoutine = null;
    }

#if UNITY_EDITOR
    [Button("Debug: Apply Default Weather")]
    private void DebugApplyDefault() => SetWeather(defaultWeather, false);

    [Button("Debug: Apply Default (Instant)")]
    private void DebugApplyDefaultInstant() => SetWeather(defaultWeather, true);

    [Button("Debug: Cycle Weather")]
    private void DebugCycleWeather()
    {
        if (availableWeathers == null || availableWeathers.Count == 0)
            return;

        int currentIndex = availableWeathers.IndexOf(targetWeather);
        int nextIndex = (currentIndex + 1) % availableWeathers.Count;
        SetWeather(availableWeathers[nextIndex], false);
    }
#endif
}
