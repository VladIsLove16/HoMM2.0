namespace Adventure.Infrastructure.Interaction
{
    public readonly struct PlayerInteractionContext
    {
        public PlayerInteractionContext(UnityEngine.Transform playerTransform)
        {
            PlayerTransform = playerTransform;
        }

        public UnityEngine.Transform PlayerTransform { get; }
    }
}
