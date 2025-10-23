using Adventure.Infrastructure.Cursor;
using TMPro;
using UnityEngine;
using Zenject;

public class AdventureDevTools : MonoBehaviour
{
    [Inject] CursorView cursorViewModel;
    [Inject] InputModeViewModel inputModeViewModel;
    [SerializeField] TextMeshProUGUI currentInputMode;
    private void Update()
    {
        currentInputMode.text = inputModeViewModel.Current.ToString();
    }

    [ContextMenu("Lock")]
    public void Lock()
    {
        cursorViewModel.Lock();
    }
    [ContextMenu("Unlock")]
    public void Unlock()
    {
        cursorViewModel.Unlock();
    }
}