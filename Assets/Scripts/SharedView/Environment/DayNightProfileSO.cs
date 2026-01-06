using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Day Night Profile", fileName = "DayNightProfile")]
public class DayNightProfileSO : ScriptableObject
{
    [Header("Time")]
    [Min(0.1f)]
    [Tooltip("Length of a full 24h cycle expressed in minutes of real time.")]
    public float dayLengthMinutes = 10f;

    [Range(0f, 1f)]
    [Tooltip("Normalized time of day applied on Awake (0 = midnight, 0.5 = noon).")]
    public float defaultStartTime = 0.25f;

    [Tooltip("Offsets the horizontal rotation of the sun to account for scene orientation.")]
    public float sunRotationOffsetY = 0f;

    [Header("Sky")]
    public Gradient skyColor = CreateGradient(new Color(0.02f, 0.05f, 0.12f), new Color(0.7f, 0.85f, 1f));
    public AnimationCurve skyboxExposure = AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1f);

    [Header("Sun Light")]
    public Gradient sunLightColor = CreateGradient(new Color(0.87f, 0.68f, 0.55f), Color.white);
    public AnimationCurve sunLightIntensity = AnimationCurve.EaseInOut(0f, 0.05f, 1f, 1.1f);

    [Header("Moon Light")]
    public Gradient moonLightColor = CreateGradient(new Color(0.2f, 0.2f, 0.35f), new Color(0.5f, 0.5f, 0.7f));
    public AnimationCurve moonLightIntensity = AnimationCurve.EaseInOut(0f, 0.8f, 1f, 0.05f);

    [Header("Ambient")]
    public Gradient ambientColor = CreateGradient(new Color(0.05f, 0.06f, 0.1f), new Color(0.8f, 0.85f, 0.9f));

    [Header("Fog")]
    public Gradient fogColor = CreateGradient(new Color(0.03f, 0.03f, 0.05f), new Color(0.7f, 0.84f, 0.95f));
    public AnimationCurve fogDensity = AnimationCurve.EaseInOut(0f, 0.02f, 1f, 0.003f);
    public FogMode fogMode = FogMode.ExponentialSquared;

    [Header("Misc")]
    [Tooltip("Optional skybox override; when provided a runtime instance will be created so editing values does not leak to shared assets.")]
    public Material overrideSkybox;

    public float SecondsPerFullDay => Mathf.Max(1f, dayLengthMinutes * 60f);

    private void OnValidate()
    {
        dayLengthMinutes = Mathf.Max(0.1f, dayLengthMinutes);
        defaultStartTime = Mathf.Repeat(defaultStartTime, 1f);
        EnsureGradient(ref skyColor, new Color(0.02f, 0.05f, 0.12f), new Color(0.7f, 0.85f, 1f));
        EnsureGradient(ref sunLightColor, new Color(0.87f, 0.68f, 0.55f), Color.white);
        EnsureGradient(ref moonLightColor, new Color(0.2f, 0.2f, 0.35f), new Color(0.5f, 0.5f, 0.7f));
        EnsureGradient(ref ambientColor, new Color(0.05f, 0.06f, 0.1f), new Color(0.8f, 0.85f, 0.9f));
        EnsureGradient(ref fogColor, new Color(0.03f, 0.03f, 0.05f), new Color(0.7f, 0.84f, 0.95f));
        fogDensity ??= AnimationCurve.EaseInOut(0f, 0.02f, 1f, 0.003f);
        sunLightIntensity ??= AnimationCurve.EaseInOut(0f, 0.05f, 1f, 1.1f);
        moonLightIntensity ??= AnimationCurve.EaseInOut(0f, 0.8f, 1f, 0.05f);
        skyboxExposure ??= AnimationCurve.EaseInOut(0f, 0.4f, 1f, 1f);
    }

    private static Gradient CreateGradient(Color start, Color end)
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(Color.Lerp(start, end, 0.5f), 0.5f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(end.a, 1f)
            });
        return gradient;
    }

    private static void EnsureGradient(ref Gradient gradient, Color start, Color end)
    {
        if (gradient != null && gradient.colorKeys != null && gradient.colorKeys.Length > 0)
            return;
        gradient = CreateGradient(start, end);
    }
}
