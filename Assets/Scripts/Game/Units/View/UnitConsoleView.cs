using System;
using UnityEngine;

public class UnitConsoleView
{
    private UnitViewModel unitViewModel;
    private void Construct(UnitViewModel unit)
    {
        this.unitViewModel = unit;
        unitViewModel.OnTakeDamage += OnTakeDamage;
        unitViewModel.OnOutDamage += OnAttack;
        unitViewModel.OnDeath += OnDeath;
        unitViewModel.OnTurnStart += OnTurnStart;
    }

    private void OnAttack(DamageContext context)
    {
        IDescriptable source = context.Source as IDescriptable;
        string sourceDescription = source != null ? source.GetDescription() : context.Source.ToString();

        IDescriptable target = context.Target as IDescriptable;
        string targetDescription = source != null ? target.GetDescription() : context.Target.ToString();
       
        Debug.Log(sourceDescription + " dealing to " + targetDescription + " " + context.Amount + " Damage");
    }

    private void OnTakeDamage(int dmg)
    {
        Debug.Log($"{unitViewModel.GetDescription()} get {dmg} Damage");
    }

    private void OnDeath()
    {
        Debug.Log($"{unitViewModel.GetDescription()} is DEAD!");
    }

    private void OnTurnStart()
    {
        Debug.Log($"{unitViewModel.GetDescription()} takes a turn");
    }
}