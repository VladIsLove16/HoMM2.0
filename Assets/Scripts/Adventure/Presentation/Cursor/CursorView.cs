using UnityEngine;
using UniRx;
using Zenject;

public sealed class CursorView : MonoBehaviour
{
    [Inject] private CursorViewModel _vm;

    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D moveCursor;

    private void Start()
    {
        _vm.CursorState.Subscribe(OnCursorStateChanged).AddTo(this);
        _vm.IsLocked.Subscribe(OnLockChanged).AddTo(this);
    }

    private void OnCursorStateChanged(CursorVisualState state)
    {
        switch (state)
        {
            case CursorVisualState.Hidden:
                Cursor.visible = false;
                break;
            case CursorVisualState.Default:
                Cursor.visible = true;
                Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
                break;
            case CursorVisualState.Attack:
                Cursor.visible = true;
                Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
                break;
            case CursorVisualState.Move:
                Cursor.visible = true;
                Cursor.SetCursor(moveCursor, Vector2.zero, CursorMode.Auto);
                break;
        }
    }

    private void OnLockChanged(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
