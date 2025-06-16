using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UnitGridConsoleView : IView
{
    private GameGridViewModel gameGridViewModel;
    public enum RenderType
    {
        gridContent,
        allCells
    }
    public UnitGridConsoleView(GameGridViewModel gameGridViewModel)
    {
        this.gameGridViewModel = gameGridViewModel;
        Render();
        gameGridViewModel.OnCellChanged += GameGridViewModel_OnCellChanged;
    }

    private void GameGridViewModel_OnCellChanged(GridCellChangedEventArgs args)
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
                if(!cell.IsEmpty())
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

