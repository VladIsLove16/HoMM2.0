using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Add float to float ", story: "Add [AmountPerSecond] to [Value] each second", category: "Action", id: "b32c6fe0d0f8920add577079c0f6926d")]
public partial class AddFloatToFloatAction : Action
{
    [SerializeReference] public BlackboardVariable<float> AmountPerSecond;
    [SerializeReference] public BlackboardVariable<float> Value;
    protected override Status OnUpdate()
    {
        Value.Value += (Time.deltaTime * AmountPerSecond.Value);
        return Status.Success;
    }
}

