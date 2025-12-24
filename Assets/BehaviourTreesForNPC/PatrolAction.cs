using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "Patroling to [A] or [B]", category: "Action", id: "869fdfbeac2aa2df97e0ec444cf30c38")]
public partial class PatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> A;
    [SerializeReference] public BlackboardVariable<Transform> B;

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

