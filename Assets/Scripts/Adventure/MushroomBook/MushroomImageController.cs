using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MushroomImageController : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image picture;
    private bool isHovered;

    public Action PointerEnter;
    public Action PointerExit;
    public void SetSprite(Sprite sprite)
    {
        picture.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke();
    }
}