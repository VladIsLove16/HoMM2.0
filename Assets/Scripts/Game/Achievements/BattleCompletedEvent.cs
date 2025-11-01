namespace Adventure.Infrastructure.Events
{
    public readonly struct BattleCompletedEvent
    {
        public BattleCompletedEvent(bool playerWon)
        {
            PlayerWon = playerWon;
        }

        public bool PlayerWon { get; }
    }
}
