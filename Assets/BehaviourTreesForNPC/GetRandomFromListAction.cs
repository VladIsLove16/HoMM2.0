using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandomFromList", story: "Set [value] to random one from [list]", category: "Action", id: "94e5f8c17377525e6fb8088ac3fc7002")]
public partial class GetRandomFromListAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Value;
    [SerializeReference] public BlackboardVariable<List<GameObject>> List;

    protected override Status OnStart()
    {
        int index = Random.Range(0, List.Value.Count);
        Value.Value = List.Value[index].transform;
        return Status.Running;
    }

}

