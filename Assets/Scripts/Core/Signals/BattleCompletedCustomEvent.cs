namespace Game.Events
{
    public readonly struct BattleCompletedCustomEvent
    {
        public BattleCompletedCustomEvent(bool playerWon)
        {
            PlayerWon = playerWon;
        }

        public bool PlayerWon { get; }
    }
}
