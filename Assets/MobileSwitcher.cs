using UnityEngine;
using UnityEngine.InputSystem;

public class MobileUiSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mobileCanvasRoot;

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        RefreshState();
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change is InputDeviceChange.Added or InputDeviceChange.Removed)
            RefreshState();
    }

    void RefreshState()
    {
        var hasTouch = Touchscreen.current != null || Pointer.current is Pen;
        mobileCanvasRoot.SetActive(hasTouch);
    }
}
