namespace Adventure.Infrastructure.Interaction
{
    /// <summary>
    /// Represents an interactable that can be collected by the player.
    /// </summary>
    public interface ICollectible : IInteractable
    {
        /// <summary>
        /// Indicates whether the item can currently be collected.
        /// </summary>
        bool CanCollect { get; }

        /// <summary>
        /// Performs the collection behaviour.
        /// </summary>
        /// <param name="context">Context describing the player interaction.</param>
        void Collect(PlayerInteractionContext context);
    }
}
