using Adventure.Domain.Inventory;
using Adventure.Presentation.Mushroom;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[CreateAssetMenu(menuName = "MockMushroomBook/MockMushroomBookViewModel", fileName = "MockMushroomBookViewModel")]
public class MockMushroomBookViewModel : ScriptableObject 
{
    [SerializeField] private UnitDefinitionSOCollection catalog;
    private MushroomBookViewModel mushroomBookViewModel;
    private MushroomInventoryModel mushroomInventoryModel;
    [SerializeField] private int entriesPerPage = 4;
    [Button("Initialize ViewModel")]
    private void OnEnable()
    {
        mushroomInventoryModel = new();
        foreach(var mushroom in catalog.GetAll())
        {
            mushroomInventoryModel.Add(mushroom.UnitType, 2);
        }
        mushroomBookViewModel = new MushroomBookViewModel(mushroomInventoryModel, catalog, entriesPerPage);
        mushroomBookViewModel.SetPresentationMode(PresentationMode.Normal);
        mushroomBookViewModel.GoToPage(0);
    }
   
}
