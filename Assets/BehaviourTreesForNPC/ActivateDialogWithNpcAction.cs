using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Adventure.Infrastructure.Dialog;
using Adventure.Infrastructure.Movement;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Activate dialog with npc", story: "Activate dialog with npc", category: "Action", id: "c82eb83440a1531faeef4ad460fea9d8")]
public partial class ActivateDialogWindowAction : Action
{
    [SerializeReference] public BlackboardVariable<NpcDialogueTrigger> trigger;
    [SerializeReference] public BlackboardVariable<PlayerMovementController> palyerMovement;
    private Quaternion _cachedBodyRotation;
    private Quaternion _cachedCameraRotation;
    private bool _orientationCached;

    protected override Status OnStart()
    {
        var movement = palyerMovement?.Value;
        var target = trigger?.Value;
        if (movement == null || target == null)
        {
            Debug.LogWarning("[ActivateDialogWindowAction] Missing movement controller or trigger.");
            return Status.Failure;
        }

        CacheOrientation(movement);
        movement.RotateTowards(target.gameObject.transform.position);
        trigger.Value.Interact(new());
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (!_orientationCached)
            return;

        var movement = palyerMovement?.Value;
        if (movement != null)
        {
            movement.RestoreOrientation(_cachedBodyRotation, _cachedCameraRotation);
        }

        _orientationCached = false;
    }

    private void CacheOrientation(PlayerMovementController controller)
    {
        _cachedBodyRotation = controller.BodyRotation;
        _cachedCameraRotation = controller.CameraLocalRotation;
        _orientationCached = true;
    }
}

