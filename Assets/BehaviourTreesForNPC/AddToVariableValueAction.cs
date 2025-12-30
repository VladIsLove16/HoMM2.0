using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AddToVariableValue", story: "Add to [Variable] [Value]", category: "Action", id: "57ae7a71c4d1726ed060bde5ccc060aa")]
public partial class AddToVariableValueAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Variable;
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        Variable.Value += Value.Value;
        return Status.Success;
    }
}

