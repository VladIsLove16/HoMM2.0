using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class GameController : MonoBehaviour
{
    [SerializeField] private int Heigh;
    [SerializeField] private int Weight;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private UnitContentEntrySO unitContentEntrySO;
    private GameModel _gameModel;
    private GameViewModel _gameViewModel;
    private GridView _gridView;
    private TurnSystem _combatSystem;
    private Vector2Int selectedCellCoords;
    private bool isCellSelected;
    //private ActionService ActionService;
    [Inject]
    public void Construct(GameViewModel viewModel, GridView view, GameModel model, TurnSystem combatSystem)
    {
        Debug.Log("_gameController Construct start");
        this._gameViewModel = viewModel;
        this._gridView = view;
        this._gameModel = model;
        this._combatSystem = combatSystem;

        Setup(Weight, Heigh);
        InitCombatSystem();

    }

    private void InitCombatSystem()
    {
        _combatSystem.ClearUnits();
        foreach (var unit in _gameModel.GetUnits())
        {
            unit.Died += () =>  OnCombatUnitDied(unit);
            _combatSystem.AddCombatUnit(unit);
        }
        _gameModel.CellContentAdded += OnGameModel_CellContentAdded;

    }

    private void OnCombatUnitDied(UnitModel unit)
    {
        _combatSystem.RemoveCombatUnit(unit);
        //unit.Died -= () => OnCombatUnitDied(unit);
    }

    private void OnGameModel_CellContentAdded(UnitModelCreatedParams @params)
    {
        ICombatUnit combatUnit = @params.UnitModel;
        _combatSystem.AddCombatUnit(combatUnit);
    }

    [Button]
    public void Setup()
    {
        Setup(Weight, Heigh);
    }
    

    [Button]
    public void RunBattle()
    {
        Debug.Log("run attle");
        _combatSystem.RunBattle();
    }

    public void Setup(int width, int height)
    {
        Debug.Log("grid init " + width + "x" + height);
        _gameModel.InitializeGrid(width, height);
        _gridView.CreateGrid();
    }

    [Button]
    public void Create()
    {
        CreateGridContent(unitContentEntrySO);
    }

    public void CreateGridContent(UnitContentEntrySO unitContentEntrySO)
    {
        _gameModel.ClearGrid();
        foreach (var content in unitContentEntrySO.contents)
        {
            UnitSpawnParams unitSpawnParams = new UnitSpawnParams(content.X, content.Y,content.unitType,content.Amount,content.isPlayer);
            _gameModel.SpawnUnit(unitSpawnParams);
        }
    }
    [Button]
    public void StartTurn()
    {
        Vector2Int selectedCell = _gameViewModel.GetSelectedCell();
        if (selectedCell == null)
        {
            Debug.LogWarning("select cell first");
            return;
        }
        IGridCell gridCell = _gameModel.GetCell(selectedCell);
        UnitModel unitModel = gridCell.Unit;
        if(unitModel == null)
        {
            Debug.LogWarning("no unit in selected cell");
            return;
        }
        unitModel.TakeTurn();

    }
}
