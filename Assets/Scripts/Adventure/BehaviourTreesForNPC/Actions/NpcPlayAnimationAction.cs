using System;
using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.State;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static Adventure.Infrastructure.Dialog.NpcAnimationController;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NPC Play Animation", story: "Switches the NPC animator [npcControlller] to the [NpcAnimationType] state", category: "Action", id: "5d24a4f3b2df4f79be8b4d835f4291a7")]
public sealed partial class NpcPlayAnimationAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcAnimationController> NpcControlller;
    [SerializeReference] public BlackboardVariable<NpcAnimationType> NpcAnimationType;
    [SerializeReference] public BlackboardVariable<NpcAnimationController> Controller;
    [SerializeReference] public BlackboardVariable<bool> WaitForCompletion;

    private bool _isWaitingForEvent;
    private bool _isCompleted;

    protected override Status OnStart()
    {
        _isCompleted = false;

        var controller = Controller?.Value;
        if (controller == null)
        {
            Debug.LogWarning("[PlayNpcAnimationAction] Controller is not assigned.");
            return Status.Failure;
        }

        _isWaitingForEvent = WaitForCompletion != null && WaitForCompletion.Value;

        controller.PlayAnimation(NpcAnimationType, _isWaitingForEvent ? OnSequenceFinished : null);

        if (!_isWaitingForEvent)
        {
            _isCompleted = true;
        }

        return _isWaitingForEvent ? Status.Running : Status.Success;
    }

    protected override Status OnUpdate()
    {
        return _isCompleted ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        _isWaitingForEvent = false;
        _isCompleted = false;
    }

    private void OnSequenceFinished()
    {
        _isCompleted = true;
    }
}
