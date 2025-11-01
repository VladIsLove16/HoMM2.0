using System;
using System.Collections.Generic;
using Adventure.Presentation.Mushroom;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IEntryView
{
    void Bind(MushroomBookEntryViewModel data);
    void SetPresentationMode(PresentationMode mode);
}
[Serializable]
public class StatIconBinding
{
    public UnitStatType Type;
    public Sprite Icon;
}

public class MushroomBookEntryView : MonoBehaviour, IEntryView
{
    [SerializeField] private MushroomImageController mushroomImageController;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform statsRoot;
    [SerializeField] private UnitSingleStatPanel statItemPrefab;
    [SerializeField] private StatIcons statIcons;

    private readonly Dictionary<UnitStatType, Sprite> _iconLookup = new();
    private MushroomBookEntryViewModel _viewData = MushroomBookEntryViewModel.Empty;
    private PresentationMode _mode = PresentationMode.Normal;
    private bool _isPointerOver;
    private bool _pointerSubscribed;

    public PresentationMode Mode => _mode;
    public string Title => nameText != null ? nameText.text : string.Empty;
    public string Description => descriptionText != null ? descriptionText.text : string.Empty;
    public int StatItemCount => statsRoot != null ? statsRoot.childCount : 0;
    public Sprite CurrentSprite => mushroomImageController != null ? mushroomImageController.CurrentSprite : null;

    private void Awake()
    {
        BuildIconLookup();
        SubscribeToPointerEvents();
    }

    private void OnEnable()
    {
        SubscribeToPointerEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromPointerEvents();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        BuildIconLookup();
    }
#endif

    private void OnDestroy()
    {
        UnsubscribeFromPointerEvents();
    }

    public void Bind(MushroomBookEntryViewModel data)
    {
        _viewData = data ?? MushroomBookEntryViewModel.Empty;
        _isPointerOver = false;
        SubscribeToPointerEvents();
        ApplyData();
    }

    public void SetPresentationMode(PresentationMode mode)
    {
        _mode = mode;
        RefreshImage();
    }

    private void BuildIconLookup()
    {
        _iconLookup.Clear();
        if (statIcons == null)
            return;

        foreach (var binding in statIcons.statIconBindings)
        {
            if (binding == null)
                continue;

            _iconLookup[binding.Type] = binding.Icon;
        }
    }

    private void EnsureIconLookup()
    {
        if (_iconLookup.Count == 0 && statIcons != null && statIcons.statIconBindings.Count > 0)
        {
            BuildIconLookup();
        }
    }

    private void ApplyData()
    {
        SetText(nameText, _viewData.DisplayName);
        SetText(amountText, _viewData.Amount.ToString());
        SetText(descriptionText, _viewData.Description);
        RenderStats(_viewData.Stats);
        RefreshImage();
    }

    private void RenderStats(IReadOnlyList<MushroomStatViewData> stats)
    {
        if (statsRoot == null)
            return;

        ClearChildren(statsRoot);

        if (stats == null || statItemPrefab == null)
            return;

        EnsureIconLookup();

        foreach (var stat in stats)
        {
            var statItem = Instantiate(statItemPrefab, statsRoot);
            var icon = ResolveIcon(stat);
            statItem.SetInfo(icon, stat.Label, stat.Value);
        }
    }

    private void OnPointerEnter()
    {
        _isPointerOver = true;
        RefreshImage();
    }

    private void OnPointerExit()
    {
        _isPointerOver = false;
        RefreshImage();
    }

    private void RefreshImage()
    {
        if (mushroomImageController == null)
            return;

        var sprite = _viewData.GetSprite(_mode, _isPointerOver);
        mushroomImageController.SetSprite(sprite);
    }

    private Sprite ResolveIcon(MushroomStatViewData stat)
    {
        if (stat.Icon != null)
            return stat.Icon;

        _iconLookup.TryGetValue(stat.Type, out var icon);
        return icon;
    }

    private void SubscribeToPointerEvents()
    {
        if (mushroomImageController == null || _pointerSubscribed)
            return;

        mushroomImageController.PointerEnter += OnPointerEnter;
        mushroomImageController.PointerExit += OnPointerExit;
        _pointerSubscribed = true;
    }

    private void UnsubscribeFromPointerEvents()
    {
        if (mushroomImageController == null || !_pointerSubscribed)
            return;

        mushroomImageController.PointerEnter -= OnPointerEnter;
        mushroomImageController.PointerExit -= OnPointerExit;
        _pointerSubscribed = false;
    }

    private static void SetText(TMP_Text field, string value)
    {
        if (field != null)
        {
            field.text = value ?? string.Empty;
        }
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(child.gameObject);
            }
            else
#endif
            {
                Destroy(child.gameObject);
            }
        }
    }
}
