using Adventure.Settings.Model;
using UnityEngine;

namespace Adventure.Settings.Configuration
{
    public interface IMouseSensitivityService
    {
        float RotationMultiplier { get; }
        float CursorSpeed { get; }
    }

    public sealed class MouseSensitivityService : IMouseSensitivityService
    {
        private const float DefaultCursorSpeed = 1400f;
        private const float MinSensitivity = 0.01f;

        private readonly GameSettingsModel _settingsModel;
        private readonly IMouseSensitivityProfile _profile;

        public MouseSensitivityService(GameSettingsModel settingsModel, IMouseSensitivityProfile profile)
        {
            _settingsModel = settingsModel ?? throw new System.ArgumentNullException(nameof(settingsModel));
            _profile = profile;
        }

        public float RotationMultiplier
        {
            get
            {
                var baseSensitivity = Mathf.Max(MinSensitivity, _settingsModel.Controls.MouseSensitivity);
                if (_profile == null)
                    return baseSensitivity;

                return _profile.EvaluateRotationMultiplier(baseSensitivity);
            }
        }

        public float CursorSpeed
        {
            get
            {
                var baseSensitivity = Mathf.Max(MinSensitivity, _settingsModel.Controls.MouseSensitivity);
                if (_profile == null)
                    return DefaultCursorSpeed * baseSensitivity;

                return _profile.EvaluateCursorSpeed(baseSensitivity);
            }
        }
    }
}
