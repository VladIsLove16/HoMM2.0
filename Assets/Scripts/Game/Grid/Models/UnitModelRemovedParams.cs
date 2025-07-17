public class UnitModelRemovedParams
{
    public UnitModelLegacy UnitModel { get; set; }
    public UnitModelRemovedParams (UnitModelLegacy unitModel)
    {
        this.UnitModel = unitModel; 
    }
}