using System;
using UnityEngine;

public class UnitAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private string walkTrigger = "Walking";
    [SerializeField] private string attackTrigger = "Attacking";
    [SerializeField] private string hitTrigger = "Hitted";
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private UnitAnimationState animationTrigger;
    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    [ContextMenu("Play Animation")] 
    public void PlayAnimation()
    {
        PlayAnimation(animationTrigger);
    }
    public void PlayAnimation(UnitAnimationState state)
    {
        if (animator == null)
            return;

        var trigger = state switch
        {
            UnitAnimationState.Idle => idleTrigger,
            UnitAnimationState.Walk => walkTrigger,
            UnitAnimationState.Attack => attackTrigger,
            UnitAnimationState.Hit => hitTrigger,
            UnitAnimationState.Die => dieTrigger,
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(trigger))
        {
            animator.SetTrigger(trigger);
            Debug.Log($"Playing animation: {trigger}");
            return;
        }
        Debug.LogError($"No trigger found for animation state: {trigger}");
    }

    public void SetPlaybackSpeed(float multiplier, bool instant)
    {
        if (animator == null)
            return;

        if (instant)
        {
            animator.speed = 100f;
            return;
        }

        animator.speed = Mathf.Max(0.01f, multiplier);
    }
}
