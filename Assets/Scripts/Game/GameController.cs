using NaughtyAttributes;
using System;
using UnityEngine;
using Zenject;

public class GameController : MonoBehaviour
{
    [SerializeField] private int Height;
    [SerializeField] private int Width;
    [SerializeField] private GridContentEntrySO gridContentEntrySO;

    private GameModel _gameModel;
    private TurnSystem _combatSystem;

    // Текущая выбранная клетка и флаг
    private Vector2Int? selectedCellCoords;
    private bool isCellSelected;

    [Inject]
    public void Construct(GameViewModel viewModel, GridView view, GameModel model, TurnSystem combatSystem)
    {
        _gameModel = model;
        _combatSystem = combatSystem;

        
    }
    private void Start()
    {
        Setup(Width, Height);
        InitCombatSystem();
        CreateGridContent();
        RunBattle();
    }
    private void InitCombatSystem()
    {
        _combatSystem.ClearUnits();

        foreach (var unit in _gameModel.GetUnits())
        {
            Action onDied = null;
            onDied = () =>
            {
                OnCombatUnitDied(unit);
                unit.Died -= onDied;
            };
            unit.Died += onDied;

            _combatSystem.AddCombatUnit(unit);
        }

        _gameModel.UnitSpawned += OnGameModel_CellContentAdded;
    }

    private void OnCombatUnitDied(UnitModel unit)
    {
        _combatSystem.RemoveCombatUnit(unit);
    }

    private void OnGameModel_CellContentAdded(UnitModelCreatedParams @params)
    {
        ICombatObject combatUnit = @params.UnitModel;
        _combatSystem.AddCombatUnit(combatUnit);
    }

    [Button]
    public void Setup()
    {
        Setup(Width, Height);
    }

    [Button]
    public void RunBattle()
    {
        Debug.Log("Run battle");
        _combatSystem.RunBattle();
    }
    [Button]
    public void CreateGridContent()
    {
        CreateGridContent(gridContentEntrySO);
    }

    public void CreateGridContent(GridContentEntrySO unitContentEntrySO)
    {
        foreach (var content in unitContentEntrySO.contents)
        {
            UnitSpawnParams unitSpawnParams = new UnitSpawnParams(content.X, content.Y, content.unitType, content.Amount, content.isPlayer);
            _gameModel.SpawnUnit(unitSpawnParams);
        }
    }
    public void Setup(int width, int height)
    {
        _gameModel.InitializeGrid(width, height);
    }
}
