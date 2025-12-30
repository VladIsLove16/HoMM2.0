using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetDeltaTime", story: "DeltaTime [delta]", category: "Variables", id: "5e148efe23d56d74ffd9a01a4723f302")]
public partial class GetDeltaTimeAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Delta;

    protected override Status OnStart()
    {
        Delta.Value = Time.deltaTime;
        return Status.Running;
    }
}

