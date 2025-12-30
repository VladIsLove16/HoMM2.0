using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Inventory;
using System;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsSeeingMushroom", story: "[NPC] seeing object on layer [seeingMusk], sets it transform to [target]", category: "Conditions", id: "104fcede3acda3d9d0a9e3c7e9653de5")]
public partial class IsSeeingMushroomCondition : Condition
{
    [SerializeReference] public BlackboardVariable<NpcAnimationController> NPC;
    [SerializeReference] public BlackboardVariable<int> seeingMusk;
    [SerializeReference] public BlackboardVariable<Transform> target;
    public override bool IsTrue()
    {
       var colliders = Physics.OverlapSphere(NPC.Value.transform.position, 5f, seeingMusk.Value);
        if (colliders.Length > 0)
            target.Value = colliders[0].transform;
        return colliders.Length > 0;
    }
}
