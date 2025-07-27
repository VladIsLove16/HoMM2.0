//using System;
//using UnityEngine;

//public class UnitConsoleView
//{
//    private UnitViewModel _vm;
//    private void Construct(UnitViewModel unit)
//    {
//        this._vm = unit;
//        _vm.HealthChanged += HealthChanged;
//        _vm.OnOutDamage += Attacked;
//        _vm.OnDeath += OnDeath;
//        _vm.TurnStarted += TurnStarted;
//    }

//    private void Attacked(EffectReactionContext context)
//    {
//        var source = context.Source;
//        var target = context.TargetObject;

//        string sourceDescription = source.ToString();

//        string targetDescription = target.ToString();
       
//        Debug.Log(sourceDescription + " dealing to " + targetDescription + " " + context.Amount + " Damage");
//    }

//    private string GetDescription(IGridCell gridContent)
//    {
//        return "";
//        //foreach (IGridContent content in gridContent.Contents)
//        //{
//        //    description += content.GetDescription() + ", ";
//        //}
//        //return description;
//    }
//    private void HealthChanged(int dmg)
//    {
//        Debug.Log($"{_vm.GetDescription()} get {dmg} Damage");
//    }

//    private void OnDeath()
//    {
//        Debug.Log($"{_vm.GetDescription()} is DEAD!");
//    }

//    private void TurnStarted()
//    {
//        Debug.Log($"{_vm.GetDescription()} takes a turn");
//    }
//}