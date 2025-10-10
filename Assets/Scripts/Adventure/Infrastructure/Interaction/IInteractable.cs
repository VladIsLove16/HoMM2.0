namespace Adventure.Infrastructure.Interaction
{
    public interface IInteractable
    {
        void Interact(PlayerInteractionContext context);
        string GetPrompt();
    }
}
