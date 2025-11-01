using System;

public interface IBattleAnimationGate
{
    bool IsLocked { get; }
    IDisposable Acquire();
    event Action<bool> LockStateChanged;
}
