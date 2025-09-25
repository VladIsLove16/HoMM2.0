using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IEntryView
{
    void Bind(UnitDefinitionSO definition);
    void SetPresentationMode(PresentationMode mode);
}

public class MushroomEntryView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEntryView
{
    [Header("Refs")]
    [SerializeField] private Image picture;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Transform characteristicsRoot;
    [SerializeField] private GameObject characteristicItemPrefab;

    private UnitDefinitionSO current;
    private PresentationMode mode = PresentationMode.Normal;
    private bool isHovered;

    public void Bind(UnitDefinitionSO definition)
    {
        current = definition;
        nameText.text = definition != null ? definition.Name : string.Empty;
        descriptionText.text = definition != null ? definition.Description : string.Empty;
        RebuildCharacteristics(definition);
        UpdateImage();
    }

    public void SetPresentationMode(PresentationMode mode)
    {
        this.mode = mode;
        UpdateImage();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateImage();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateImage();
    }

    private void UpdateImage()
    {
        if (picture == null || current == null) return;
        Sprite sprite = null;
        if (mode == PresentationMode.Humanized)
        {
            sprite = isHovered ? current.HumanizedIconHovered : current.HumanizedIcon;
        }
        else
        {
            sprite = isHovered ? current.UnitIconHovered : current.UnitIcon;
        }
        picture.sprite = sprite;
    }

    private void RebuildCharacteristics(UnitDefinitionSO definition)
    {
        if (characteristicsRoot == null || characteristicItemPrefab == null) return;
        for (int i = characteristicsRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(characteristicsRoot.GetChild(i).gameObject);
        }

        if (definition == null) return;
        var list = definition.Characteristics;
        if (list == null) return;

        foreach (var c in list)
        {
            var go = Instantiate(characteristicItemPrefab, characteristicsRoot);
            var text = go.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = string.IsNullOrEmpty(c.Key) ? c.Value : $"{c.Key}: {c.Value}";
            }
        }
    }
}


