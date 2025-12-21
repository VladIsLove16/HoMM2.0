using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Profiling;
using Zenject;

public sealed class BattleMemoryLeak_PlayModeTests
{
    [UnityTest]
    public IEnumerator BattleMemoryLeakTester_MultipleLoadsAndBattles_LogMemoryStats()
    {
        var stats = new MemoryStats();
        var adventureScene = SceneLoader.Scene.Adventure.ToString();
        var gridScene = SceneLoader.Scene.GridFight.ToString();

        yield return LoadSceneAndMeasure(adventureScene, "[Initial Adventure]", stats, countTowardsSummary: false);
        stats.SetBaselineToLast();

        const int sceneLoads = 7;
        const int turnsPerBattle = 5;
        var adventureManagedDeltas = new List<long>();
        var adventureTotalDeltas = new List<long>();

        for (int i = 0; i < sceneLoads; i++)
        {
            yield return LoadSceneAndMeasure(gridScene, $"Grid load #{i + 1}", stats);

            var sceneContext = Object.FindObjectOfType<SceneContext>();
            Assert.IsNotNull(sceneContext, $"SceneContext was not found after grid load #{i + 1}.");

            var gameController = sceneContext.Container.Resolve<GameController>();
            var turnService = sceneContext.Container.Resolve<ITurnService>();

            Assert.IsNotNull(gameController, "GameController was not resolved from the scene container.");
            Assert.IsNotNull(turnService, "ITurnService was not resolved from the scene container.");

            var testerGO = new GameObject($"BattleMemoryLeakTester_PlayMode_{i + 1}");
            var tester = testerGO.AddComponent<BattleMemoryLeakTester>();
            tester.Construct(gameController, turnService);

            yield return tester.RunLeakCheck(
                battles: 1,
                turnsPerBattle: turnsPerBattle,
                (index, managedDelta, totalDelta) =>
                {
                    stats.RecordBattle($"Battle {i + 1}", managedDelta, totalDelta);
                });

#if UNITY_EDITOR
            Object.DestroyImmediate(testerGO);
#else
            Object.Destroy(testerGO);
#endif

            yield return LoadSceneAndMeasure(adventureScene, $"Adventure return #{i + 1}", stats, countTowardsSummary: false);
            var managedReturnDelta = stats.LastManaged - stats.BaselineManaged;
            var totalReturnDelta = stats.LastTotal - stats.BaselineTotal;
            stats.RecordAdventureReturn($"Adventure return #{i + 1}", managedReturnDelta, totalReturnDelta);
            adventureManagedDeltas.Add(managedReturnDelta);
            adventureTotalDeltas.Add(totalReturnDelta);
        }

        var finalManaged = stats.LastManaged;
        var finalTotal = stats.LastTotal;
        var managedDeltaTotal = finalManaged - stats.BaselineManaged;
        var totalDeltaTotal = finalTotal - stats.BaselineTotal;

        for (int i = 0; i < adventureManagedDeltas.Count; i++)
        {
            var battleIndex = i + 1;
            var managedDelta = adventureManagedDeltas[i];
            var totalDelta = i < adventureTotalDeltas.Count ? adventureTotalDeltas[i] : 0;
            Debug.Log($"[BattleMemoryLeak_PlayModeTests] After battle {battleIndex} memory usage increased by managed={managedDelta} bytes, total={totalDelta} bytes compared to baseline.");
        }

        if (IsStrictlyIncreasing(adventureManagedDeltas) && managedDeltaTotal > 0)
        {
            Assert.Fail($"Managed memory grows every time Adventure reloads. Baseline={stats.BaselineManaged} bytes, final={finalManaged} bytes, delta={managedDeltaTotal} bytes. Battle deltas={string.Join(", ", adventureManagedDeltas)} bytes.");
        }
        if (IsStrictlyIncreasing(adventureTotalDeltas) && totalDeltaTotal > 0)
        {
            Assert.Fail($"Total (native) memory grows every time Adventure reloads. Baseline={stats.BaselineTotal} bytes, final={finalTotal} bytes, delta={totalDeltaTotal} bytes. Battle deltas={string.Join(", ", adventureTotalDeltas)} bytes.");
        }

        Debug.Log($"[BattleMemoryLeak_PlayModeTests] Baseline -> managed={stats.BaselineManaged} bytes, total={stats.BaselineTotal} bytes. " +
                  $"After cycles -> managed={finalManaged} bytes, total={finalTotal} bytes. " +
                  $"Delta -> managed={managedDeltaTotal} bytes, total={totalDeltaTotal} bytes.");
        Debug.Log($"[BattleMemoryLeak_PlayModeTests] Summary -> Scene loads: {stats.SceneLoadCount} (sum managed delta={stats.TotalSceneManagedDelta} bytes, sum total delta={stats.TotalSceneNativeDelta} bytes); " +
                  $"Battles: {stats.BattleCount} (sum managed delta={stats.TotalBattleManagedDelta} bytes, sum total delta={stats.TotalBattleNativeDelta} bytes).");
        Debug.Log($"[BattleMemoryLeak_PlayModeTests] Per-load memory snapshot: {string.Join(" | ", stats.SceneLoadReports)}");
    }

