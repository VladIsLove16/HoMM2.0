using Unity.Behavior;

namespace Adventure.Infrastructure.Dialog
{
    [BlackboardEnum]
    public enum NpcAnimationType
    {
        Greeting,
        Talking,
        BattleStart,
        BattleWon,
        BattleLost,
        Bye,
        StartWalkingWithRotation,
        LookAround
    }
}
