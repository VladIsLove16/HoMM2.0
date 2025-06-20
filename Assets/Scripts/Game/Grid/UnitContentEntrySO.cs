// UnitContentEntrySO.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Grid Unit Content/Entry",
    fileName = "UnitContentEntrySO"
)]
public class UnitContentEntrySO : ScriptableObject
{
    [Serializable]
    public class UnitContent
    {
        public UnitType unitType;
        public int Amount;
        public int X;
        public int Y;
        public bool isPlayer;
    }
    public List<UnitContent> contents;
}
