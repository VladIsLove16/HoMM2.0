using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Tests.EditMode.Pathfinding
{
    [TestFixture]
    public class MovementSystem_PathfindingPerformanceTests
    {
        private static GridXZ<GameCell> CreateGrid(int width, int height)
        {
            return new GridXZ<GameCell>(width, height, (grid, x, y) => new GameCell(grid, x, y));
        }

        private static MovementSystem CreateMovementSystem(int width, int height, out GridXZ<GameCell> grid)
        {
            grid = CreateGrid(width, height);
            var movementSystem = new MovementSystem();
            movementSystem.Init(grid);
            return movementSystem;
        }

        private static List<(Vector2Int from, Vector2Int to)> BuildTestPairs(int width, int height)
        {
            var pairs = new List<(Vector2Int from, Vector2Int to)>();

            // deterministic набор старт/финиш пар по сетке
            for (int x = 0; x < width; x += 4)
            {
                var from = new Vector2Int(0, 0);
                var to = new Vector2Int(Mathf.Min(width - 1, x), Mathf.Min(height - 1, height - 1));
                if (from != to)
                {
                    pairs.Add((from, to));
                }
            }

            for (int y = 0; y < height; y += 4)
            {
                var from = new Vector2Int(0, 0);
                var to = new Vector2Int(Mathf.Min(width - 1, width - 1), Mathf.Min(height - 1, y));
                if (from != to)
                {
                    pairs.Add((from, to));
                }
            }

            return pairs;
        }

        private class TestObstacleContent : IGridContent
        {
            public Team Team { get; }
            public Vector2Int Position { get; set; }
            public GridContentType GridContentType { get; }

            public TestObstacleContent(Vector2Int position)
            {
                Team = Team.None;
                Position = position;
                GridContentType = GridContentType.rock;
            }
        }

        private static void ApplyRandomObstacles(
            GridXZ<GameCell> grid,
            float obstacleChance,
            int seed,
            Vector2Int start,
            Vector2Int end)
        {
            var random = new Random(seed);
            int width = grid.GetWidth();
            int height = grid.GetHeight();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var position = new Vector2Int(x, y);

                    // Гарантированный свободный коридор по диагонали от start до end
                    bool onMainPath =
                        x - start.x == y - start.y &&
                        x >= start.x && y >= start.y &&
                        x <= end.x && y <= end.y;

                    if (position == start || position == end || onMainPath)
                        continue;

                    if (random.NextDouble() < obstacleChance)
                    {
                        var cell = grid.GetGridObject(x, y);
                        cell.AddContent(new TestObstacleContent(position));
                    }
                }
            }
        }

        private static long MeasureLegacy(MovementSystem movementSystem, List<(Vector2Int from, Vector2Int to)> pairs, int iterations)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var pair in pairs)
                {
                    var success = movementSystem.FindPathLegacy(pair.from, pair.to, out var route);
                    Assert.IsTrue(success);
                    Assert.IsNotNull(route);
                    Assert.Greater(route.Count, 0);
                }
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static long MeasureDijkstra(MovementSystem movementSystem, List<(Vector2Int from, Vector2Int to)> pairs, int iterations)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var pair in pairs)
                {
                    var success = movementSystem.FindPathDijkstra(pair.from, pair.to, out var route);
                    Assert.IsTrue(success);
                    Assert.IsNotNull(route);
                    Assert.Greater(route.Count, 0);
                }
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static long MeasureAStar(MovementSystem movementSystem, List<(Vector2Int from, Vector2Int to)> pairs, int iterations)
        {
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var pair in pairs)
                {
                    var success = movementSystem.FindPathAStar(pair.from, pair.to, out var route);
                    Assert.IsTrue(success);
                    Assert.IsNotNull(route);
                    Assert.Greater(route.Count, 0);
                }
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private static long MeasureLegacyWithOptions(
            int width,
            int height,
            List<(Vector2Int from, Vector2Int to)> pairs,
            int iterations,
            bool useRouteCache,
            bool useNeighborPrecomputation,
            bool skipReachableCollection)
        {
            var movementSystem = CreateMovementSystem(width, height, out _);

            // небольшое прогревание JIT для одной пары
            var warmupPair = pairs[0];
            movementSystem.FindPathLegacy(
                warmupPair.from,
                warmupPair.to,
                out _,
                int.MaxValue,
                ignoreObstacles: false,
                useRouteCache: useRouteCache,
                useNeighborPrecomputation: useNeighborPrecomputation,
                skipReachableCollection: skipReachableCollection);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                foreach (var pair in pairs)
                {
                    var success = movementSystem.FindPathLegacy(
                        pair.from,
                        pair.to,
                        out var route,
                        int.MaxValue,
                        ignoreObstacles: false,
                        useRouteCache: useRouteCache,
                        useNeighborPrecomputation: useNeighborPrecomputation,
                        skipReachableCollection: skipReachableCollection);

                    Assert.IsTrue(success);
                    Assert.IsNotNull(route);
                    Assert.Greater(route.Count, 0);
                }
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        [Test]
        public void Compare_Pathfinding_Methods_Performance_On_Same_Data()
        {
            const int width = 10;
            const int height = 10;
            const int iterations = 20;

            var movementSystem = CreateMovementSystem(width, height, out _);
            var pairs = BuildTestPairs(width, height);

            // небольшое прогревание JIT
            movementSystem.FindPathLegacy(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathDijkstra(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathAStar(pairs[0].from, pairs[0].to, out _);

            var legacyTime = MeasureLegacy(movementSystem, pairs, iterations);
            var dijkstraTime = MeasureDijkstra(movementSystem, pairs, iterations);
            var aStarTime = MeasureAStar(movementSystem, pairs, iterations);

            Debug.Log($"Pathfinding performance (ms): Legacy={legacyTime}, Dijkstra={dijkstraTime}, AStar={aStarTime}");

            // Дополнительно убеждаемся, что все алгоритмы дают одинаковую стоимость пути на одной паре
            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            movementSystem.FindPathLegacy(start, end, out var legacyRoute);
            movementSystem.FindPathDijkstra(start, end, out var dijkstraRoute);
            movementSystem.FindPathAStar(start, end, out var aStarRoute);

            var legacyCost = movementSystem.GetRouteCost(legacyRoute);
            var dijkstraCost = movementSystem.GetRouteCost(dijkstraRoute);
            var aStarCost = movementSystem.GetRouteCost(aStarRoute);

            Assert.That(dijkstraCost, Is.EqualTo(legacyCost).Within(0.001f));
            Assert.That(aStarCost, Is.EqualTo(legacyCost).Within(0.001f));
        }

        [Test]
        public void Compare_Pathfinding_Methods_Performance_On_Same_Data_20x20()
        {
            const int width = 20;
            const int height = 20;
            const int iterations = 10;

            var movementSystem = CreateMovementSystem(width, height, out _);
            var pairs = BuildTestPairs(width, height);

            movementSystem.FindPathLegacy(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathDijkstra(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathAStar(pairs[0].from, pairs[0].to, out _);

            var legacyTime = MeasureLegacy(movementSystem, pairs, iterations);
            var dijkstraTime = MeasureDijkstra(movementSystem, pairs, iterations);
            var aStarTime = MeasureAStar(movementSystem, pairs, iterations);

            Debug.Log($"Pathfinding performance 20x20 (ms): Legacy={legacyTime}, Dijkstra={dijkstraTime}, AStar={aStarTime}");

            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            movementSystem.FindPathLegacy(start, end, out var legacyRoute);
            movementSystem.FindPathDijkstra(start, end, out var dijkstraRoute);
            movementSystem.FindPathAStar(start, end, out var aStarRoute);

            var legacyCost = movementSystem.GetRouteCost(legacyRoute);
            var dijkstraCost = movementSystem.GetRouteCost(dijkstraRoute);
            var aStarCost = movementSystem.GetRouteCost(aStarRoute);

            Assert.That(dijkstraCost, Is.EqualTo(legacyCost).Within(0.001f));
            Assert.That(aStarCost, Is.EqualTo(legacyCost).Within(0.001f));
        }

        [Test]
        public void Compare_Pathfinding_Methods_Performance_On_Same_Data_40x40()
        {
            const int width = 40;
            const int height = 40;
            const int iterations = 3;

            var movementSystem = CreateMovementSystem(width, height, out _);
            var pairs = BuildTestPairs(width, height);

            movementSystem.FindPathLegacy(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathDijkstra(pairs[0].from, pairs[0].to, out _);
            movementSystem.FindPathAStar(pairs[0].from, pairs[0].to, out _);

            var legacyTime = MeasureLegacy(movementSystem, pairs, iterations);
            var dijkstraTime = MeasureDijkstra(movementSystem, pairs, iterations);
            var aStarTime = MeasureAStar(movementSystem, pairs, iterations);

            Debug.Log($"Pathfinding performance 40x40 (ms): Legacy={legacyTime}, Dijkstra={dijkstraTime}, AStar={aStarTime}");

            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            movementSystem.FindPathLegacy(start, end, out var legacyRoute);
            movementSystem.FindPathDijkstra(start, end, out var dijkstraRoute);
            movementSystem.FindPathAStar(start, end, out var aStarRoute);

            var legacyCost = movementSystem.GetRouteCost(legacyRoute);
            var dijkstraCost = movementSystem.GetRouteCost(dijkstraRoute);
            var aStarCost = movementSystem.GetRouteCost(aStarRoute);

            Assert.That(dijkstraCost, Is.EqualTo(legacyCost).Within(0.001f));
            Assert.That(aStarCost, Is.EqualTo(legacyCost).Within(0.001f));
        }

        [Test]
        public void Compare_Pathfinding_With_Obstacles_10_Percent()
        {
            const int width = 40;
            const int height = 40;
            const int iterations = 10;
            const float obstacleChance = 0.10f;
            const int seed = 12345;

            var movementSystem = CreateMovementSystem(width, height, out var grid);

            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            ApplyRandomObstacles(grid, obstacleChance, seed, start, end);

            var pairs = new List<(Vector2Int from, Vector2Int to)>
            {
                (start, end)
            };

            movementSystem.FindPathLegacy(start, end, out _);
            movementSystem.FindPathDijkstra(start, end, out _);
            movementSystem.FindPathAStar(start, end, out _);

            var legacyTime = MeasureLegacy(movementSystem, pairs, iterations);
            var dijkstraTime = MeasureDijkstra(movementSystem, pairs, iterations);
            var aStarTime = MeasureAStar(movementSystem, pairs, iterations);

            Debug.Log($"Pathfinding performance 40x40 obstacles 10% (ms): Legacy={legacyTime}, Dijkstra={dijkstraTime}, AStar={aStarTime}");

            movementSystem.FindPathLegacy(start, end, out var legacyRoute);
            movementSystem.FindPathDijkstra(start, end, out var dijkstraRoute);
            movementSystem.FindPathAStar(start, end, out var aStarRoute);

            var legacyCost = movementSystem.GetRouteCost(legacyRoute);
            var dijkstraCost = movementSystem.GetRouteCost(dijkstraRoute);
            var aStarCost = movementSystem.GetRouteCost(aStarRoute);

            Assert.That(dijkstraCost, Is.EqualTo(legacyCost).Within(0.001f));
            Assert.That(aStarCost, Is.EqualTo(legacyCost).Within(0.001f));
        }

        [Test]
        public void Compare_Pathfinding_With_Obstacles_30_Percent()
        {
            const int width = 40;
            const int height = 40;
            const int iterations = 10;
            const float obstacleChance = 0.30f;
            const int seed = 12345;

            var movementSystem = CreateMovementSystem(width, height, out var grid);

            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            ApplyRandomObstacles(grid, obstacleChance, seed, start, end);

            var pairs = new List<(Vector2Int from, Vector2Int to)>
            {
                (start, end)
            };

            movementSystem.FindPathLegacy(start, end, out _);
            movementSystem.FindPathDijkstra(start, end, out _);
            movementSystem.FindPathAStar(start, end, out _);

            var legacyTime = MeasureLegacy(movementSystem, pairs, iterations);
            var dijkstraTime = MeasureDijkstra(movementSystem, pairs, iterations);
            var aStarTime = MeasureAStar(movementSystem, pairs, iterations);

            Debug.Log($"Pathfinding performance 40x40 obstacles 30% (ms): Legacy={legacyTime}, Dijkstra={dijkstraTime}, AStar={aStarTime}");

            movementSystem.FindPathLegacy(start, end, out var legacyRoute);
            movementSystem.FindPathDijkstra(start, end, out var dijkstraRoute);
            movementSystem.FindPathAStar(start, end, out var aStarRoute);

            var legacyCost = movementSystem.GetRouteCost(legacyRoute);
            var dijkstraCost = movementSystem.GetRouteCost(dijkstraRoute);
            var aStarCost = movementSystem.GetRouteCost(aStarRoute);

            Assert.That(dijkstraCost, Is.EqualTo(legacyCost).Within(0.001f));
            Assert.That(aStarCost, Is.EqualTo(legacyCost).Within(0.001f));
        }

        [Test]
        public void Legacy_Pathfinding_Optimization_Parameters_Performance()
        {
            const int width = 40;
            const int height = 40;
            const int iterations = 10;

            var pairs = BuildTestPairs(width, height);

            var baseline = MeasureLegacyWithOptions(
                width,
                height,
                pairs,
                iterations,
                useRouteCache: false,
                useNeighborPrecomputation: false,
                skipReachableCollection: false);

            var skipReachable = MeasureLegacyWithOptions(
                width,
                height,
                pairs,
                iterations,
                useRouteCache: false,
                useNeighborPrecomputation: false,
                skipReachableCollection: true);

            var precomputed = MeasureLegacyWithOptions(
                width,
                height,
                pairs,
                iterations,
                useRouteCache: false,
                useNeighborPrecomputation: true,
                skipReachableCollection: true);

            var cached = MeasureLegacyWithOptions(
                width,
                height,
                pairs,
                iterations,
                useRouteCache: true,
                useNeighborPrecomputation: true,
                skipReachableCollection: true);

            Debug.Log(
                $"Legacy pathfinding optimization 40x40 (iterations={iterations}) " +
                $"ms: baseline={baseline}, skipReachable={skipReachable}, precomputed={precomputed}, cached={cached}");

            // Дополнительно проверим, что варианты дают одинаковую стоимость пути
            var movementSystem = CreateMovementSystem(width, height, out _);
            var start = new Vector2Int(0, 0);
            var end = new Vector2Int(width - 1, height - 1);

            movementSystem.FindPathLegacy(start, end, out var baselineRoute);
            movementSystem.FindPathLegacy(
                start,
                end,
                out var skipRoute,
                int.MaxValue,
                ignoreObstacles: false,
                useRouteCache: false,
                useNeighborPrecomputation: false,
                skipReachableCollection: true);
            movementSystem.FindPathLegacy(
                start,
                end,
                out var precomputedRoute,
                int.MaxValue,
                ignoreObstacles: false,
                useRouteCache: false,
                useNeighborPrecomputation: true,
                skipReachableCollection: true);
            movementSystem.FindPathLegacy(
                start,
                end,
                out var cachedRoute,
                int.MaxValue,
                ignoreObstacles: false,
                useRouteCache: true,
                useNeighborPrecomputation: true,
                skipReachableCollection: true);

            var baselineCost = movementSystem.GetRouteCost(baselineRoute);
            var skipCost = movementSystem.GetRouteCost(skipRoute);
            var precomputedCost = movementSystem.GetRouteCost(precomputedRoute);
            var cachedCost = movementSystem.GetRouteCost(cachedRoute);

            Assert.That(skipCost, Is.EqualTo(baselineCost).Within(0.001f));
            Assert.That(precomputedCost, Is.EqualTo(baselineCost).Within(0.001f));
            Assert.That(cachedCost, Is.EqualTo(baselineCost).Within(0.001f));
        }
    }
}
