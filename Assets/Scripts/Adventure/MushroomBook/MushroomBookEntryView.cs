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

public class MushroomBookEntryView : MonoBehaviour, IEntryView, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private MushroomImageController mushroomImageController;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform statsRoot;
    [SerializeField] private UnitSingleStatPanel statItemPrefab;
    [SerializeField] private StatIcons statIcons;
    [SerializeField] private Button DropButton;
    [SerializeField] private RectTransform dropBounds;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private CanvasGroup canvasGroup;

    private readonly Dictionary<UnitStatType, Sprite> _iconLookup = new();
    private MushroomBookEntryViewModel _viewData = MushroomBookEntryViewModel.Empty;
    private PresentationMode _mode = PresentationMode.Normal;
    private bool _isPointerOver;
    private bool _pointerSubscribed;
    private bool _isDragging;
    private RectTransform _rectTransform;
    private RectTransform _dragLayerRect;
    private Transform _originalParent;
    private int _originalSiblingIndex;
    private Vector2 _originalAnchoredPosition;
    private Vector2 _dragOffsetPosition;
    private int _slotIndex = -1;
    private GameObject _placeholder;
    private CanvasGroup _placeholderCanvasGroup;
    private Dictionary<string, TMP_Text> _placeholderTexts;
    private Dictionary<string, Image> _placeholderImages;

    public PresentationMode Mode => _mode;
    public string Title => nameText != null ? nameText.text : string.Empty;
    public string Description => descriptionText != null ? descriptionText.text : string.Empty;
    public string AmountText => amountText != null ? amountText.text : string.Empty;
    public int StatItemCount => statsRoot != null ? statsRoot.childCount : 0;
    public Sprite CurrentSprite => mushroomImageController != null ? mushroomImageController.CurrentSprite : null;
    public Action<int, int> SwapRequested;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        BuildIconLookup();
        SubscribeToPointerEvents();
        SubscribeToButtonClicks();
    }

    private void SubscribeToButtonClicks()
    {
        if (DropButton != null)
        {
            DropButton.onClick.AddListener(OnDropClicked);
        }
    }

    private void OnDropClicked()
    {
        _viewData?.Drop();
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
        string viewName = _viewData.DisplayName == string.Empty ? "Unnamed unit." : _viewData.DisplayName;
        string desc = _viewData.Description == string.Empty ? "No description for this unit yet. " : _viewData.Description;

        SetText(nameText, _viewData.DisplayName);
        SetText(amountText, _viewData.Amount.ToString());
        SetText(descriptionText, desc);
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

    public void SetSlotIndex(int index)
    {
        _slotIndex = index;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        if (_rectTransform == null)
            return;

        _isDragging = true;
        _originalParent = _rectTransform.parent;
        _originalSiblingIndex = _rectTransform.GetSiblingIndex();
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
        _dragLayerRect = ResolveDragLayer();
        CreatePlaceholder();
        SyncPlaceholderVisuals();
        SetPlaceholderVisible(true);
        if (_dragLayerRect != null)
        {
            _rectTransform.SetParent(_dragLayerRect, true);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dragLayerRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                _dragOffsetPosition = _rectTransform.anchoredPosition - localPoint;
            }
            else
            {
                _dragOffsetPosition = Vector2.zero;
            }
        }
        else
        {
            _dragOffsetPosition = (Vector2)_rectTransform.position - eventData.position;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        UpdateDragPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        UpdateDragPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        _isDragging = false;
        var swapTarget = FindSwapTarget(eventData);
        if (_originalParent != null && swapTarget != null && swapTarget.transform.parent == _originalParent)
        {
            SwapRequested?.Invoke(_slotIndex, swapTarget._slotIndex);
            _rectTransform.SetParent(_originalParent, false);
            _rectTransform.SetSiblingIndex(_originalSiblingIndex);
        }
        else if (_originalParent != null)
        {
            _rectTransform.SetParent(_originalParent, false);
            _rectTransform.SetSiblingIndex(_originalSiblingIndex);
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
        }

        SetPlaceholderVisible(false);

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (ShouldDrop(eventData))
        {
            _viewData?.Drop();
        }
    }

    private Sprite ResolveIcon(MushroomStatViewData stat)
    {
        if (stat.Icon != null)
            return stat.Icon;

        _iconLookup.TryGetValue(stat.Type, out var icon);
        return icon;
    }

    private RectTransform ResolveDragLayer()
    {
        if (dragLayer != null)
            return dragLayer;

        var canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.transform as RectTransform : null;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void UpdateDragPosition(PointerEventData eventData)
    {
        if (_rectTransform == null)
            return;

        var targetRect = _dragLayerRect;
        if (targetRect == null)
        {
            _rectTransform.position = eventData.position + _dragOffsetPosition;
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetRect,
                eventData.position,
                eventData.pressEventCamera,
                out var localPoint))
        {
            _rectTransform.anchoredPosition = localPoint + _dragOffsetPosition;
        }
    }

    private bool ShouldDrop(PointerEventData eventData)
    {
        if (eventData == null || dropBounds == null)
            return false;

        return !RectTransformUtility.RectangleContainsScreenPoint(
            dropBounds,
            eventData.position,
            eventData.pressEventCamera);
    }
    private void RemovePlaceholder()
    {
        // kept for compatibility; we now reuse the placeholder
        SetPlaceholderVisible(false);
    }

    private void SetPlaceholderVisible(bool visible)
    {
        if (_placeholder == null)
            return;
        if (visible)
        {
            _placeholder.transform.SetSiblingIndex(_originalSiblingIndex);
        }
        if (_placeholder.transform.parent != _originalParent)
        {
            _placeholder.transform.SetParent(_originalParent, false);
        }
        _placeholder.SetActive(visible);
    }

    private void CreatePlaceholder()
    {
        if (_placeholder != null || _originalParent == null || _rectTransform == null)
            return;

        _placeholder = Instantiate(_rectTransform.gameObject, _originalParent, false);
        _placeholder.name = $"{name}_Placeholder";

        var placeholderView = _placeholder.GetComponent<MushroomBookEntryView>();
        if (placeholderView != null)
        {
            placeholderView.enabled = false;
        }

        _placeholderCanvasGroup = _placeholder.GetComponent<CanvasGroup>() ?? _placeholder.AddComponent<CanvasGroup>();
        _placeholderCanvasGroup.alpha = 0.4f;
        _placeholderCanvasGroup.blocksRaycasts = false;
        _placeholderCanvasGroup.interactable = false;

        _placeholderTexts = new Dictionary<string, TMP_Text>();
        foreach (var text in _placeholder.GetComponentsInChildren<TMP_Text>(true))
        {
            _placeholderTexts[GetPath(text.transform)] = text;
        }

        _placeholderImages = new Dictionary<string, Image>();
        foreach (var img in _placeholder.GetComponentsInChildren<Image>(true))
        {
            _placeholderImages[GetPath(img.transform)] = img;
        }

        _placeholder.transform.SetSiblingIndex(_originalSiblingIndex);
        SetPlaceholderVisible(false);
    }

    private void SyncPlaceholderVisuals()
    {
        if (_placeholder == null)
            return;

        if (_placeholder.transform.parent != _originalParent)
        {
            _placeholder.transform.SetParent(_originalParent, false);
        }
        _placeholder.transform.SetSiblingIndex(_originalSiblingIndex);

        // Sync texts
        foreach (var text in GetComponentsInChildren<TMP_Text>(true))
        {
            var path = GetPath(text.transform);
            if (_placeholderTexts != null && _placeholderTexts.TryGetValue(path, out var target))
            {
                target.text = text.text;
            }
        }

        // Sync images (sprite + color)
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            var path = GetPath(img.transform);
            if (_placeholderImages != null && _placeholderImages.TryGetValue(path, out var target))
            {
                target.sprite = img.sprite;
                target.color = new Color(img.color.r, img.color.g, img.color.b, img.color.a * 0.5f);
                target.preserveAspect = img.preserveAspect;
                target.type = img.type;
            }
        }
    }

    private static string GetPath(Transform t)
    {
        var path = t.name;
        var current = t.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    private MushroomBookEntryView FindSwapTarget(PointerEventData eventData)
    {
        if (eventData == null || EventSystem.current == null)
            return null;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            if (result.gameObject == gameObject)
                continue;

            var view = result.gameObject.GetComponentInParent<MushroomBookEntryView>();
            if (view != null && view != this)
            {
                return view;
            }
        }

        return null;
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
