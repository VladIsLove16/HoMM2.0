using UnityEngine;

/// <summary>
/// Interface for game objects that can be hovered and selected in the view layer
/// </summary>
public interface IGameViewObject
{
    Transform transform { get; }
    bool IsHoverable { get; }
    bool IsSelectable { get; }
}








