using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class UnitViewModelFactory
{
    private Dictionary<UnitType,UnitDefinitionSO> _dataMap;
    public UnitViewModelFactory(Dictionary<UnitType, UnitDefinitionSO> dataMap)
    {
        _dataMap = dataMap;
    }

    public UnitViewModel Create(UnitModel model)
    {
        UnitDefinitionSO so = _dataMap[model.UnitType.Value];
        UnitViewModelMaterialsInfo unitViewModelMaterialsInfo = new(so);
        UnitViewModel unitViewModel = new UnitViewModel(model, unitViewModelMaterialsInfo);
        return unitViewModel;
    }
}
public class UnitViewModelMaterialsInfo
{
    public UnitViewModelMaterialsInfo(UnitDefinitionSO unitDefinitionSO)
    {
        BlueTeamMaterial = unitDefinitionSO.BlueTeamMaterial;
        HoveredBlueTeamMaterial = unitDefinitionSO.HoveredBlueTeamMaterial;
        RedTeamMaterial = unitDefinitionSO.RedTeamMaterial;
        HoveredRedTeamMaterial = unitDefinitionSO.HoveredRedTeamMaterial;
    }
    public Material BlueTeamMaterial { get; internal set; }
    public Material HoveredBlueTeamMaterial { get; internal set; }
    public Material RedTeamMaterial { get; internal set; }
    public Material HoveredRedTeamMaterial { get; internal set; }
}