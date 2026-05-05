namespace Adventure.Domain.Progression
{
    public interface IStoryFlagsStore
    {
        StoryFlagsSnapshot Load();
        void Save(StoryFlagsSnapshot snapshot);
        void Clear();
    }
}
