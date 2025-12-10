using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

/// <summary>
/// Helper that executes several battle turns and logs managed/total memory deltas.
/// Lives in the PlayMode test assembly so it can be reused by leak-related tests
/// without pulling dev-only scripts into runtime builds.
/// </summary>
public sealed class BattleMemoryLeakTester : MonoBehaviour
{
    private GameController _gameController;
    private ITurnService _turnService;

    [Inject]
    public void Construct(GameController gameController, ITurnService turnService)
    {
        _gameController = gameController;
        _turnService = turnService;
    }

    /// <summary>
    /// Runs a simple battle loop for the specified number of battles and turns,
    /// logging managed and total memory before and after each battle.
    /// </summary>
    public IEnumerator RunLeakCheck(
        int battles = 5,
        int turnsPerBattle = 10,
        System.Action<int, long, long> onBattleCompleted = null)
    {
        if (_gameController == null || _turnService == null)
        {
            Debug.LogError("[BattleMemoryLeakTester] GameController or TurnService is not injected.");
            yield break;
        }

        for (int i = 0; i < battles; i++)
        {
            var managedBefore = System.GC.GetTotalMemory(true);
            var totalBefore = Profiler.GetTotalAllocatedMemoryLong();

            _gameController.Setup(null);
            _turnService.RunBattle();

            var turns = 0;
            while (_turnService.BattleState == BattleState.inProgress && turns < turnsPerBattle)
            {
                _turnService.EndTurn();
                turns++;
                yield return null;
            }

            yield return Resources.UnloadUnusedAssets();
            System.GC.Collect();

            var managedAfter = System.GC.GetTotalMemory(true);
            var totalAfter = Profiler.GetTotalAllocatedMemoryLong();

            var managedDelta = managedAfter - managedBefore;
            var totalDelta = totalAfter - totalBefore;

            Debug.Log(
                $"[BattleMemoryLeakTester] Battle {i + 1}/{battles} finished. " +
                $"Managed Δ={managedDelta} bytes, " +
                $"Total Δ={totalDelta} bytes.");

            onBattleCompleted?.Invoke(i + 1, managedDelta, totalDelta);
        }
    }
}
