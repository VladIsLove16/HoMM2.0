using System;
using TMPro;
using UnityEngine;
using Zenject;

public class UnitViewUI : MonoBehaviour
{
    [SerializeField] UnitHealthBar healthBar;
    [SerializeField] TextMeshProUGUI AmountText;
    private UnitViewModel _vm;

    public void Init(UnitViewModel vm)
    {
        _vm = vm;

        _vm.OnTakeDamage += OnTakeDamage;
        _vm.OnOutDamage += OnAttack;
        _vm.OnDeath += OnDeath;
        _vm.OnTurnStart += OnTurnStart;

        healthBar.Init();
        SetHealthRatio((float)_vm.Health / _vm.MaxHealth);
        SetAmount(vm.Amount);
    }

    private void OnTurnStart()
    {
        throw new NotImplementedException();
    }

    private void OnDeath()
    {
        throw new NotImplementedException();
    }

    private void OnAttack(DamageContext context)
    {
        throw new NotImplementedException();
    }

    private void OnTakeDamage(int obj)
    {
        throw new NotImplementedException();
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
