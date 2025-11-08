using UnityEngine;

[CreateAssetMenu(menuName = "Units/Unit Shared data", fileName =  "Unit Shared data")]
public class UnitSharedDataSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [TextArea][SerializeField] private string description;
    [Header("Presentation")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite hoveredIcon;
    [SerializeField] private Sprite humanizedIcon;
    [SerializeField] private Sprite humanizedHoveredIcon;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public Sprite HoveredIcon => hoveredIcon;
    public Sprite HumanizedIcon => humanizedIcon;
    public Sprite HumanizedHoveredIcon => humanizedHoveredIcon;
}