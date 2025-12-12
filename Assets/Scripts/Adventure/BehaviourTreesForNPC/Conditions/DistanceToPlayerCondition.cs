using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Movement;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Distance to player", story: "[Player] must be [closerDistance] closer from [NPC] .", category: "Conditions", id: "786a19c5ff5428957a0b2fa40a0688ea")]
public partial class DistanceToPlayerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> NPC;
    [SerializeReference] public BlackboardVariable<Transform> Player;
    [SerializeReference] public BlackboardVariable<float> closerDistance;
    public override bool IsTrue()
    {
        Vector3 playerPos= Player.Value. position;
        Vector3 npcPos= NPC.Value.position;
        float dist = (playerPos - npcPos).magnitude; 
        return dist < closerDistance;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
