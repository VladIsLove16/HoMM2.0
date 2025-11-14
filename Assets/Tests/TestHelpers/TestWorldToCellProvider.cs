using System.Collections.Generic;
using UnityEngine;

namespace Tests.Common
{
    /// <summary>
    /// Minimal world/cell converter for edit/play mode tests.
    /// </summary>
    public sealed class TestWorldToCellProvider : IWorldToCellProvider
    {
        public Vector3 ToWorld(int x, int y) => new Vector3(x, 0f, y);

        public bool ToGrid(Vector3 position, out Vector2Int coords)
        {
            coords = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            return true;
        }

        public bool ToGridPair(Vector3 position, out KeyValuePair<Vector2Int, Vector2Int> coords)
        {
            var cell = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
            coords = new KeyValuePair<Vector2Int, Vector2Int>(cell, cell);
            return true;
        }
    }
}
