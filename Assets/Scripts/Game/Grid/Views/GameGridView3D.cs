using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameGridView3D
{
    private const float CORPSEALIVETIME = 5f;
    private GameGridViewModel gameGridViewModel;
    [Inject]  private UnitViewFactory unitViewFactory;
    Dictionary<UnitViewModel, UnitView3D> views = new();
    public GameGridView3D(GameGridViewModel gameGridViewModel)
    {
        this.gameGridViewModel = gameGridViewModel;
        gameGridViewModel.OnCellContentAdded += GameGridViewModel_OnCellContentAdded;
        gameGridViewModel.OnCellContentRemoved += GameGridViewModel_OnCellContentRemoved;
    }
    private void GameGridViewModel_OnCellContentAdded(UnitViewModel viewModel)
    {
        UnitView3D unitView3D = unitViewFactory.Create(viewModel);
        views[viewModel] = unitView3D;
    }

    private void GameGridViewModel_OnCellContentRemoved(UnitViewModel viewModel)
    {
        UnitView3D unitView3D = views[viewModel];
        views.Remove(viewModel);
        Destroy(unitView3D, CORPSEALIVETIME);
    }

    private void Destroy(UnitView3D unitView3D,float time = 0f)
    {
        Debug.Log("content will be removed after" + time);
        if (unitView3D != null)
            GameObject.Destroy(unitView3D.gameObject);
        else
            Debug.LogWarning("view have been destroyed");
    }
}

