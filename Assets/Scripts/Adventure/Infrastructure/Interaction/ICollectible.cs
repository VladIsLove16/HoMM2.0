namespace Adventure.Infrastructure.Interaction
{
    public interface ICollectible
    {
        bool CanCollect { get; }
        void Collect();
    }
}
