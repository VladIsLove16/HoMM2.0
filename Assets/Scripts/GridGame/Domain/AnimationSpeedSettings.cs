using System;
using UniRx;
using UnityEngine;
[CreateAssetMenu(menuName = "GridGame/Animation Speed Settings", fileName = "AnimationSpeedSettings")]
public sealed class AnimationSpeedSettings : ScriptableObject, IAnimationSpeedSettings, IDisposable
{
    private ReactiveProperty<AnimationSpeedMode> _mode = new();
    [SerializeField] private AnimationSpeedMode mode = AnimationSpeedMode.Normal;
    public IReadOnlyReactiveProperty<AnimationSpeedMode> Mode => _mode;
    public float PlaybackMultiplier => _mode.Value switch
    {
        AnimationSpeedMode.Normal => 1f,
        AnimationSpeedMode.Fast => 2f,
        AnimationSpeedMode.VeryFast => 4f,
        _ => 1f
    };
    private void Awake()
    {
        _mode.SetValueAndForceNotify(mode);
    }
    public bool IsInstant => _mode.Value == AnimationSpeedMode.VeryFast;

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
    private void OnValidate()
    {
        _mode?.SetValueAndForceNotify(mode);
    }
}

