using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Weather/Fog", fileName = "FogWeatherProfile")]
public class FogWeatherProfileSO : WeatherProfileBaseSO
{
    [Header("Fog Specific")]
    [Range(0f, 1f)] public float visibilityMultiplier = 0.5f;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (string.IsNullOrWhiteSpace(weatherId))
            weatherId = "Fog";

        overrideFog = true;
        spawnPrecipitation = false;
        fogDensity = Mathf.Clamp(fogDensity * Mathf.Max(visibilityMultiplier, 0.1f), 0.01f, 0.5f);
        ambienceVolume = Mathf.Clamp01(ambienceVolume);
        EnsureSpawnCurve();
    }

    private void EnsureSpawnCurve()
    {
        if (spawnChanceOverDay == null || spawnChanceOverDay.keys == null || spawnChanceOverDay.keys.Length < 2)
        {
            spawnChanceOverDay = new AnimationCurve(
                new Keyframe(0f, 0.4f),
                new Keyframe(0.15f, 0.85f),
                new Keyframe(0.3f, 0.2f),
                new Keyframe(0.5f, 0.05f),
                new Keyframe(0.7f, 0.2f),
                new Keyframe(0.85f, 0.85f),
                new Keyframe(1f, 0.4f));
        }
    }
}
