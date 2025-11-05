using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct ActionContext : INetworkSerializable
{
    public Vector2Int FromCell;
    public Vector2Int TargetCell;
    public Vector2Int AttackFromCell;
    public SpellType AbilityUsed;

    public ActionContext(Vector2Int fromCell, Vector2Int targetCell, SpellType abilityUsed, Vector2Int attackFromCell)
    {
        FromCell = fromCell;
        TargetCell = targetCell;
        AbilityUsed = abilityUsed;
        AttackFromCell = attackFromCell;
    }

    public ActionContext(ActionContext other)
    {
        FromCell = other.FromCell;
        TargetCell = other.TargetCell;
        AbilityUsed = other.AbilityUsed;
        AttackFromCell = other.AttackFromCell;
    }

    public override string ToString()
    {
        string spellInfo = AbilityUsed == SpellType.None
            ? "no spell"
            : $"with spell {AbilityUsed}";
        return $"From {FromCell} to {TargetCell} attackFrom {AttackFromCell} {spellInfo}";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref FromCell);
        serializer.SerializeValue(ref TargetCell);
        serializer.SerializeValue(ref AttackFromCell);
        serializer.SerializeValue(ref AbilityUsed);
    }
}
