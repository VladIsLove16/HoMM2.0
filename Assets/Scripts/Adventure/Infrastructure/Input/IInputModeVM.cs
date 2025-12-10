using System;

public interface IInputModeVM
{
    InputMode Current { get; }
    event Action<InputMode> OnModeChanged;
    void PushMode(InputMode mode);
    void PopMode(InputMode mode);

    bool CanMove { get; }
    bool CanLook { get; }
    bool IsCursorVisible { get; }
}
