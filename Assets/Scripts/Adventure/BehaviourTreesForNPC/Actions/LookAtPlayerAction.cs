using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "NPC Look At Target",
    story: "Rotates the NPC towards the provided target transform",
    category: "Adventure/NPC",
    id: "f73a7555c1aa4211a5a2d8ad07a4477f")]
public sealed partial class LookAtPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> NpcTransform;
    [SerializeReference] public BlackboardVariable<Transform> TargetTransform;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed;
    [SerializeReference] public BlackboardVariable<bool> RestrictToYaw;

    private const float FinishedAngleThreshold = 1f;

    protected override Status OnUpdate()
    {
        var npc = NpcTransform?.Value;
        var target = TargetTransform?.Value;

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

        if (RestrictToYaw == null || RestrictToYaw.Value)
        {
            direction.y = 0f;
        }

        var desiredRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var speed = Mathf.Max(1f, RotationSpeed != null ? RotationSpeed.Value : 360f);
        npc.rotation = Quaternion.RotateTowards(npc.rotation, desiredRotation, speed * Time.deltaTime);

        var remainingAngle = Quaternion.Angle(npc.rotation, desiredRotation);
        return remainingAngle <= FinishedAngleThreshold ? Status.Success : Status.Running;
    }
}
