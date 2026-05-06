using Unity.Behavior;

namespace Adventure.Infrastructure.Dialog
{
    [BlackboardEnum]
    public enum NpcAnimationType
    {
        Greeting,
        Grabbing,
        Talking,
        BattleStart,
        BattleWon,
        BattleLost,
        Bye,
        StartWalkingWithRotation,
        LookAround
    }
}
