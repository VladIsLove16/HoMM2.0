using System;
using System.Collections.Generic;
using System.Linq;

public sealed class InputModeViewModel : IInputModeVM
{
    private readonly Stack<InputMode> _modes = new();
    public event Action<InputMode> OnModeChanged;
    public InputMode Current => _modes.Count > 0 ? _modes.Peek() : InputMode.Enabled;
    public bool CanMove => Current is InputMode.Enabled or InputMode.OnlyMove;
    public bool CanLook => Current is InputMode.Enabled or InputMode.OnlyLook;
    public bool IsCursorVisible => Current is InputMode.Blocked or InputMode.OnlyMove;
    public void PushMode(InputMode mode)
    {
        _modes.Push( mode);
        UnityLogger.Log("new input " + mode);
        OnModeChanged?.Invoke(Current);
    }

    public void PopMode(InputMode mode)
    {
        if (_modes.Count == 0 || !_modes.Contains(mode))
            return;

        var snapshot = _modes.ToArray(); // top-to-bottom
        _modes.Clear();

        var removed = false;
        for (var i = snapshot.Length - 1; i >= 0; i--)
        {
            var current = snapshot[i];
            if (!removed && current == mode)
            {
                removed = true;
                continue;
            }

            _modes.Push(current);
        }

        UnityLogger.Log("new input " + Current);
        OnModeChanged?.Invoke(Current);
    }
}
