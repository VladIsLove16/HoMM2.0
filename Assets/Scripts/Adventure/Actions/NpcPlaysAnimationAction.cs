using Adventure.Infrastructure.Dialog;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "npc plays animation", story: "npc plays animation [animationType]", category: "Action", id: "5fea234caa7dbb43a58c4a0837c69f34")]
public partial class NpcPlaysAnimationAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcAnimationType> AnimationType;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

