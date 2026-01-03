using Unity.Behavior;

[BlackboardEnum]
public enum NPCState
{
    None,
    RotatingToTarget,
    Walking,
    Grabbing,
    NavigatingToGrabItem,
    NavigatingToTalk,
    Talking,
    Sad,
    Happy,
    Fighting,
    Hiphoping,
    LookingAround
}
