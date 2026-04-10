using UniRx;
using System;

public interface IAnimationSpeedSettings
{
    IReadOnlyReactiveProperty<AnimationSpeedMode> Mode { get; }
    IObservable<Unit> Changed { get; }
    float PlaybackMultiplier { get; }
    float MovementSpeedScale { get; }
    float MovementMultiplier { get; }
    bool IsInstant { get; }
    void SetMode(AnimationSpeedMode mode);
    IDisposable PushPlaybackOverride(float playbackMultiplier, bool isInstant = false);
}

