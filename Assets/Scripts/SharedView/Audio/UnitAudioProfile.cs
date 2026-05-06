using System;

namespace SharedView.Audio
{
    [Serializable]
    public sealed class UnitAudioProfile
    {
        public GameAudioClipSet MeleeAttack = new();
        public GameAudioClipSet RangedAttack = new();
        public GameAudioClipSet MagicAttack = new();
        public GameAudioClipSet DamageTaken = new();

        public GameAudioClipSet GetAttackClips(CombatSfxType type)
        {
            return type switch
            {
                CombatSfxType.Ranged => RangedAttack,
                CombatSfxType.Magic => MagicAttack,
                CombatSfxType.Melee => MeleeAttack,
                _ => MeleeAttack
            };
        }
    }
}
