using Adventure.Infrastructure.Cursor;
using TMPro;
using UnityEngine;
using Zenject;

public class AdventureDevTools : MonoBehaviour
{
    [Inject] CursorView cursorViewModel;
    [Inject] InputModeViewModel inputModeViewModel;
    [SerializeField] TextMeshProUGUI currentInputMode;
    private bool _missingDependenciesLogged;
    private void Update()
    {
        if (cursorViewModel == null || inputModeViewModel == null)
        {
            if (!_missingDependenciesLogged)
            {
                Debug.LogWarning("AdventureDevTools dependencies are not injected.");
                _missingDependenciesLogged = true;
            }
            return;
        }

        if (currentInputMode == null)
        {
            if (!_missingDependenciesLogged)
            {
                Debug.LogWarning("AdventureDevTools is missing a TextMeshProUGUI reference for currentInputMode.");
                _missingDependenciesLogged = true;
            }
            return;
        }
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
