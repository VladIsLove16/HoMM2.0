using System;

public interface IInputModeService
{
    InputMode Current { get; }
    event Action<InputMode> OnModeChanged;
    void PushMode(InputMode mode);
    void PopMode(InputMode mode);
    bool CanMove { get; }
    bool IsCursorVisible { get; }
}
