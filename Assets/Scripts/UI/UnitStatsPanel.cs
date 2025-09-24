using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UnitStatsPanel : MonoBehaviour, IDisposable
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

    // Removed direct GameView3D dependency - should work through GameViewModel events
    private UnitStatsViewModel _vm;
    private CompositeDisposable _disposables  = new CompositeDisposable();
    private readonly List<Image> _statusEffectIcons = new();
    [Inject] private List<StatusEffectData> statusEffectDatas;
    [Inject] private GameViewModel _gameViewModel;
    private Dictionary<string, StatusEffectData> _statusEffectDict;
    private List<StatusEffectViewModel> _statusEffectViewModels;

    private void Start()
    {
        _statusEffectDict = statusEffectDatas.ToDictionary(se => se.name, se => se);
        _gameViewModel.UnitStatsRequested += OnGameViewModel_UnitStatsRequested;
        Hide();
    }
    
    private void OnGameViewModel_UnitStatsRequested(UnitViewModel unit)
    {
        var vm = new UnitStatsViewModel(unit.Model);
        Init(vm);
    }

    public void Init(UnitStatsViewModel vm)
    {
        _vm = vm;

        _vm.Health.Subscribe(val => healthText.text = val.ToString()).AddTo(_disposables);
        _vm.MaxHealth.Subscribe(val => maxHealthText.text = val.ToString()).AddTo(_disposables);
        _vm.AttackDamage.Subscribe(val => attackDamageText.text = val.ToString()).AddTo(_disposables);
        _vm.MoveSpeed.Subscribe(val => moveSpeedText.text = val.ToString()).AddTo(_disposables);
        _vm.Amount.Subscribe(OnAmountChanged).AddTo(_disposables);

        _vm.StatusEffects.ObserveAdd().Subscribe(e => AddStatusEffect(e.Value)).AddTo(_disposables);
        _vm.StatusEffects.ObserveReset().Subscribe(_ => RefreshStatusEffects()).AddTo(_disposables);

        closeButton.onClick.AddListener(Hide);

        RefreshStatusEffects();
        Show();
    }

    private void OnAmountChanged(int obj)
    {
        amountText.text = obj.ToString();
    }

    private void AddStatusEffect(StatusEffectViewModel e)
    {
        _statusEffectViewModels.Add(e);
        var icon = Instantiate(statusEffectIconPrefab, statusEffectIconsParent);
        icon.sprite = _statusEffectDict[e.Name].Sprite;
        _statusEffectIcons.Add(icon);
    }


    private void RefreshStatusEffects()
    {
        foreach (var icon in _statusEffectIcons)
            Destroy(icon.gameObject);
        _statusEffectIcons.Clear();

        foreach (var effect in _vm.StatusEffects)
            AddStatusEffect(effect);
    }

    public void Show(Vector3 worldPosition)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition) + showOffset;

        // ��������� �������� ������
        RectTransform panelRect = GetComponent<RectTransform>();
        Vector2 panelSize = panelRect.sizeDelta * canvas.scaleFactor;

        // ����������� ������� � �������� ������
        float clampedX = Mathf.Clamp(screenPos.x, panelSize.x / 2 + screenEdgeOffset.x, Screen.width - panelSize.x / 2 - screenEdgeOffset.x);
        float clampedY = Mathf.Clamp(screenPos.y, panelSize.y / 2 + screenEdgeOffset.y, Screen.height - panelSize.y / 2 - screenEdgeOffset.y);

        panelRect.position = new Vector3(clampedX, clampedY, screenPos.z);
    }


    public void Show()
    {
        gameObject.SetActive(true); 
        // Position should be set by the caller or through GameViewModel events
        Show(Vector3.zero); // Default position
    }
    [ContextMenu("Hide")]
    public void Hide()
    {
        gameObject.SetActive(false);
        _vm?.Dispose();
        _disposables.Clear();
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
}
