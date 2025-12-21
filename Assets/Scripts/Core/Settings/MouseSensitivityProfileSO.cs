using UnityEngine;

namespace Adventure.Settings.Configuration
{
    public interface IMouseSensitivityProfile
    {
        float EvaluateRotationMultiplier(float userSensitivity);
        float EvaluateCursorSpeed(float userSensitivity);
    }

    [CreateAssetMenu(menuName = "Settings/Mouse Sensitivity Profile", fileName = "MouseSensitivityProfile")]
    public sealed class MouseSensitivityProfileSO : ScriptableObject, IMouseSensitivityProfile
    {
        private const float MinSensitivity = 0.01f;

        [Header("User multipliers")]
        [SerializeField, Min(0f)] private float rotationMultiplier = 1f;
        [SerializeField, Min(0f)] private float cursorMultiplier = 1f;

        [Header("Base cursor speed")]
        [SerializeField, Min(0f)] private float baseCursorSpeed = 1400f;

        public float EvaluateRotationMultiplier(float userSensitivity)
        {
            var normalized = Mathf.Max(MinSensitivity, userSensitivity);
            return normalized * Mathf.Max(MinSensitivity, rotationMultiplier);
        }

        public float EvaluateCursorSpeed(float userSensitivity)
        {
            var normalized = Mathf.Max(MinSensitivity, userSensitivity);
            var cursorScale = Mathf.Max(MinSensitivity, cursorMultiplier);
            return baseCursorSpeed * normalized * cursorScale;
        }
    }
}
