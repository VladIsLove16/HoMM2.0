namespace Game.Achievements
{
    public sealed class ConditionProgress
    {
        public ConditionProgress(string conditionId)
        {
            ConditionId = conditionId;
        }

        public string ConditionId { get; }
        public int CurrentValue { get; set; }
        public bool IsCompleted { get; set; }

        public void Reset()
        {
            CurrentValue = 0;
            IsCompleted = false;
        }
    }
}
