using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Log", story: "Hello from behavior graph on [NPCName]", category: "Action", id: "ad40779889f2393c62322fc5356a3d43")]
public partial class LogAction : Action
{
    [SerializeReference] public BlackboardVariable<string> NPCName;

    protected override Status OnStart()
    {
        Debug.Log($"Hello from behavior graph on {NPCName}");
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

