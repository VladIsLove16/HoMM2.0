using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

public class CustomOnScreenControl : OnScreenStick
{
    private void Update()
    {
        SendValueToControl(Vector2.up);
    }
}
