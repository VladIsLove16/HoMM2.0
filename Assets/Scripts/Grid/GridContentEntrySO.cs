using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Game/Grid Unit Content/Entry",
    fileName = "GridContentEntrySO"
)]
public class GridContentEntrySO : ScriptableObject
{
    [SerializeField] int width;
    [SerializeField] int height;
    public int Width => width;
    public int Height => height;
    [Serializable]
    public class UnitContent
    {
        public UnitType unitType;
        public int Amount;
        public int X;
        public int Y;
        public Team Team;
    }
    public List<UnitContent> contents;
}
