using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UnitViewUI : MonoBehaviour
{
    [SerializeField] UnitHealthBar healthBar;
    [SerializeField] TextMeshProUGUI AmountText;
    private UnitViewModel _unitvViewModel;
  
    public void Init(UnitViewModel vm)
    {
        _unitvViewModel = vm;

        _unitvViewModel.HealthChanged += OnHealthChanged;
        _unitvViewModel.Died += OnDeath;
        _unitvViewModel.TurnStarted += OnTurnStart;

        healthBar.Init();
        SetHealthRatio((float)_unitvViewModel.UnitState.Health / _unitvViewModel.UnitState.MaxHealth);
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
        SetHealthRatio((float)_unitvViewModel.UnitState.Health / _unitvViewModel.UnitState.MaxHealth);
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
