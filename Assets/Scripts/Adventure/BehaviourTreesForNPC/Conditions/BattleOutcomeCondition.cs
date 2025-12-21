using Adventure.Infrastructure.State;
using System;
using Unity.Behavior;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[Condition(
    name: "Battle Outcome Equals",
    story: "[Battle outcome] must equal [Expected]",
    category: "Conditions",
    id: "bbefad0fb7c3480d8b13afc3b1919af8")]
public sealed partial class BattleOutcomeCondition : Condition
{
    //[SerializeReference] public BlackboardVariable<BattleOutcome> BattleOutcome;
    //[SerializeReference] public BlackboardVariable<BattleOutcome> Expected;

    public override bool IsTrue()
    {
        //return BattleOutcome != null
        //    && Expected != null
        //    && BattleOutcome.Value == Expected.Value;
        return true;
    }

    //public override void OnStart()
    //{
    //}

    //public override void OnEnd()
    //{
    //}
}
