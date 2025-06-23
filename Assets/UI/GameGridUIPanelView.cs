using System;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameGridUIPanelView : MonoBehaviour
{
    private const float showErrorTime = 5f;
    [SerializeField] private Button btnSpawnAt;
    [SerializeField] private Button btnSpawnRandom;
    [SerializeField] private Button btnSelectCell;
    [SerializeField] private TMP_Text selectedCellDescriptionText;
    [SerializeField] private TMP_Text lastChangedCellDescriptionText;
    [SerializeField] private TMP_InputField xText;
    [SerializeField] private TMP_InputField yText;
    [SerializeField] private TMP_Text errorText;

    private int X 
    {
        get
        {
            int.TryParse(xText.text, out int res);
            return res;
        }
    } 

    private int Y 
    {
        get
        {
            int.TryParse(yText.text, out int res);
            return res;
        }
    }
 
    private GameViewModel viewModel;

    [Inject]
    public void Construct(GameViewModel viewModel)
    {
        this.viewModel = viewModel;

        btnSpawnRandom.onClick.AddListener(() =>
        {
            Debug.Log("btnSpawnRandom clicked");
            viewModel.SpawnUnitAtRandomPlace();
        });
        btnSpawnAt.onClick.AddListener(() => viewModel.SpawnUnitAt(xText.text, yText.text));
        btnSelectCell.onClick.AddListener(BtnSelectCell_onClick);
        btnSpawnAt.onClick.AddListener(() => Debug.Log("btnSpawnAt clicked"));
        btnSelectCell.onClick.AddListener(() => Debug.Log("btnSelectCell clicked"));

        viewModel.OnError += msg => ShowError(msg);
    }

    private void BtnSelectCell_onClick()
    {
        var cell = viewModel.GetCellContent(xText.text, yText.text);
        selectedCellDescriptionText.text = GetDescription(cell);
    }

    private void ViewModel_OnCellChanged(GridCellUnitSpawnedEventArgs args)
    {
         lastChangedCellDescriptionText.text = GetDescription(args.addedContent);
    }

    private void ShowError(string msg)
    {
        errorText.text = msg;
        Invoke("HideEror", showErrorTime);

    }
    private void HideEror()
    {
        errorText.text = string.Empty;
    }

    private void HandleCellChanged(GridCellUnitSpawnedEventArgs args)
    {
        lastChangedCellDescriptionText.text = GetDescription(args.addedContent);
    }
    private string GetDescription(IGridCell gridContent)
    {
        string description = string.Empty;
        foreach (IGridContent content in gridContent.Contents)
        {
            description += GetDescription(content) + ", ";
        }
        return description;
    }
    private string GetDescription(IGridContent gridContent)
    {
       return gridContent.ToString();
    }

}
