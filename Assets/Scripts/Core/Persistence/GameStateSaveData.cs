using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Infrastructure.Persistence
{
    [Serializable]
    public sealed class GameStateSaveData
    {
        public List<UnitStackRecord> Inventory = new();
        public List<Vector3IntRecord> CollectedMushrooms = new();
        public List<string> ClaimedBattleRewardIds = new();
        public int Currency;
    }

    [Serializable]
    public struct UnitStackRecord
    {
        public UnitType UnitType;
        public int Amount;
    }

    [Serializable]
    public struct Vector3IntRecord
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3IntRecord(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3Int ToVector3Int() => new Vector3Int(X, Y, Z);

        public static Vector3IntRecord From(Vector3Int value)
        {
            return new Vector3IntRecord(value.x, value.y, value.z);
        }
    }
}
