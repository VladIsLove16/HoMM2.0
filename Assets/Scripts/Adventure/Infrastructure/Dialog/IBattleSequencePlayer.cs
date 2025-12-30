using System;
using static Adventure.Infrastructure.Dialog.NpcAnimationController;

namespace Adventure.Infrastructure.Dialog
{
    /// <summary>
    /// Provides a hook for NPC-specific animation sequences that must finish before battle scene activation.
    /// </summary>
    public interface IBattleSequencePlayer
    {
        /// <summary>
        /// Plays the pre-battle sequence. The provided callback must be invoked once the sequence finishes so loading can continue.
        /// </summary>
        void PlayAnimation(NpcAnimationType animationType, Action onSequenceComplete);
    }
}
