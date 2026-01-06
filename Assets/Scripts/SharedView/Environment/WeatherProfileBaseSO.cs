using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Weather/Base Profile", fileName = "WeatherProfile")]
public class WeatherProfileBaseSO : ScriptableObject
{
    [Serializable]
    public struct WeatherIntensityOption
    {
        public string label;
        [Range(0f, 1f)] public float strength;
        [Min(0f)] public float probabilityWeight;
    }

    [Header("Metadata")]
    public string weatherId = "Default";
    [TextArea] public string description;

    [Header("Lighting Overrides")]
    [Tooltip("Multiplies the sun intensity provided by the day/night system.")]
    public float sunIntensityMultiplier = 1f;
    public Color sunColorTint = Color.white;
    public Color ambientColorTint = Color.white;

    [Header("Fog")]
    public bool overrideFog = true;
    public FogMode fogMode = FogMode.ExponentialSquared;
    [Range(0f, 0.5f)] public float fogDensity = 0.02f;
    public Color fogColor = new Color(0.7f, 0.8f, 0.9f);

    [Header("Precipitation (Particle/VFX Prefab)")]
    public bool spawnPrecipitation;
    public GameObject precipitationPrefab;
    public Vector3 precipitationOffset;

    [Header("Audio")]
    public AudioClip ambienceLoop;
    public AudioClip enterSfx;
    public AudioClip exitSfx;
    [Range(0f, 1f)] public float ambienceVolume = 0.6f;

    [Header("Wind Zone")]
    public bool overrideWind;
    public float windMain = 0.5f;
    public float windTurbulence = 0.1f;

    [Header("Intensity Randomization")]
    [Tooltip("Weighted list of possible strengths. Leave empty for deterministic strength 1.0.")]
    public WeatherIntensityOption[] intensityOptions = Array.Empty<WeatherIntensityOption>();

    [Header("Scheduling")]
    [Tooltip("Chance (0-1) that this profile will be picked automatically at given normalized time-of-day.")]
    public AnimationCurve spawnChanceOverDay = AnimationCurve.Constant(0f, 1f, 0f);

    protected virtual void OnValidate()
    {
        sunIntensityMultiplier = Mathf.Max(0f, sunIntensityMultiplier);
        fogDensity = Mathf.Clamp(fogDensity, 0f, 0.5f);
        ambienceVolume = Mathf.Clamp01(ambienceVolume);

        if (intensityOptions == null)
            intensityOptions = Array.Empty<WeatherIntensityOption>();
        else
        {
            for (int i = 0; i < intensityOptions.Length; i++)
            {
                intensityOptions[i].strength = Mathf.Clamp01(intensityOptions[i].strength);
                intensityOptions[i].probabilityWeight = Mathf.Max(0f, intensityOptions[i].probabilityWeight);
            }
        }

        spawnChanceOverDay ??= AnimationCurve.Constant(0f, 1f, 0f);
    }

    public float RollRandomIntensity()
    {
        if (intensityOptions == null || intensityOptions.Length == 0)
            return 1f;

        float totalWeight = 0f;
        foreach (var option in intensityOptions)
        {
            totalWeight += Mathf.Max(0.0001f, option.probabilityWeight);
        }

        float pick = UnityEngine.Random.value * totalWeight;
        foreach (var option in intensityOptions)
        {
            pick -= Mathf.Max(0.0001f, option.probabilityWeight);
            if (pick <= 0f)
                return Mathf.Clamp01(option.strength);
        }

        return Mathf.Clamp01(intensityOptions[intensityOptions.Length - 1].strength);
    }

    public virtual float EvaluateSpawnChance(float normalizedTime)
    {
        if (spawnChanceOverDay == null)
            return 0f;
        return Mathf.Clamp01(spawnChanceOverDay.Evaluate(Mathf.Repeat(normalizedTime, 1f)));
    }
}
