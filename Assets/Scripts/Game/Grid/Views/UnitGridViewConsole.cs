using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UnitGridViewConsole : IView
{
    private GameViewModel gameGridViewModel;
    public enum RenderType
    {
        gridContent,
        allCells
    }
    public UnitGridViewConsole(GameViewModel gameGridViewModel)
    {
        this.gameGridViewModel = gameGridViewModel;
        Render();
        //_gameGridViewModel.OnCellChanged += GameGridViewModel_OnCellChanged;
    }

    private void GameGridViewModel_OnCellChanged(GridCellUnitSpawnedEventArgs args)
    {
        Debug.Log("new content at " + args.x +  ";" + args.y + " : " + args.addedContent.ToString());
    }
    public void Render()
    {
        Render(RenderType.allCells);
    }
    public void Render(RenderType renderType = RenderType.allCells)
    {
        if(renderType == RenderType.gridContent)
        {
            foreach (var cell in gameGridViewModel.GetCells())
            {
                if(!cell.IsEmpty)
                    Debug.Log(cell.ToString());
            }
        }
        else
            foreach (var cell in gameGridViewModel.GetCells())
            {
                Debug.Log(cell.ToString());
            }
    }
}