namespace Adventure.Domain.Inventory
{
    public sealed class MushroomItem
    {
        public MushroomItem(string id, string displayName, string description)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
    }
}
