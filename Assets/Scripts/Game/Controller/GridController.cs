using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class GridController : MonoBehaviour
{
    [SerializeField] private int Heigh;
    [SerializeField] private int Weight;
    [SerializeField] private UnitContentEntrySO unitContentEntrySO;
    private GameGridModel model;
    private GameGridViewModel viewModel;
    private GridView view;

    [Inject]
    public void Construct(GameGridViewModel viewModel, GridView view, GameGridModel model)
    {
        this.viewModel = viewModel;
        this.view = view;
        this.model = model;
        model.InitializeGrid(Weight, Heigh);
    }
    private void Awake()
    {
        Setup(Weight, Heigh);
    }

    [Button]
    public void Setup()
    {
        Setup(Weight, Heigh);
    }

    public void Setup(int width, int height)
    {
        model.InitializeGrid(width, height);
        view.CreateGrid();
    }

    [Button]
    public void Create()
    {
        CreateGridContent(unitContentEntrySO);
    }

    public void CreateGridContent(UnitContentEntrySO unitContentEntrySO)
    {
        model.ClearGrid();
        foreach (var content in unitContentEntrySO.contents)
        {
            UnitSpawnParams unitSpawnParams = new UnitSpawnParams(content.X, content.Y,content.unitType,content.Amount,content.isPlayer);
            model.SpawnUnit(unitSpawnParams);
        }
    }
}
