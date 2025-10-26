using Adventure.Domain.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IEntryView
{
    void Bind(UnitDefinitionSO? data);
    void SetPresentationMode(PresentationMode mode);
}

public class MushroomBookEntryView : MonoBehaviour, IEntryView
{
    [Header("Refs")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform characteristicsRoot;
    [SerializeField] private UnitSingleStatPanel characteristicItemPrefab;
    [SerializeField] private MushroomImageController mushroomImage;

    private UnitDefinitionSO? runtimeData;
    private PresentationMode mode = PresentationMode.Normal;
    public PresentationMode Mode => mode;
    private void Start()
    {
        mushroomImage.PointerEnter += OnImagePointerEnter;
        mushroomImage.PointerExit += OnImagePointerExit;
    }

    public void Bind(UnitDefinitionSO data)
    {
        runtimeData = data;
        ApplyData();
    }

    public void SetPresentationMode(PresentationMode mode)
    {
        this.mode = mode;
        UpdateImage();
    }

    private void UpdateImage()
    {
        Sprite sprite;
        if (mode == PresentationMode.Humanized)
        {
            sprite = runtimeData.HumanizedIcon;
        }
        else
        {
            sprite = runtimeData.Icon;
        }
        mushroomImage.SetSprite(sprite);

    }

    private void ApplyData()
    {
        UpdateTexts();
        RebuildCharacteristics(runtimeData);
        UpdateImage();
    }

    private void UpdateTexts()
    {
        if (nameText != null)
            nameText.text = runtimeData?.Name ?? string.Empty;
        if (descriptionText != null)
            descriptionText.text = runtimeData?.Description ?? string.Empty;
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
            CreateCharacteristicEntry(c,0);
        }
        else
        {
            CreateCharacteristicEntry("Health" , stats.Health);
            CreateCharacteristicEntry("Damage" , stats.Damage);
            CreateCharacteristicEntry("MoveSpeed", stats.MoveSpeed);
        }
    }

    private void CreateCharacteristicEntry(string statText, int amount)
    {
        UnitSingleStatPanel go = Instantiate(characteristicItemPrefab, characteristicsRoot);
        go.SetInfo(null, statText, amount);
    }
    private void OnImagePointerExit()
    {
        var data = runtimeData;
        Sprite sprite;
        if (mode == PresentationMode.Humanized)
        {
            sprite = data.HumanizedIcon;
        }
        else
        {
            sprite = data.Icon;
        }
        mushroomImage.SetSprite(sprite);
    }

    private void OnImagePointerEnter()
    {
        var data = runtimeData;
        Sprite sprite;
        if (mode == PresentationMode.Humanized)
        {
            sprite = data.HoveredIcon;
        }
        else
        {
            sprite = data.HumanizedHoveredIcon;
        }
        mushroomImage.SetSprite(sprite);
    }

}
