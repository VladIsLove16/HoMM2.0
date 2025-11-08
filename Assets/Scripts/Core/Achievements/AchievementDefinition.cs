using System;
using UnityEngine;

namespace Game.Achievements
{
    [Serializable]
    public sealed class AchievementDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string Title => title;
        public string Description => description;
        public Sprite Icon => icon;
    }
}