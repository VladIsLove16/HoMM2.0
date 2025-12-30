using Unity.Behavior;

public sealed partial class NpcPatrolAction
{
    [BlackboardEnum]
    public enum PatrolPhase
    {
        None,
        RotatingToTarget,
        Walking,
        Grabbing,
        NavigatingToGrabItem,
        Talking,
        Sad,
        Happy,
        LookAround
    }
}
