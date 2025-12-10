using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx.Toolkit;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ReachableCellObjectPool_PlayModeTests
{
    private sealed class TestCellPool : ObjectPool<CellView>
    {
        private readonly CellView _prefab;

        public TestCellPool(CellView prefab)
        {
            _prefab = prefab;
        }

        protected override CellView CreateInstance()
        {
            return Object.Instantiate(_prefab);
        }
    }

    [Test]
    public void ReachableCellGrid_5x5_ObjectPool_vs_Instantiate_Performance()
    {
        const int gridSize = 5;
        const int iterations = 1000;

        var prefabGO = new GameObject("CellViewPrefab");
        prefabGO.AddComponent<MeshRenderer>();
        var cellPrefab = prefabGO.AddComponent<CellView>();

        var pool = new TestCellPool(cellPrefab);

        long withoutPoolTime;
        long withoutPoolMemoryDelta;
        {
            var memBefore = System.GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var cells = new List<CellView>(gridSize * gridSize);
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var cell = Object.Instantiate(cellPrefab);
                        cells.Add(cell);
                    }
                }

                foreach (var cell in cells)
                {
#if UNITY_EDITOR
                    Object.DestroyImmediate(cell.gameObject);
#else
                    Object.Destroy(cell.gameObject);
#endif
                }
            }
            stopwatch.Stop();
            withoutPoolTime = stopwatch.ElapsedMilliseconds;
            withoutPoolMemoryDelta = System.GC.GetTotalMemory(true) - memBefore;
        }

        long withPoolTime;
        long withPoolMemoryDelta;
        {
            var memBefore = System.GC.GetTotalMemory(true);
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var cells = new List<CellView>(gridSize * gridSize);
                for (int x = 0; x < gridSize; x++)
                {
                    for (int y = 0; y < gridSize; y++)
                    {
                        var cell = pool.Rent();
                        cells.Add(cell);
                    }
                }

                foreach (var cell in cells)
                {
                    pool.Return(cell);
                }
            }
            stopwatch.Stop();
            withPoolTime = stopwatch.ElapsedMilliseconds;
            withPoolMemoryDelta = System.GC.GetTotalMemory(true) - memBefore;
        }

        Debug.Log($"ReachableCell 5x5 grid performance: " +
                  $"withoutPool={withoutPoolTime} ms (Δmem={withoutPoolMemoryDelta} bytes), " +
                  $"withPool={withPoolTime} ms (Δmem={withPoolMemoryDelta} bytes)");

#if UNITY_EDITOR
        Object.DestroyImmediate(prefabGO);
#else
        Object.Destroy(prefabGO);
#endif

        Assert.Greater(withoutPoolTime, 0);
        Assert.Greater(withPoolTime, 0);
    }
}
