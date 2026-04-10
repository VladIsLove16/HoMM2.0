using Adventure.Domain.Dialog;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Target choses start battle", story: "Target choses start battle? [yesorno]", category: "Conditions", id: "02e3d4c2955a61d92bde780d4f841f1a")]
public partial class PlayerChosesStartBattleCondition : Condition
{
    [SerializeReference] public BlackboardVariable<DialogueChoiceAction> Yesorno;

    public override bool IsTrue()
    {
        return Yesorno == DialogueChoiceAction.StartBattle || Yesorno == DialogueChoiceAction.StartOnlineBattle;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
