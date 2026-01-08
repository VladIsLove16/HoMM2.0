using Adventure.Infrastructure.Dialog;
using log4net;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NPC Look At Target", story: "Rotates the [NPC] towards the [Target]", category: "Action", id: "f73a7555c1aa4211a5a2d8ad07a4477f")]
public sealed partial class NpcLookAtTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<NpcAnimationController> NPC;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<bool> RestrictToYaw;

    private const float FinishedAngleThreshold = 1f;

    protected override Status OnUpdate()
    {
        NPC.Value.SetWalking(false);
        Debug.Log("[LookAtPlayerAction] OnUpdate");
        var npc = NPC?.Value.transform;
        var target = Target?.Value;

        if (npc == null || target == null)
        {
            Debug.LogWarning("[LookAtPlayerAction] Missing NPC or Target transform");
            return Status.Failure;
        }

        var direction = target.position - npc.position;
        if (direction.sqrMagnitude < Mathf.Epsilon)
        {
            return Status.Success;
        }

        // Always restrict rotation to yaw (no pitch/roll), regardless of RestrictToYaw flag.
        direction.y = 0f;

        var desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var speed = Mathf.Max(1f, RotationSpeed != null ? RotationSpeed.Value : 360f);
        npc.rotation = Quaternion.RotateTowards(npc.rotation, desiredRotation, speed * Time.deltaTime);

        var remainingAngle = Quaternion.Angle(npc.rotation, desiredRotation);
        return remainingAngle <= FinishedAngleThreshold ? Status.Success : Status.Running;
    }
}
