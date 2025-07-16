//using System;
//using UnityEngine;

//public class UnitConsoleView
//{
//    private UnitViewModel unitViewModel;
//    private void Construct(UnitViewModel unit)
//    {
//        this.unitViewModel = unit;
//        unitViewModel.HealthChanged += HealthChanged;
//        unitViewModel.OnOutDamage += Attacked;
//        unitViewModel.OnDeath += OnDeath;
//        unitViewModel.TurnStarted += TurnStarted;
//    }

//    private void Attacked(EffectReactionContext context)
//    {
//        var source = context.Source;
//        var target = context.Target;

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
//        Debug.Log($"{unitViewModel.GetDescription()} get {dmg} Damage");
//    }

//    private void OnDeath()
//    {
//        Debug.Log($"{unitViewModel.GetDescription()} is DEAD!");
//    }

//    private void TurnStarted()
//    {
//        Debug.Log($"{unitViewModel.GetDescription()} takes a turn");
//    }
//}