using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
public class GridController : MonoBehaviour
{
    [SerializeField] private int Heigh;
    [SerializeField] private int Weight;
    private GameGridModel model;
    private GameGridViewModel viewModel;
    private GameGridSceneView view;

    [Inject]
    public void Construct(GameGridViewModel viewModel, GameGridSceneView view, GameGridModel model)
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

}
