using UniRx;

public interface IAnimationSpeedSettings
{
    IReadOnlyReactiveProperty<AnimationSpeedMode> Mode { get; }
    float PlaybackMultiplier { get; }
    bool IsInstant { get; }
    void SetMode(AnimationSpeedMode mode);
}

