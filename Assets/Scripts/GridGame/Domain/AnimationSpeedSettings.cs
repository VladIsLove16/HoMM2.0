using System;
using UniRx;
using UnityEngine;

[CreateAssetMenu(menuName = "GridGame/Animation Speed Settings", fileName = "AnimationSpeedSettings")]
public sealed class AnimationSpeedSettings : ScriptableObject, IAnimationSpeedSettings, IDisposable
{
    private ReactiveProperty<AnimationSpeedMode> _mode = new();
    private readonly Subject<Unit> _changed = new();

    [SerializeField] private AnimationSpeedMode mode = AnimationSpeedMode.Normal;
    [SerializeField, Min(0.01f)] private float normalPlaybackMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float fastPlaybackMultiplier = 2f;
    [SerializeField, Min(0.01f)] private float veryFastPlaybackMultiplier = 4f;
    [SerializeField] private bool veryFastIsInstant = true;

    private float? _overridePlaybackMultiplier;
    private bool? _overrideIsInstant;

    public IReadOnlyReactiveProperty<AnimationSpeedMode> Mode => _mode;
    public IObservable<Unit> Changed => _changed;

    public float PlaybackMultiplier => _mode.Value switch
    {
        _ when _overridePlaybackMultiplier.HasValue => _overridePlaybackMultiplier.Value,
        AnimationSpeedMode.Normal => normalPlaybackMultiplier,
        AnimationSpeedMode.Fast => fastPlaybackMultiplier,
        AnimationSpeedMode.VeryFast => veryFastPlaybackMultiplier,
        _ => 1f
    };

    public bool IsInstant => _overrideIsInstant ?? (_mode.Value == AnimationSpeedMode.VeryFast && veryFastIsInstant);

    private void Awake()
    {
        _mode.SetValueAndForceNotify(mode);
        NotifyChanged();
    }

    public void SetMode(AnimationSpeedMode mode)
    {
        if (_mode.Value == mode)
            return;

        _mode.Value = mode;
        NotifyChanged();
    }

    public IDisposable PushPlaybackOverride(float playbackMultiplier, bool isInstant = false)
    {
        var previousMultiplier = _overridePlaybackMultiplier;
        var previousInstant = _overrideIsInstant;

        _overridePlaybackMultiplier = playbackMultiplier;
        _overrideIsInstant = isInstant;
        NotifyChanged();

        return Disposable.Create(() =>
        {
            _overridePlaybackMultiplier = previousMultiplier;
            _overrideIsInstant = previousInstant;
            NotifyChanged();
        });
    }

    public void Dispose()
    {
        _changed?.Dispose();
        _mode?.Dispose();
    }

    private void OnValidate()
    {
        _mode?.SetValueAndForceNotify(mode);
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        _changed.OnNext(Unit.Default);
    }
}
