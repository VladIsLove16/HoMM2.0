using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Log", story: "Hello from behavior graph on [NPCName]", category: "Action", id: "ad40779889f2393c62322fc5356a3d43")]
public partial class LogAction : Action
{
    [SerializeReference] public BlackboardVariable<string> NPCName;

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

