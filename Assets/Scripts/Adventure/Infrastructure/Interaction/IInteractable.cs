namespace Adventure.Infrastructure.Interaction
{
    public interface IInteractable
    {
        void Interact(PlayerInteractionContext context);
    }

    public readonly struct PlayerInteractionContext
    {
        public PlayerInteractionContext(UnityEngine.Transform playerTransform)
        {
            PlayerTransform = playerTransform;
        }

        public UnityEngine.Transform PlayerTransform { get; }
    }
}
