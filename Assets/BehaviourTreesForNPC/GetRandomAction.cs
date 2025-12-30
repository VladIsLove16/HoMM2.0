using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Random = UnityEngine.Random;
[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GetRandom", story: "Set [value] randomly from [a] to [b]", category: "Action", id: "c3501d11b05c4b2c6cb11fbd6f99c86f")]
public partial class GetRandomAction : Action
{
    [SerializeReference] public BlackboardVariable<float> A;
    [SerializeReference] public BlackboardVariable<float> B;
    [SerializeReference] public BlackboardVariable<float> Value;

    protected override Status OnStart()
    {
        Value.Value = Random.Range(A.Value, B.Value);
        return Status.Success;
    }
}

