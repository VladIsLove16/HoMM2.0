using System;

namespace SharedView.Audio
{
    [Serializable]
    public sealed class UnitAudioProfile
    {
        public GameAudioClipSet MeleeAttack = new();
        public GameAudioClipSet DamageTaken = new();
        public GameAudioClipSet RangedAttack = new();

        public GameAudioClipSet GetAttackClips(CombatSfxType type)
        {
            return type switch
            {
                CombatSfxType.Ranged => RangedAttack,
                CombatSfxType.Melee => MeleeAttack,
                _ => MeleeAttack
            };
        }
    }
}
