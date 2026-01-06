using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Weather/Sunny", fileName = "SunnyWeatherProfile")]
public class SunnyWeatherProfileSO : WeatherProfileBaseSO
{
    protected override void OnValidate()
    {
        base.OnValidate();
        if (string.IsNullOrWhiteSpace(weatherId))
            weatherId = "Sunny";

        spawnPrecipitation = false;
        overrideFog = false;
        sunIntensityMultiplier = Mathf.Max(1f, sunIntensityMultiplier);
        ambientColorTint = Color.Lerp(Color.white, ambientColorTint, 0.5f);
    }
}
