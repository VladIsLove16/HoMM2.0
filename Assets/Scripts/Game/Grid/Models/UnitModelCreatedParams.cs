public class UnitModelCreatedParams
{
    public UnitModelLegacy UnitModel { get; }
    public UnitModelCreatedParams(UnitModelLegacy unitModel)
    {
        this.UnitModel = unitModel;
    }
}