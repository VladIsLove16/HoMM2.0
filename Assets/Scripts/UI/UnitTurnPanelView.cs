using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

public class UnitTurnPanelView : MonoBehaviour
{
    [SerializeField] UnitIconController unitIconpf;
    [SerializeField] GameObject parent;
    [SerializeField] UnitIconController ActiveCharacterController;
    [SerializeField] List<UnitDefinitionSO> unitDatas;
    private Dictionary<UnitIconController, ICombatObject> unitModels = new();
    [SerializeField] private List<UnitIconController> unitIconControllers = new List<UnitIconController>();
    private UnitTurnPanelViewModel _combatSystemViewModel;

    [Inject]
    public void Initialize(UnitTurnPanelViewModel combatSystemViewModel)
    {
        _combatSystemViewModel = combatSystemViewModel;
        _combatSystemViewModel.ActiveUnit.Subscribe(OnActiveObjectChanged);
        _combatSystemViewModel.CombatUnitsAdded += OnCombatUnitsAdded;
        List<UnitTurnInfo> combatQueue = _combatSystemViewModel.turnList;
        CreatePanelInfo(combatQueue);
    }

    [Button]
    private void Clear()
    {
        for (int i = parent.transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(parent.transform.GetChild(i).gameObject);
#else
            Destroy(parent.transform.GetChild(i).gameObject);
#endif
        }
        unitModels.Clear();
        unitIconControllers.Clear();
        ActiveCharacterController.Clear();
    }
    private void OnCombatUnitsAdded(UnitTurnInfo info)
    {
        AddUnit(info.Unit,info.Turn);
    }

    [Button]
    public void OnActiveObjectChanged(ICombatObject combatUnit)
    {
        UpdateActiveCharacter();
        RemoveObject();
    }

    public void CreatePanelInfo(List<UnitTurnInfo> unitcontents)
    {
        foreach (var model in unitcontents)
        {
            AddUnit(model.Unit,model.Turn);
        }
    }

    private void AddUnit(ICombatObject model,int turn)
    {
        Sprite sprite;
        bool isBlueTeam;
        GetData(model, out sprite, out isBlueTeam);
        UnitIconController unitIconController = Create(model, sprite, isBlueTeam, turn);
    }

    private void GetData(ICombatObject model, out Sprite sprite, out bool isBlueTeam)
    {
        UnitDefinitionSO unitDefinitionSO = unitDatas.First(x => x.UnitType == model.UnitType);
        sprite = unitDefinitionSO.UnitIcon;
        isBlueTeam = model.IsBlueTeam;
    }

    private UnitIconController Create(ICombatObject model, Sprite sprite, bool isBlueTeam, int turn)
    {
        UnitIconController unitIconController = Create();
        unitIconController.SetInfo(sprite, isBlueTeam);
        unitIconController.Link(model, turn);
        unitIconControllers.Add(unitIconController);
        unitModels[unitIconController] = model;
        return unitIconController;
    }

    private void UpdateActiveCharacter()
    {
        if (unitIconControllers.Count > 0)
        {
            ActiveCharacterController.SetInfo(unitIconControllers[0].Sprite, unitIconControllers[0].IsBlueTeam);
            ActiveCharacterController.Link(unitIconControllers[0].CombatUnit, _combatSystemViewModel.TurnNumber.Value);
            unitModels[unitIconControllers[0]] = unitIconControllers[0].CombatUnit;
        }
    }

    private void RemoveObject()
    {
        if (unitIconControllers == null || unitIconControllers.Count == 0)
            return;
        UnitIconController unitIconController = unitIconControllers[0];
        if (unitModels != null)
        {
            unitModels.TryGetValue(unitIconController, out var model);
            if (model != null)
            {
                unitModels.Remove(unitIconController);
            }
        }
        if (unitIconController != null)
        {
            unitIconControllers.Remove(unitIconController);
#if UNITY_EDITOR
            DestroyImmediate(unitIconController.gameObject);
#else
            Destroy(unitIconController.gameObject);
#endif
        }
    }

    [Button]
    private void AddBlueTeamIcons()
    {
        foreach (var item in unitDatas)
        {
            UnitIconController iconController = Create();
            iconController.SetInfo(item.UnitIcon,true);
            unitIconControllers.Add(iconController);
        }
        UpdateActiveCharacter();
    }

    private UnitIconController Create()
    {
        UnitIconController unitIconController = Instantiate(unitIconpf, parent.transform);
        return unitIconController;
    }

}