    private static IEnumerator LoadSceneAndWait(string sceneName)
    {
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // allow installers and Start() methods to finish
        yield return null;
    }

    private static IEnumerator LoadSceneAndMeasure(string sceneName, string label, MemoryStats stats, bool countTowardsSummary = true)
    {
        var managedBefore = System.GC.GetTotalMemory(true);
        var totalBefore = Profiler.GetTotalAllocatedMemoryLong();

        yield return LoadSceneAndWait(sceneName);

        var managedAfter = System.GC.GetTotalMemory(true);
        var totalAfter = Profiler.GetTotalAllocatedMemoryLong();
        var managedDelta = managedAfter - managedBefore;
        var totalDelta = totalAfter - totalBefore;

        stats.RecordSceneLoad(label, managedDelta, totalDelta, managedAfter, totalAfter, countTowardsSummary);
        stats.UpdateLastMemory(managedAfter, totalAfter);
    }

    private static bool IsStrictlyIncreasing(IReadOnlyList<long> deltas)
    {
        if (deltas.Count < 2)
            return false;

        for (int i = 1; i < deltas.Count; i++)
        {
            if (deltas[i] <= deltas[i - 1])
            {
                return false;
            }
        }

        return true;
    }

    private sealed class MemoryStats
    {
        public int SceneLoadCount { get; private set; }
        public long TotalSceneManagedDelta { get; private set; }
        public long TotalSceneNativeDelta { get; private set; }

        public int BattleCount { get; private set; }
        public long TotalBattleManagedDelta { get; private set; }
        public long TotalBattleNativeDelta { get; private set; }

        public long LastManaged { get; private set; }
        public long LastTotal { get; private set; }
        public long BaselineManaged { get; private set; }
        public long BaselineTotal { get; private set; }

        private readonly List<string> _sceneLoadReports = new();
        public IReadOnlyList<string> SceneLoadReports => _sceneLoadReports;

        public void RecordSceneLoad(string label, long managedDelta, long totalDelta, long managedAfter, long totalAfter, bool countTowardsSummary)
        {
            Debug.Log($"[BattleMemoryLeak_PlayModeTests] {label}: managed delta={managedDelta} bytes, total delta={totalDelta} bytes.");
            _sceneLoadReports.Add($"{label} -> managed={managedAfter} bytes, total={totalAfter} bytes (delta m={managedDelta}, delta t={totalDelta})");
            if (countTowardsSummary)
            {
                SceneLoadCount++;
                TotalSceneManagedDelta += managedDelta;
                TotalSceneNativeDelta += totalDelta;
            }
        }

        public void RecordBattle(string label, long managedDelta, long totalDelta)
        {
            BattleCount++;
            TotalBattleManagedDelta += managedDelta;
            TotalBattleNativeDelta += totalDelta;
            Debug.Log($"[BattleMemoryLeak_PlayModeTests] {label}: managed delta={managedDelta} bytes, total delta={totalDelta} bytes.");
        }

        public void UpdateLastMemory(long managed, long total)
        {
            LastManaged = managed;
            LastTotal = total;
        }

        public void SetBaselineToLast()
        {
            BaselineManaged = LastManaged;
            BaselineTotal = LastTotal;
        }

        public void RecordAdventureReturn(string label, long managedDeltaFromBaseline, long totalDeltaFromBaseline)
        {
            Debug.Log($"[BattleMemoryLeak_PlayModeTests] {label} vs baseline: managed delta={managedDeltaFromBaseline} bytes, total delta={totalDeltaFromBaseline} bytes.");
        }
    }
}
