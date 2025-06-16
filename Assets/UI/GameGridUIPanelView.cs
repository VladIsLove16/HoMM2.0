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
 

    private GameGridViewModel viewModel;
    [Inject]
    public void Construct(GameGridViewModel viewModel)
    {
        this.viewModel = viewModel;

        btnSpawnRandom.onClick.AddListener(() =>
        {
            Debug.Log("btnSpawnRandom clicked");
            viewModel.SpawnRandomUnit();
        });
        btnSpawnAt.onClick.AddListener(() => viewModel.SpawnUnitAt(xText.text, yText.text));
        btnSelectCell.onClick.AddListener(() => viewModel.GetCell(xText.text, yText.text)?.GetDescription());

        
        btnSpawnAt.onClick.AddListener(() => Debug.Log("btnSpawnAt clicked"));
        btnSelectCell.onClick.AddListener(() => Debug.Log("btnSelectCell clicked"));

        viewModel.OnSelectedCellChanged += desc => selectedCellDescriptionText.text = desc;
        viewModel.OnCellChanged += desc => lastChangedCellDescriptionText.text = desc.addedContent is IDescriptable descriptable ? descriptable.GetDescription() : desc.addedContent.ToString();
        viewModel.OnError += msg => ShowError(msg);
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

    private void HandleCellChanged(GridCellChangedEventArgs args)
    {
        lastChangedCellDescriptionText.text = viewModel.GetDescription(X, Y);
    }

}
