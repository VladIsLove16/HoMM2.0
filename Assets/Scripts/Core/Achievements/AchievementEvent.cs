using System;

namespace Game.Achievements
{
    public readonly struct AchievementEvent
    {
        public AchievementEvent(string id, int amount = 0, object payload = null)
        {
            Id = string.IsNullOrWhiteSpace(id) ? string.Empty : id;
            Amount = amount;
            Payload = payload;
        }

        public string Id { get; }
        public int Amount { get; }
        public object Payload { get; }

        public bool Matches(string expectedId)
        {
            return !string.IsNullOrWhiteSpace(expectedId)
                   && string.Equals(Id, expectedId, StringComparison.OrdinalIgnoreCase);
        }
    }
}
