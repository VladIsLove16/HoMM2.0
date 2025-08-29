using System.Collections.Generic;
using Zenject;

public class UnitViewModelFactory
{
   [Inject] private IReadOnlyDictionary<UnitType,UnitDefinitionSO> _dataMap;
    [Inject] IMaterialProvider unitViewModelMaterialsInfo;
    public UnitViewModelFactory()
    {
        
    }

    public UnitViewModel Create(UnitModel model)
    {
        UnitDefinitionSO so = _dataMap[model.UnitType.Value];
        UnitViewModel unitViewModel = new UnitViewModel(model, unitViewModelMaterialsInfo);
        return unitViewModel;
    }
}
