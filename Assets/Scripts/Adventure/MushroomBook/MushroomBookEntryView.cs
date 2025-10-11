using Adventure.Domain.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IEntryView
{
    void Bind(MushroomViewModel? data);
    void SetPresentationMode(PresentationMode mode);
}

public class MushroomBookEntryView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEntryView
{
    [Header("Refs")]
    [SerializeField] private Image picture;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Transform characteristicsRoot;
    [SerializeField] private GameObject characteristicItemPrefab;

    private MushroomViewModel? runtimeData;
    private PresentationMode mode = PresentationMode.Normal;
    private bool isHovered;

    public void Bind(MushroomViewModel? data)
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
        var data = runtimeData.Value;
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

    private void RebuildCharacteristics(MushroomViewModel? data)
    {
        if (characteristicsRoot == null || characteristicItemPrefab == null) return;
        for (int i = characteristicsRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(characteristicsRoot.GetChild(i).gameObject);
        }

        if (data == null) return;
        var list = data.Value.Characteristics;

        if (list == null)
        {
            string c = "No characteristics";
            CreateCharacteristicEntry(c);
        }
        else
            foreach (var c in list)
            {
                CreateCharacteristicEntry(c);
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

