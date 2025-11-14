using System;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    [RequireComponent(typeof(Animator))]
    public sealed partial class NpcBattleAnimationController : MonoBehaviour, IBattleSequencePlayer
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string greetTrigger = "Greet";
        [SerializeField] private string battleStartTrigger = "StartBattle";
        [SerializeField] private string battleWonTrigger = "BattleLost";
        [SerializeField] private string battleLostTrigger = "BattleWon";
        [SerializeField] private string byeTrigger = "Bye";

        private Action _onSequenceComplete;
        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        public void PlayAnimation(NpcAnimationType animationType, Action onSequenceComplete = null)
        {
            string battleTrigger = ResolveAnimation(animationType);
            if (animator == null || string.IsNullOrWhiteSpace(battleTrigger))
            {
                if(onSequenceComplete != null)
                    onSequenceComplete?.Invoke();
                return;
            }

            _onSequenceComplete = onSequenceComplete;
            animator.SetTrigger(battleTrigger);
        }

        /// <summary>
        /// Invoked from animation events. Once the configured event fires we continue loading.
        /// </summary>
        public void HandleAnimationEvent(string eventName)
        {
            //if (!string.Equals(eventName, battleReadyEvent, StringComparison.OrdinalIgnoreCase))
            //    return;

            var callback = _onSequenceComplete;
            _onSequenceComplete = null;
            callback?.Invoke();
        }
        private string ResolveAnimation(NpcAnimationType animationType)
        {
            switch(animationType)
            {
                case NpcAnimationType.Greeting:
                    return greetTrigger;
                case NpcAnimationType.BattleStart:
                    return battleStartTrigger;
                case NpcAnimationType.BattleLost:
                    return battleLostTrigger;
                case NpcAnimationType.BattleWon:
                    return battleWonTrigger;
                case NpcAnimationType.Bye:
                    return byeTrigger;
                default:
                    return string.Empty;
            }
        }
    }
}
