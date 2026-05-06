namespace SharedView.Audio
{
    public interface IUnitAudioProfileProvider
    {
        bool TryGetAudioProfile(UnitType type, out UnitAudioProfile profile);
    }
}
