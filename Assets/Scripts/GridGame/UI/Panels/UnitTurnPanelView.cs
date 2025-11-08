using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using UniRx;

public class UnitTurnPanelView : MonoBehaviour
{
    [SerializeField] private UnitIconController unitIconPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private UnitIconController activeCharacterController;
    private GridUnitAssetMap _unitAssets;

    private readonly Dictionary<UnitIconController, ICombatObject> _iconToUnit = new();
    private readonly List<UnitIconController> _icons = new();

    private UnitTurnPanelViewModel _vm;
    private CompositeDisposable _disposables = new();

    [Inject]
    public void Initialize(UnitTurnPanelViewModel vm, GridUnitAssetMap unitAssets )
    {
        _vm = vm;
        _unitAssets = unitAssets;

        _vm.ActiveUnit.Subscribe(UpdateActiveCharacter).AddTo(_disposables);
        _vm.UnitAdded += AddUnitToPanel;

        // Инициализация начального состояния
        foreach (var info in _vm.TurnQueue)
            AddUnitToPanel(info);
    }

    private void AddUnitToPanel(UnitTurnInfo info)
    {
        if (_unitAssets == null || !_unitAssets.TryGetShared(info.Unit.UnitType, out var shared) || shared?.Icon == null)
        {
            Debug.LogWarning($"Unit data not found for {info.Unit.UnitType}");
            return;
        }

        var iconSprite = shared.Icon;
        var icon = Instantiate(unitIconPrefab, parent);
        icon.SetInfo(iconSprite, info.Unit.Team);
        icon.Link(info.Unit, info.Turn);

        _icons.Add(icon);
        _iconToUnit[icon] = info.Unit;
    }

    private void UpdateActiveCharacter(ICombatObject combatUnit)
    {
        if (combatUnit == null) return;

        var foundIcon = _iconToUnit.FirstOrDefault(x => x.Value == combatUnit).Key;
        if (foundIcon == null) return;

        activeCharacterController.SetInfo(foundIcon.Sprite, foundIcon.Team);
        activeCharacterController.Link(combatUnit, _vm.TurnNumber.Value);

        RemoveIcon(foundIcon);
    }

    private void RemoveIcon(UnitIconController icon)
    {
        if (_icons.Contains(icon))
        {
            _icons.Remove(icon);
            _iconToUnit.Remove(icon);
            Destroy(icon.gameObject);
        }
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
        _vm.UnitAdded -= AddUnitToPanel;
    }
}
