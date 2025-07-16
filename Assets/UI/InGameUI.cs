using System;
using TMPro;
using UnityEngine;
using Zenject;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    [Inject] UnitTurnPanelViewModel UnitTurnPanelViewModel;
    private void Start()
    {
        UnitTurnPanelViewModel.TurnNumberChanged += OnTurnNumberChanged;
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
