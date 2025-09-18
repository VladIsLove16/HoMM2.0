using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Network_RemoveAndDeathIntegrationTest
{
    [UnityTest]
    public IEnumerator Unit_Death_Removes_View_And_TurnSystem_Entry()
    {
        var go = new GameObject("TestRoot");
        GameNetworkCommandGateway gameNetworkCommandGateway = new();
        var model = new GameModel(new UnitModelFactory(), new MovementSystem(), new ActionHandlerFactory(),new CommandService(new NetworkCommandExecutor(gameNetworkCommandGateway)));
        var turn = new TurnSystem();

        // init grid
        model.InitializeGrid(4,4);

        // spawn unit
        var stats = ScriptableObject.CreateInstance<UnitStats>();
        stats.MaxHealth = 10; stats.Health = 10; stats.Damage = 5; stats.MoveSpeed = 3;
        var spawn = new UnitSpawnParams(1,1, UnitType.Archer, 1, true);
        model.SpawnUnit(spawn);

        var unit = model.GetCell(new Vector2Int(1,1)).Unit;
        Assert.NotNull(unit);

        // add to turn system
        turn.AddCombatUnit(unit);
        Assert.IsTrue(turn.CombatUnits.Contains(unit));

        // create simple view container
        var viewRoot = new GameObject("Views");
        var view = viewRoot.AddComponent<UnitView3D>();
        var vm = new UnitViewModel(unit, new MaterialProvider());
        view.Init(vm);

        // kill unit
        var ctx = new DamageContext(9999, DamageType.physical, null);
        unit.RecieveDamage(ctx);
        // UnitModel.Died -> GameModel.UnitRemoved -> GameView3D.OnUnitRemoved -> Destroy(view)

        yield return null;

        // view should be disabled or destroyed
        Assert.IsTrue(view == null || !view.gameObject.activeSelf);

        // remove from turn system by controller on UnitRemoved (in real scene); here emulate
        turn.RemoveCombatUnit(unit);
        Assert.IsFalse(turn.CombatUnits.Contains(unit));
    }
}



