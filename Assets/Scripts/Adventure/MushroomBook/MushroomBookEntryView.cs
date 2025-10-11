using Adventure.Domain.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IEntryView
{
    void Bind(UnitDefinitionSO? data);
    void SetPresentationMode(PresentationMode mode);
}

public class MushroomBookEntryView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEntryView
{
    [Header("Refs")]
    [SerializeField] private Image picture;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform characteristicsRoot;
    [SerializeField] private GameObject characteristicItemPrefab;

    private UnitDefinitionSO? runtimeData;
    private PresentationMode mode = PresentationMode.Normal;
    public PresentationMode Mode => mode;
    private bool isHovered;

    public void Bind(UnitDefinitionSO? data)
    {
        runtimeData = data;
        ApplyData();
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
        if (picture == null || runtimeData == null) return;
        var data = runtimeData;
        Sprite sprite;
        if (mode == PresentationMode.Humanized)
        {
            sprite = isHovered ? data.HumanizedHoveredIcon : data.HumanizedIcon;
        }
        else
        {
            sprite = isHovered ? data.HoveredIcon : data.Icon;
        }
        picture.sprite = sprite;
    }

    private void ApplyData()
    {
        if (nameText != null)
            nameText.text = runtimeData?.Name ?? string.Empty;
        if (descriptionText != null)
            descriptionText.text = runtimeData?.Description ?? string.Empty;
        RebuildCharacteristics(runtimeData);
        if (runtimeData == null && picture != null)
        {
            picture.sprite = null;
        }
        else
        {
            UpdateImage();
        }
    }

    private void RebuildCharacteristics(UnitDefinitionSO? data)
    {
        if (characteristicsRoot == null || characteristicItemPrefab == null) return;
        for (int i = characteristicsRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(characteristicsRoot.GetChild(i).gameObject);
        }

        if (data == null) return;
        var stats = data.Stats;

        if (stats == null)
        {
            string c = "No characteristics";
            CreateCharacteristicEntry(c);
        }
        else
        {
            CreateCharacteristicEntry(" stats.Health " + stats.Health);
            CreateCharacteristicEntry(" stats.Damage " + stats.Damage);
            CreateCharacteristicEntry(" stats.MoveSpeed " + stats.MoveSpeed);
        }
    }

    private void CreateCharacteristicEntry(string c)
    {
        var go = Instantiate(characteristicItemPrefab, characteristicsRoot);
        var text = go.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = c;
        }
    }
}

