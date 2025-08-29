using System;
using TMPro;
using UnityEngine;
using Zenject;
using UniRx;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentActionText;
    [Inject] UnitTurnPanelViewModel UnitTurnPanelViewModel;
    [Inject] PlayerInputHandler PlayerInputHandler;
    [SerializeField] TurnTextAnimator turnTextAnimator;
    private void Start()
    {
        if (UnitTurnPanelViewModel != null)
        {
            UnitTurnPanelViewModel.TurnNumber.Subscribe(turnTextAnimator.OnTurnNumberChanged);
        }
    }
    private void OnCurrentActionChanged(IActionHandler action)
    {
        currentActionText.text = action.ToString();
    }
}
