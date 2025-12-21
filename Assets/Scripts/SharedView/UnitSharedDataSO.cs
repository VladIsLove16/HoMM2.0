using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Units/Unit Shared data", fileName =  "Unit Shared data")]
public class UnitSharedDataSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private LocalizedString displayNameLocalized;
    [TextArea][SerializeField] private string description;
    [SerializeField] private LocalizedString descriptionLocalized;
    [Header("Presentation")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite hoveredIcon;
    [SerializeField] private Sprite humanizedIcon;
    [SerializeField] private Sprite humanizedHoveredIcon;

    public string DisplayName => ResolveLocalizedString(displayNameLocalized, displayName);
    public string Description => ResolveLocalizedString(descriptionLocalized, description);
    public LocalizedString DisplayNameEntry => displayNameLocalized;
    public LocalizedString DescriptionEntry => descriptionLocalized;
    public Sprite Icon => icon;
    public Sprite HoveredIcon => hoveredIcon;
    public Sprite HumanizedIcon => humanizedIcon;
    public Sprite HumanizedHoveredIcon => humanizedHoveredIcon;

    private static string ResolveLocalizedString(LocalizedString entry, string fallback)
    {
        if (entry != null && !entry.IsEmpty)
        {
            var localized = entry.GetLocalizedString();
            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return fallback ?? string.Empty;
    }
}
