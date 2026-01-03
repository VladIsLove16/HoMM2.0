using System;
using Adventure.Infrastructure.Dialog;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "NpcPatrol",
    story: "Patroling [npc] to [A] or [B] with movespeed [MoveSpeed], delay [SwitchTargetDelay]",
    category: "Action",
    id: "869fdfbeac2aa2df97e0ec444cf30c38")]
public sealed partial class NpcPatrolAction : Action
{

    private const float RotationThresholdDegrees = 1f;
    private const float DefaultStopDistance = 0.1f;
    private const float DefaultSlowdownDistance = 0.5f;
    private const float DefaultWaitTime = 2f;

    [SerializeReference] public BlackboardVariable<Transform> A;
    [SerializeReference] public BlackboardVariable<Transform> B;
    [SerializeReference] public BlackboardVariable<Transform>[] PatrolPoints;
    [SerializeReference] public BlackboardVariable<float> MoveSpeed;
    [SerializeReference] public BlackboardVariable<float> SwitchTargetDelay;
    [SerializeReference] public BlackboardVariable<float> StopDistance;
    [SerializeReference] public BlackboardVariable<float> SlowdownDistance;
    [SerializeReference] public BlackboardVariable<bool> AllowRotation;
    [SerializeReference] public BlackboardVariable<bool> AllowMovement;
    [SerializeReference] public BlackboardVariable<NpcAnimationController> Npc;
    [SerializeReference] public BlackboardVariable<NPCState> _phase;
    private readonly List<Transform> _patrolCandidates = new();
    private bool subs;
    private Transform _currentTarget;
    private float _waitTimer;
    private bool _slowdownTriggered;

