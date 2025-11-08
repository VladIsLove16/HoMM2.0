using System;

namespace Adventure.Infrastructure.Persistence
{
    [Serializable]
    public sealed class GameSettingsSaveData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 0.8f;
        public float EffectsVolume = 0.8f;

        public int QualityLevel = 0;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;

        public bool Fullscreen = true;

        public AnimationSpeedMode AnimationSpeed = AnimationSpeedMode.Normal;
    }
}
