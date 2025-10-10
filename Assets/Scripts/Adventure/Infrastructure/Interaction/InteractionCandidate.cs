namespace Adventure.Infrastructure.Interaction
{
    public sealed partial class PlayerInteractionController
    {
        private readonly struct InteractionCandidate
        {
            public InteractionCandidate(InteractionType type, IInteractable interactable, ICollectible collectible, string prompt)
            {
                Type = type;
                Interactable = interactable;
                Collectible = collectible;
                Prompt = prompt;
            }

            public InteractionType Type { get; }
            public IInteractable Interactable { get; }
            public ICollectible Collectible { get; }
            public string Prompt { get; }
        }
    }
}