    protected override Status OnStart()
    {
        Debug.Log("OnStart with state " + _phase.Value);
        if (!IsConfigured())
            return Status.Failure;
        if (!subs)
        {
            _phase.OnValueChanged += () => Debug.Log("new phase - " + _phase.Value.ToString());
            subs = true;
        }
            
        EnsureTargetAssigned();
        if (_phase.Value == NPCState.None)
        {
            EnterRotationPhase();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Debug.Log("OnUpdate)");
        if (!IsConfigured())
            return Status.Failure;

        switch (_phase.Value)
        {
            case NPCState.RotatingToTarget:
                UpdateRotationPhase();
                if (IsRotationCompleted(Npc.Value.transform, _currentTarget))
                    EnterWalkingPhase();
                break;
            case NPCState.Walking:
                UpdateWalkingPhase();
                break;
            case NPCState.LookingAround:
                UpdateWaitingPhase();
                break;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
        //_phase.Value = NPCState.None;
        //_waitTimer = 0f;
        //_slowdownTriggered = false;

        //Npc?.Value?.SetWalking(false);
    }

    private bool IsConfigured()
    {
        return Npc?.Value != null && A?.Value != null && B?.Value != null;
    }

    private void EnsureTargetAssigned()
    {
        if (_currentTarget != null)
            return;

        _currentTarget = SelectRandomPatrolPoint()
            ?? (UnityEngine.Random.value > 0.5f ? A.Value : B.Value);
    }

    private void EnterRotationPhase()
    {
        TriggerAnimation(NpcAnimationType.StartWalkingWithRotation);
        _phase.Value = NPCState.RotatingToTarget;
        UpdateRotationPhase();
    }

    private void UpdateRotationPhase()
    {
        if (!IsRotationAllowed())
            return;

        var npcTransform = Npc.Value.transform;
        var direction = _currentTarget.position - npcTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        var desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
        npcTransform.rotation = Quaternion.RotateTowards(
            npcTransform.rotation,
            desired,
            GetRotationSpeed() * Time.deltaTime);
    }
    private bool IsRotationCompleted(Transform npcTransform, Transform target)
    {
        var direction = target.position - npcTransform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return true;
        }
        var desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
        return CheckTargetRotation(npcTransform, desired);
    }
    private bool CheckTargetRotation(Transform npcTransform, Quaternion desired)
    {
        if (Quaternion.Angle(npcTransform.rotation, desired) <= RotationThresholdDegrees)
        {
            return true;
        }
        return false;
    }

    private void EnterWalkingPhase()
    {
        _phase.Value = NPCState.Walking;
       
        _slowdownTriggered = false;
    }

    private void UpdateWalkingPhase()
    {
        if (!IsMovementAllowed())
            return;

        SetWalkingState(true);
        var npcTransform = Npc.Value.transform;
        var direction = _currentTarget.position - npcTransform.position;
        var distance = direction.magnitude;

        TriggerSlowdown(distance);

        if (distance <= GetStopDistance())
        {
            BeginLookAroundPhase();
            return;
        }

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        AlignTowardsTarget();
        var planarDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        var speed = Mathf.Max(0.01f, MoveSpeed != null ? MoveSpeed.Value : 1f);
        npcTransform.position += planarDirection * speed * Time.deltaTime;
    }

    private void BeginLookAroundPhase()
    {
        SetWalkingState(false);
        TriggerAnimation(NpcAnimationType.LookAround);

        _waitTimer = GetSwitchDelay();
        _phase.Value = NPCState.LookingAround;
    }

    private void UpdateWaitingPhase()
    {
        _waitTimer -= Time.deltaTime;
        if (_waitTimer > 0f)
            return;

        SwitchTarget();
        EnterRotationPhase();
    }

    private void AlignTowardsTarget()
    {
        if (!IsRotationAllowed())
            return;

        var npcTransform = Npc.Value.transform;
        var direction = _currentTarget.position - npcTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        var desired = Quaternion.LookRotation(direction.normalized, Vector3.up);
        npcTransform.rotation = Quaternion.RotateTowards(
            npcTransform.rotation,
            desired,
            GetRotationSpeed() * Time.deltaTime);
    }

    private void TriggerSlowdown(float distanceToTarget)
    {
        if (_slowdownTriggered)
            return;

        if (distanceToTarget <= GetSlowdownDistance())
        {
            _slowdownTriggered = true;
        }
    }

    private void SwitchTarget()
    {
        var next = SelectRandomPatrolPoint(_currentTarget);
        if (next != null)
        {
            _currentTarget = next;
            return;
        }

        var pointA = A.Value;
        var pointB = B.Value;
        _currentTarget = ReferenceEquals(_currentTarget, pointB) ? pointA : pointB;
    }

    private float GetStopDistance()
    {
        return StopDistance != null && StopDistance.Value > 0f
            ? StopDistance.Value
            : DefaultStopDistance;
    }

    private float GetSlowdownDistance()
    {
        return SlowdownDistance != null && SlowdownDistance.Value > 0f
            ? SlowdownDistance.Value
            : DefaultSlowdownDistance;
    }

    private float GetSwitchDelay()
    {
        return SwitchTargetDelay != null && SwitchTargetDelay.Value > 0f
            ? SwitchTargetDelay.Value
            : DefaultWaitTime;
    }

    private float GetRotationSpeed()
    {
        return 360f;
    }

    private void SetWalkingState(bool isWalking)
    {
        Npc?.Value?.SetWalking(isWalking);
    }

    private void TriggerAnimation(NpcAnimationType type)
    {
        Npc?.Value?.PlayAnimation(type);
    }

    private bool IsRotationAllowed()
    {
        return AllowRotation == null || AllowRotation.Value;
    }

    private bool IsMovementAllowed()
    {
        return AllowMovement == null || AllowMovement.Value;
    }

    private Transform SelectRandomPatrolPoint(Transform exclude = null)
    {
        _patrolCandidates.Clear();
        if (PatrolPoints != null)
        {
            foreach (var entry in PatrolPoints)
            {
                var point = entry?.Value;
                if (point != null && point != exclude)
                {
                    _patrolCandidates.Add(point);
                }
            }
        }

        if (_patrolCandidates.Count == 0)
            return null;

        var index = UnityEngine.Random.Range(0, _patrolCandidates.Count);
        return _patrolCandidates[index];
    }
}
