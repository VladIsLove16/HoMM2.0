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

        _unitModel.OnHealthChanged += OnHealthChanged;
        _unitModel.OnDeath += OnDeath;
        _unitModel.OnTurnStart += OnTurnStart;

        healthBar.Init();
        SetHealthRatio((float)_unitModel.UnitStats.Health / _unitModel.UnitStats.MaxHealth);
        SetAmount(vm.Amount);
    }

    private void OnTurnStart()
    {
        
    }

    private void OnDeath()
    {
        AmountText.color = Color.black;
    }

    private void OnAttack(DamageContext context)
    {
        
    }

    private void OnHealthChanged(int obj)
    {
        SetHealthRatio((float)_unitModel.UnitStats.Health / _unitModel.UnitStats.MaxHealth);
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
