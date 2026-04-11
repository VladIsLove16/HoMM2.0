using System;
using Adventure.Infrastructure.Dialog;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopTalking", story: "Npc stops talking with player", category: "Action", id: "3c358f89502b1528c54dad7d991b3552")]
public partial class StopTalkingAction : Action
{
    private bool _requestedStop;

    protected override Status OnStart()
    {
        _requestedStop = false;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_requestedStop)
        {
            return Status.Success;
        }

        var trigger = GameObject != null ? GameObject.GetComponent<NpcDialogueTrigger>() : null;
        if (trigger == null)
        {
            Debug.LogWarning("[StopTalkingAction] NpcDialogueTrigger is missing on behavior agent object.", GameObject);
            return Status.Failure;
        }

        _requestedStop = trigger.TryStopCurrentDialogueFromNpc();
        return _requestedStop ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

