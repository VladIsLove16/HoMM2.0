using System;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    [RequireComponent(typeof(Animator))]
    public sealed class NpcAnimationController : MonoBehaviour, IBattleSequencePlayer
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string greetTrigger = "Greet";
        [SerializeField] private string byeTrigger = "Bye";
        [SerializeField] private string battleStartTrigger = "StartBattle";
        [SerializeField] private string talkingTrigger = "Talking";
        [SerializeField] private string battleWonTrigger = "BattleWon";
        [SerializeField] private string battleLostTrigger = "BattleLost";
        [SerializeField] private string startWalkingWithRotation = "StartWalkingWithRotation";
        [SerializeField] private string lookAroundTrigger = "LookingAround";
        [SerializeField] private string WalkingBool = "Walking";
        [SerializeField] private bool forceRendererShadows = true;

        private Action _onSequenceComplete;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            ApplyRendererShadowSettings();
        }

        private void OnEnable()
        {
            ApplyRendererShadowSettings();
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
        public void SetWalking(bool walking)
        {
            animator.SetBool(WalkingBool, walking);
        }

        private void ApplyRendererShadowSettings()
        {
            if (!forceRendererShadows)
                return;

            NpcRendererShadowSettings.Apply(gameObject);
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
                case NpcAnimationType.Talking:
                    return talkingTrigger;
                case NpcAnimationType.StartWalkingWithRotation:
                    return startWalkingWithRotation;
                case NpcAnimationType.LookAround:
                    return lookAroundTrigger;
                default:
                    return string.Empty;
            }
        }

        
    }
}
