using System;
using UniRx;

public sealed class AnimationSpeedSettings : IAnimationSpeedSettings, IDisposable
{
    private readonly ReactiveProperty<AnimationSpeedMode> _mode;

    public AnimationSpeedSettings(AnimationSpeedMode defaultMode = AnimationSpeedMode.Normal)
    {
        _mode = new ReactiveProperty<AnimationSpeedMode>(defaultMode);
    }

    public IReadOnlyReactiveProperty<AnimationSpeedMode> Mode => _mode;

    public float PlaybackMultiplier => _mode.Value switch
    {
        AnimationSpeedMode.Normal => 1f,
        AnimationSpeedMode.Fast => 2f,
        AnimationSpeedMode.Instant => float.PositiveInfinity,
        _ => 1f
    };

    public bool IsInstant => _mode.Value == AnimationSpeedMode.Instant;

    public void SetMode(AnimationSpeedMode mode)
    {
        if (_mode.Value == mode)
            return;
        _mode.Value = mode;
    }

    public void Dispose()
    {
        _mode?.Dispose();
    }
}

