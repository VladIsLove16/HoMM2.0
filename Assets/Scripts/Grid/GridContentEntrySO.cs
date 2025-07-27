using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Grid Unit Content/Entry",
    fileName = "GridContentEntrySO"
)]
public class GridContentEntrySO : ScriptableObject
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
