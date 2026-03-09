namespace Game.Achievements
{
    public interface IAchievementStorage
    {
        AchievementProgressStorageData Load();
        void Save(AchievementProgressStorageData data);
        void Clear();
    }
}
