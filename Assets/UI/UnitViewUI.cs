using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UnitViewUI : MonoBehaviour
{
    [SerializeField] UnitHealthBar healthBar;
    [SerializeField] TextMeshProUGUI AmountText;
    private UnitModel _unitModel;
  
    public void Init(UnitModel vm)
    {
        _unitModel = vm;

        _unitModel.HealthChanged += OnHealthChanged;
        _unitModel.Died += OnDeath;
        _unitModel.TurnStarted += OnTurnStart;

        healthBar.Init();
        SetHealthRatio((float)_unitModel.UnitState.Health / _unitModel.UnitState.MaxHealth);
        SetAmount(vm.Amount);
    }

    private void OnTurnStart()
    {
        
    }

    private void OnDeath()
    {
        AmountText.color = Color.black;
    }

    private void OnAttack(EffectReactionContext context)
    {
        
    }

    private void OnHealthChanged(int obj)
    {
        SetHealthRatio((float)_unitModel.UnitState.Health / _unitModel.UnitState.MaxHealth);
    }

    public void SetHealthRatio(float ration)
    {
        healthBar.SetRatio(ration);
    }

    public void SetAmount(int amount)
    {
        AmountText.text = amount.ToString();
    }
}
