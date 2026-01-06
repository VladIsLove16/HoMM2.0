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

    [SerializeField] private float maxDistance = 8f;
    [SerializeField] private float horizontalFovDegrees = 45f;
    [SerializeField] private float verticalFovDegrees = 20f;
    [SerializeField] private int horizontalSamples = 5;
    [SerializeField] private int verticalSamples = 3;

    public override bool IsTrue()
    {
        var npc = NPC?.Value;
        if (npc == null)
            return false;

        var mask = seeingMusk?.Value ?? 0;
        if (mask == 0)
            return false;

        var origin = npc.transform.position + Vector3.up * 1.6f;
        var forward = npc.transform.forward.sqrMagnitude > Mathf.Epsilon ? npc.transform.forward.normalized : npc.transform.parent?.forward ?? Vector3.forward;

        // First, try straight ahead for early success
        if (CastRay(origin, forward, mask, out var hit))
        {
            target.Value = hit.transform;
            return true;
        }

        int hSamples = Mathf.Max(1, horizontalSamples);
        int vSamples = Mathf.Max(1, verticalSamples);

        for (int v = 0; v < vSamples; v++)
        {
            float vNormalized = vSamples == 1 ? 0f : (v / (float)(vSamples - 1) - 0.5f);
            float pitch = vNormalized * verticalFovDegrees;
            var pitchRotation = Quaternion.AngleAxis(pitch, npc.transform.right);

            for (int h = 0; h < hSamples; h++)
            {
                if (v == 0 && h == hSamples / 2) // central ray already tested
                    continue;

                float hNormalized = hSamples == 1 ? 0f : (h / (float)(hSamples - 1) - 0.5f);
                float yaw = hNormalized * horizontalFovDegrees;
                var yawRotation = Quaternion.AngleAxis(yaw, Vector3.up);
                var direction = yawRotation * pitchRotation * forward;

                if (CastRay(origin, direction, mask, out hit))
                {
                    target.Value = hit.transform;
                    return true;
                }
            }
        }

        return false;
    }

    private bool CastRay(Vector3 origin, Vector3 direction, int mask, out RaycastHit hit)
    {
        return Physics.Raycast(origin, direction, out hit, maxDistance, mask, QueryTriggerInteraction.Ignore);
    }
}
