namespace Adventure.Infrastructure.Interaction
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        void Interact(PlayerInteractionContext context);
        string GetPrompt();
    }
}
