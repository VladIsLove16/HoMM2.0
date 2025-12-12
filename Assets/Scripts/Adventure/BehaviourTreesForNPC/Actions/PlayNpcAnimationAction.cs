using System;
using Adventure.Infrastructure.Dialog;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using static Adventure.Infrastructure.Dialog.NpcBattleAnimationController;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "NPC Play Animation",
    story: "Switches the NPC animator to the requested state",
    category: "Adventure/NPC",
    id: "5d24a4f3b2df4f79be8b4d835f4291a7")]
public sealed partial class PlayNpcAnimationAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcBattleAnimationController> Controller;
    [SerializeReference] public BlackboardVariable<NpcAnimationType> Animation;
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

        var animationType = Animation != null
            ? Animation.Value
            : NpcAnimationType.Greeting;

        _isWaitingForEvent = WaitForCompletion != null && WaitForCompletion.Value;

        controller.PlayAnimation(animationType, _isWaitingForEvent ? OnSequenceFinished : null);

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
