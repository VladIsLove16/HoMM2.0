using System;
using System.Threading;

public sealed class BattleAnimationGate : IBattleAnimationGate
{
    private int _lockCount;
    public event Action<bool> LockStateChanged;

    public bool IsLocked => Volatile.Read(ref _lockCount) > 0;

    public IDisposable Acquire()
    {
        Interlocked.Increment(ref _lockCount);
        RaiseChanged();
        return new ReleaseHandle(this);
    }

    private void Release()
    {
        Interlocked.Decrement(ref _lockCount);
        RaiseChanged();
    }

    private void RaiseChanged()
    {
        LockStateChanged?.Invoke(IsLocked);
    }

    private sealed class ReleaseHandle : IDisposable
    {
        private BattleAnimationGate _owner;

        public ReleaseHandle(BattleAnimationGate owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.Release();
        }
    }
}
