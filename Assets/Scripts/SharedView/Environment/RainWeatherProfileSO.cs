using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Weather/Rain", fileName = "RainWeatherProfile")]
public class RainWeatherProfileSO : WeatherProfileBaseSO
{
    [Header("Rain Specific")]
    [Range(0f, 1f)] public float wetnessDarken = 0.2f;
    public AudioClip thunderSfx;
    [Min(0.5f)] public float minThunderInterval = 6f;
    [Min(0.5f)] public float maxThunderInterval = 18f;
    [Range(0f, 1f)] public float minThunderChance = 0.05f;
    [Range(0f, 1f)] public float maxThunderChance = 0.6f;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (string.IsNullOrWhiteSpace(weatherId))
            weatherId = "Rain";

        spawnPrecipitation = true;
        overrideFog = true;
        sunIntensityMultiplier = Mathf.Min(sunIntensityMultiplier, 0.7f);
        ambientColorTint = Color.Lerp(ambientColorTint, new Color(0.8f, 0.85f, 0.9f), 0.5f);
        ambienceVolume = Mathf.Clamp01(ambienceVolume);
        if (minThunderInterval > maxThunderInterval)
            (minThunderInterval, maxThunderInterval) = (maxThunderInterval, minThunderInterval);
        EnsureSpawnCurve();
    }

    public void PlayThunder(AudioSource source)
    {
        if (source == null || thunderSfx == null)
            return;
        source.PlayOneShot(thunderSfx);
    }

    public float EvaluateThunderChance(float intensity)
    {
        return Mathf.Lerp(minThunderChance, maxThunderChance, Mathf.Clamp01(intensity));
    }

    public float EvaluateThunderDelay(float intensity)
    {
        float min = Mathf.Max(0.5f, minThunderInterval);
        float max = Mathf.Max(min, maxThunderInterval);
        return Mathf.Lerp(max, min, Mathf.Clamp01(intensity));
    }

    private void EnsureSpawnCurve()
    {
        if (spawnChanceOverDay == null || spawnChanceOverDay.keys == null || spawnChanceOverDay.keys.Length < 2)
        {
            spawnChanceOverDay = new AnimationCurve(
                new Keyframe(0f, 0.2f),
                new Keyframe(0.25f, 0.35f),
                new Keyframe(0.5f, 0.65f),
                new Keyframe(0.75f, 0.45f),
                new Keyframe(1f, 0.2f));
        }
    }
}
