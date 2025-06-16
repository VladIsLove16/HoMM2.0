using System;
using UnityEngine;

public interface ICellGridRenderer
{
    void Clear();
    void Render(int width, int height, float cellSize, Vector3 origin, float padding);
    Vector3 ToWorld(int x, int y);
}