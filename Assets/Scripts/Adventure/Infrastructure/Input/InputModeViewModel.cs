using System;
using System.Collections.Generic;
using System.Linq;

public sealed class InputModeViewModel : IInputModeService
{
    private readonly Stack<InputMode> _modes = new();
    public event Action<InputMode> OnModeChanged;
    public InputMode Current => _modes.Count > 0 ? _modes.Peek() : InputMode.Enabled;
    public bool CanMove => Current is InputMode.Enabled or InputMode.OnlyMove;
    public bool CanLook => Current is InputMode.Enabled or InputMode.OnlyLook;
    public bool IsCursorVisible => Current is InputMode.Blocked or InputMode.OnlyMove;
    public void PushMode(InputMode mode)
    {
        _modes.Push(mode);
        OnModeChanged?.Invoke(Current);
    }

    public void PopMode(InputMode mode)
    {
        if (_modes.Contains(mode))
        {
            var remaining = _modes.Where(m => m != mode).ToList();
            _modes.Clear();
            foreach (var m in remaining)
                _modes.Push(m);

            OnModeChanged?.Invoke(Current);
        }
    }
}
