using System.Collections.Generic;
using UnityEngine;

namespace Tests.Common
{
    public sealed class TestGridRenderSettings : IGridRenderSettings
    {
        public float CellSize { get; set; } = 1f;
        public float CellHeight { get; set; } = 1f;
        public float CellPadding { get; set; } = 0.1f;
        public IReadOnlyList<CellMaterial> Materials { get; set; } = new List<CellMaterial>
        {
            new CellMaterial(CellState.normal, CreateMaterial("Normal"))
        };

        private static Material CreateMaterial(string name)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            return new Material(shader) { name = name };
        }
    }
}
