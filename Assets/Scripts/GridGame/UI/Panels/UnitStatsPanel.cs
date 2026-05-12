using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using SharedView;

public class UnitStatsPanel : CanvasGroupVisibilityPanelBase, IDisposable
{
    [SerializeField] Canvas canvas;
    [SerializeField] Button closeButton;
    [SerializeField] Image statusEffectIconPrefab;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI maxHealthText;
    [SerializeField] TextMeshProUGUI attackDamageText;
    [SerializeField] TextMeshProUGUI moveSpeedText;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] Transform statusEffectIconsParent;
    [SerializeField] Vector3 showOffset;
    [SerializeField] Vector3 screenEdgeOffset;
    [SerializeField] bool enableOnStart;

    // Removed direct GameView3D dependency - should work through GameViewModel events
    private UnitStatsViewModel _vm;
    private CompositeDisposable _disposables  = new CompositeDisposable();
    private readonly List<Image> _statusEffectIcons = new();
    [Inject] private StatusEffectDatas statusEffectDatas;
    [Inject] private GameViewModel _gameViewModel;
    private IReadOnlyDictionary<StatusEffectType, StatusEffectData> _statusEffectDict;
    private List<StatusEffectViewModel> _statusEffectViewModels;

    private void Start()
    {
        _statusEffectDict = statusEffectDatas != null
            ? statusEffectDatas.ToDictionary()
            : new Dictionary<StatusEffectType, StatusEffectData>();

        if (statusEffectDatas == null)
        {
            Debug.LogWarning("[UnitStatsPanel] StatusEffectDatas is not injected or assigned. Status effect icons will be hidden.", this);
        }

        _statusEffectViewModels = new List<StatusEffectViewModel>();
        if (_gameViewModel != null)
        {
            _gameViewModel.UnitStatsRequested += OnGameViewModel_UnitStatsRequested;
        }
        else
        {
            Debug.LogWarning("[UnitStatsPanel] GameViewModel is not injected. Unit stats panel will not receive selection events.", this);
        }

        ToggleStartingVivsibilty();
    }

    private void ToggleStartingVivsibilty()
    {
        if (enableOnStart)
            ShowPanelImmediate();
        else
            HidePanelImmediate();
    }

    private void OnGameViewModel_UnitStatsRequested(UnitViewModel unit)
    {
        if (unit == null)
            return;

        var vm = new UnitStatsViewModel(unit.Model);
        Init(vm);
    }

    public void Init(UnitStatsViewModel vm)
    {
        if (vm == null)
            return;

        _vm = vm;

        _vm.Health.Subscribe(val => SetText(healthText, val)).AddTo(_disposables);
        _vm.MaxHealth.Subscribe(val => SetText(maxHealthText, val)).AddTo(_disposables);
        _vm.AttackDamage.Subscribe(val => SetText(attackDamageText, val)).AddTo(_disposables);
        _vm.MoveSpeed.Subscribe(val => SetText(moveSpeedText, val)).AddTo(_disposables);
        _vm.Amount.Subscribe(OnAmountChanged).AddTo(_disposables);

        _vm.StatusEffects.ObserveAdd().Subscribe(e => AddStatusEffect(e.Value)).AddTo(_disposables);
        _vm.StatusEffects.ObserveReset().Subscribe(_ => RefreshStatusEffects()).AddTo(_disposables);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        RefreshStatusEffects();
        Show();
    }

    private void OnAmountChanged(int obj)
    {
        if (amountText == null)
            return;

        amountText.text = obj.ToString();
    }

    private void AddStatusEffect(StatusEffectViewModel e)
    {
        if (e == null || statusEffectIconPrefab == null || statusEffectIconsParent == null)
            return;

        if (_statusEffectDict == null || !_statusEffectDict.TryGetValue(e.Type, out var data) || data == null)
            return;

        _statusEffectViewModels.Add(e);
        var icon = Instantiate(statusEffectIconPrefab, statusEffectIconsParent);
        icon.sprite = data.Sprite;
        _statusEffectIcons.Add(icon);
    }


    private void RefreshStatusEffects()
    {
        foreach (var icon in _statusEffectIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }
        _statusEffectIcons.Clear();

        if (_vm == null)
            return;

        foreach (var effect in _vm.StatusEffects)
            AddStatusEffect(effect);
    }

    public void Show(Vector3 worldPosition)
    {
        if (Camera.main == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition) + showOffset;

        // ��������� �������� ������
        RectTransform panelRect = GetComponent<RectTransform>();
        if (panelRect == null || canvas == null)
            return;

        Vector2 panelSize = panelRect.sizeDelta * canvas.scaleFactor;

        // ����������� ������� � �������� ������
        float clampedX = Mathf.Clamp(screenPos.x, panelSize.x / 2 + screenEdgeOffset.x, Screen.width - panelSize.x / 2 - screenEdgeOffset.x);
        float clampedY = Mathf.Clamp(screenPos.y, panelSize.y / 2 + screenEdgeOffset.y, Screen.height - panelSize.y / 2 - screenEdgeOffset.y);

        panelRect.position = new Vector3(clampedX, clampedY, screenPos.z);
    }


    public void Show()
    {
        ShowPanel(); 
        // Position should be set by the caller or through GameViewModel events
        Show(Vector3.zero); // Default position
    }
    [ContextMenu("Hide")]
    public void Hide()
    {
        HidePanel();
        _vm?.Dispose();
        _disposables.Clear();
    }

    private static void SetText(TextMeshProUGUI text, int value)
    {
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    public void Dispose()
    {
        if (_gameViewModel != null)
        {
            _gameViewModel.UnitStatsRequested -= OnGameViewModel_UnitStatsRequested;
        }
        _vm?.Dispose();
        _disposables.Dispose();
    }

    protected override void OnDestroy()
    {
        Dispose();
        base.OnDestroy();
    }
}
