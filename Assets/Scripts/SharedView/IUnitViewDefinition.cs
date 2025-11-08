public interface IUnitViewDefinition<TAsset> where TAsset : UnityEngine.Object
{
    bool TryGetAsset(UnitType type, out TAsset asset);
}
