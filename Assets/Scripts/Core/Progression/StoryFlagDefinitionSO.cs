using UnityEngine;

namespace Adventure.Domain.Progression
{
    [CreateAssetMenu(menuName = "Adventure/Progression/Story Flag", fileName = "StoryFlag")]
    public sealed class StoryFlagDefinitionSO : ScriptableObject
    {
        [SerializeField] private string id;
        [TextArea]
        [SerializeField] private string description;

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Description => description;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = name;
            }
        }
#endif
    }
}
