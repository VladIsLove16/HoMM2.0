using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnitAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string idleTrigger = "Idle";
    [SerializeField] private string walkTrigger = "Walking";
    [SerializeField] private string attackTrigger = "Attacking";
    [SerializeField] private string hitTrigger = "Hitted";
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private bool logEvents;
    [SerializeField] private UnitAnimationState previewState = UnitAnimationState.Idle;

    public event Action<UnitAnimationEvent> AnimationEventRaised;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("[UnitAnimatorController] Animator is missing", this);
        }
    }

    [ContextMenu("Play Animation")]
    private void PlayAnimationInEditor()
    {
        PlayAnimation(previewState);
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
        }
        else
        {
            Debug.LogWarning($"[UnitAnimatorController] Trigger not configured for {state}", this);
        }
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

    /// <summary>
    /// Called via Animation Event (string parameter) to raise strongly typed events.
    /// </summary>
    public void DispatchAnimationEvent(string eventName)
    {
        if (Enum.TryParse<UnitAnimationEvent>(eventName, true, out var evt))
        {
            RaiseAnimationEvent(evt);
        }
        else
        {
            Debug.LogWarning($"[UnitAnimatorController] Unknown animation event '{eventName}' on {name}", this);
        }
    }

    /// <summary>
    /// Optional int overload for Animation Event (int) usage.
    /// </summary>
    public void DispatchAnimationEvent(int eventId)
    {
        if (Enum.IsDefined(typeof(UnitAnimationEvent), eventId))
        {
            RaiseAnimationEvent((UnitAnimationEvent)eventId);
        }
        else
        {
            Debug.LogWarning($"[UnitAnimatorController] Unknown animation event id '{eventId}' on {name}", this);
        }
    }

    private void RaiseAnimationEvent(UnitAnimationEvent evt)
    {
        if (logEvents)
        {
            Debug.Log($"[UnitAnimatorController] Event {evt} raised on {name}", this);
        }

        AnimationEventRaised?.Invoke(evt);
    }
}
