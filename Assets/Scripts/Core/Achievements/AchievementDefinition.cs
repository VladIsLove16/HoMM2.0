using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Achievements
{
    [CreateAssetMenu(menuName = "Game/Achievements/Definition", fileName = "AchievementDefinition")]
    public sealed class AchievementDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private LocalizedString titleLocalized;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private LocalizedString descriptionLocalized;
        [SerializeField] private Sprite icon;
        [SerializeField] private AchievementCategory category = AchievementCategory.General;
        [SerializeField] private AchievementCompletionMode completionMode = AchievementCompletionMode.All;
        [SerializeField] private bool isSecret;
        [SerializeField] private int rewardCurrencyAmount;
        [SerializeField] private List<AchievementConditionSO> conditions = new();

        public string Id => id;
        public string Title => title;
        public string Description => description;
        public LocalizedString TitleLocalized => titleLocalized;
        public LocalizedString DescriptionLocalized => descriptionLocalized;
        public Sprite Icon => icon;
        public AchievementCategory Category => category;
        public AchievementCompletionMode CompletionMode => completionMode;
        public bool IsSecret => isSecret;
        public int RewardCurrencyAmount => rewardCurrencyAmount;
        public IReadOnlyList<AchievementConditionSO> Conditions => conditions;
    }
}
