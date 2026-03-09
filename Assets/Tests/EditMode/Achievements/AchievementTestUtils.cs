using System.Reflection;
using Game.Achievements;

namespace Tests.EditMode.Achievements
{
    internal sealed class AchievementDefinitionBuilder
    {
        private readonly AchievementDefinition _definition = new AchievementDefinition();

        public static AchievementDefinitionBuilder New() => new AchievementDefinitionBuilder();

        public AchievementDefinitionBuilder WithId(string id)
        {
            SetField("id", id);
            return this;
        }

        public AchievementDefinitionBuilder WithTitle(string title)
        {
            SetField("title", title);
            return this;
        }

        public AchievementDefinitionBuilder WithDescription(string description)
        {
            SetField("description", description);
            return this;
        }

        public AchievementDefinitionBuilder WithConditions(params AchievementConditionSO[] conditions)
        {
            SetField("conditions", conditions == null ? null : new System.Collections.Generic.List<AchievementConditionSO>(conditions));
            return this;
        }

        public AchievementDefinitionBuilder WithCompletionMode(AchievementCompletionMode mode)
        {
            SetField("completionMode", mode);
            return this;
        }

        public AchievementDefinition Create() => _definition;

        private void SetField(string fieldName, object value)
        {
            typeof(AchievementDefinition)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(_definition, value);
        }
    }
}
