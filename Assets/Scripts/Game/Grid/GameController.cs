using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class GameController : MonoBehaviour
{
    [SerializeField] private int Heigh;
    [SerializeField] private int Weight;
    [SerializeField] private UnitContentEntrySO unitContentEntrySO;
    private GameModel _gameModel;
    private GameViewModel _gameViewModel;
    private GridView _gridView;
    private Vector2Int selectedCellCoords;
    private bool isCellSelected;
    [Inject]
    public void Construct(GameViewModel viewModel, GridView view, GameModel model)
    {
        Debug.Log("_gameController Construct start");
        this._gameViewModel = viewModel;
        this._gridView = view;
        this._gameModel = model;
        Setup(Weight, Heigh);
    }

    [Button]
    public void Setup()
    {
        Setup(Weight, Heigh);
    }

    public void Setup(int width, int height)
    {
        Debug.Log("grid init " + width +"x" + height);
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

    public void SelectCell(Vector2Int coords)
    {
        _gridView.SelectCell(coords);
        selectedCellCoords = coords;
        isCellSelected = true;
    }

    public void PerformAction(Vector2Int coords)
    {
        Debug.Log("performing action for "  + coords + " from " + selectedCellCoords);
        if(_gameModel.SwapUnits(coords, selectedCellCoords))
        {
            isCellSelected = false;      
            _gridView.UnSelect();
        }
            
        else if(_gameModel.MoveUnit(coords, selectedCellCoords))
        {
            selectedCellCoords = coords;
            SelectCell(coords);
        }
    }

    internal void Hover(Vector2Int coords)
    {
        _gridView.Hover(coords);
    }
}
