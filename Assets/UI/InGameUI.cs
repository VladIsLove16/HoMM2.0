using System;
using TMPro;
using UnityEngine;
using Zenject;
using UniRx;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private TextMeshProUGUI currentActionText;
    [SerializeField] private Animator turnNumberAnimator;
    [Inject] UnitTurnPanelViewModel UnitTurnPanelViewModel;
    [Inject] PlayerInputHandler PlayerInputHandler;
    private void Start()
    {
        if (UnitTurnPanelViewModel != null)
        {
            UnitTurnPanelViewModel.TurnNumber.Subscribe(OnTurnNumberChanged);
        }
        PlayerInputHandler.CurrentAction.Skip(1).Subscribe(OnCurrentActionChanged);
    }
    private void OnCurrentActionChanged(IActionHandler action)
    {
        currentActionText.text = action.ToString();
    }

    private void OnTurnNumberChanged(int obj)
    {
        turnNumberAnimator.SetTrigger("turnChanged");
    }
    public void UpdateTurnText()
    {
        turnNumber.text = "Turn " + UnitTurnPanelViewModel.TurnNumber.ToString();
    }
}
