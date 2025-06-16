using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameGridCell : IDescriptable
{
    protected GridXZ<GameGridCell> mainGrid;
    private List<IGridContent> gameGridObjectContents = new();
    public int x;
    public int y;
    public GameGridCell(GridXZ<GameGridCell> grid, int x, int y)
    {
        this.mainGrid = (grid);
        this.x = x;
        this.y = y;
    }
    public void Setup(GridXZ<GameGridCell> grid, int x, int y)
    {
        this.mainGrid = (grid);
        this.x = x;
        this.y = y;
    }
    public void AddContent(IGridContent content)
    {
        if (content == null)
            throw new ArgumentNullException("cant add null content");
        gameGridObjectContents.Add(content);
        mainGrid.TriggerGridObjectChanged(x, y);
    }
    public void RemoveContent(IGridContent content)
    {
        gameGridObjectContents.Remove(content);
        mainGrid.TriggerGridObjectChanged(x, y);
    }
    public virtual bool IsEmpty()
    {
        return !gameGridObjectContents.Any();
    }
    public bool CanMove()
    {
        foreach (IGridContent content in gameGridObjectContents)
        {
            if (content is IBlockable blockable && !blockable.CanMoveThrough())
            {
                return false; // Если хотя бы один объект не пропускает движение
            }
        }
        return true; // Если все объекты позволяют двигаться через клетку
    }
    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(x + " " + y);
        foreach(var content in gameGridObjectContents)
        {
            if(content is IDescriptable descriptable)
                stringBuilder.AppendLine(descriptable.GetDescription());
            else
                stringBuilder.AppendLine(content.ToString());
        }
        return stringBuilder.ToString();
    }
    public List<GameGridCell> GetNeigbours(int range,bool AddCorners)
    {
        List<GameGridCell> neighbors = new List<GameGridCell>();
        for (int i = x - range; i <= x + range; i++)
        {
            for (int j = y - range; j <= y + range; j++)
            {
                // Проверяем, что координаты находятся в пределах матрицы
                if (i >= 0 && i < mainGrid.GetHeight() && j >= 0 && j < mainGrid.GetWidth())
                {
                    if((Math.Abs(x-i) + Math.Abs(y-j)) <= range || AddCorners)
                        neighbors.Add(mainGrid.GetGridObject(i,j));
                }
            }
        }
        Debug.Log("Neigbour Count" +  neighbors.Count);
        return neighbors;
    }
    public string GetDescription()
    {
        // Выводим описание всех объектов на клетке
        string description = "GameGridModel " + x + " " + y + " contains: ";
        foreach (IGridContent content in gameGridObjectContents)
        {
            description += content.GetDescription() + ", ";
        }
        return description;
    }
}
