using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
public class UnitView3D : MonoBehaviour, IDisposable, IHoverable, IGameViewObject
{
    private readonly CompositeDisposable _disposables = new();
    [SerializeField] private UnitAnimatorController _animationController;
    [SerializeField] private UnitViewUI unitViewUI;
    [SerializeField] private SkinnedMeshRenderer[] meshes;
    [SerializeField] private Vector3 blueDefaultForward = Vector3.forward;
    [SerializeField] private Vector3 redDefaultForward = Vector3.back;
    [Inject] private IMaterialProvider _teamMaterials;
    [Inject] private IBattleAnimationGate _animationGate;
    [Inject] private IAnimationSpeedSettings _animationSpeedSettings;

    private Queue<IEnumerator> actionQueue = new();
    private bool isExecuting = false;
    private UnitViewModel _vm;
    private bool _disposed;
    private IDisposable _gateHandle;
    private Vector3 _defaultForward = Vector3.forward;
    public bool IsHoverable => true;
    public bool IsSelectable => true;

    private void Awake()
    {
        if (meshes == null || meshes.Length == 0)
        {
            Debug.LogWarning("meshes have not been setted");
        }
        if (unitViewUI == null)
        {
            Debug.LogWarning("unitViewUI null ref");
            return;
        }
        if (_animationController == null)
        {
            _animationController = GetComponent<UnitAnimatorController>();
            if (_animationController == null)
            {
                Debug.LogError("[UnitView3D] UnitAnimatorController component not found");
                return;
            }
        }

    }
    /// <summary>
    /// UnitView does not change UnitModel at all
    /// </summary>
    /// <param name="vm"></param>
    public void Init(UnitViewModel vm)
    {
        if (vm == null)
            throw new ArgumentNullException(nameof(vm));

        _disposables.Clear();
        _vm = vm;
        SetupMaterial(vm);
        UpdateDefaultFacing(vm.Team);
        FaceDefaultDirection();
        unitViewUI.Init(vm);
        SubscribeToViewModel(vm);
        ApplyAnimationSpeed();
    }

    private void SetupMaterial(UnitViewModel vm)
    {
        var mat = _teamMaterials.GetTeamMaterial(vm.Team);
        if (mat != null)
        {
            SetMaterial(mat);
        }
    }

    public void SnapToCell(Vector3 worldPosition)
    {
        StopAllCoroutines();
        actionQueue.Clear();
        isExecuting = false;

        transform.position = worldPosition;
        Play(UnitAnimationState.Idle);
        FaceDefaultDirection();

        LogDebugEvent($"Snapped instantly to {worldPosition}");
    }
    public void Move(List<Vector3> route)
    {
        ExecuteMove(route);
    }
    
    /// <summary>
    /// ���������� ����� ��� ���������� ����������� (���������� �� ������� ������)
    /// </summary>
    private void ExecuteMove(List<Vector3> route)
    {
        LogDebugEvent($"Unit Moving by Route Manually: {route.Count} points");
        EnqueueAction(MoveAlongRoute(route));
    }
    
    private void HandleAttackCommand(ulong targetUnitId)
    {
        // ����� ����� ������ �����
        LogDebugEvent($"Unit attacking target: {targetUnitId}");
        // TODO: Implement attack logic
    }

    private void SetMaterial(Material material)
    {
        if (meshes == null || meshes.Length == 0)
        {
            Debug.LogWarning("[UnitView3D] 'meshes' not assigned and no SkinnedMeshRenderer found. Skipping material set.");
            return;
        }
        string oldMaterialName = meshes[0].material?.name;
        string newMaterialName = material?.name;
        
        foreach (var mesh in meshes)
        {
            mesh.material = material;
        }
        
        LogDebugEvent($"Material Changed: {oldMaterialName} -> {newMaterialName}");
    }

    private void EnqueueAction(IEnumerator action)
    {
        actionQueue.Enqueue(action);
        LogDebugEvent($"Action Enqueued. Queue Count: {actionQueue.Count}");

        if (!isExecuting)
            StartCoroutine(ProcessActions());
    }
    private IEnumerator ProcessActions()
    {
        isExecuting = true;
        LogDebugEvent("Action Processing Started");
        _gateHandle = _animationGate?.Acquire();
        try
        {
            while (actionQueue.Count > 0)
            {
                var action = actionQueue.Dequeue();
                LogDebugEvent($"Executing Action {action.ToString()}. Remaining: {actionQueue.Count}");
                yield return StartCoroutine(action);    
            }
        }
        finally
        {
            ReleaseGateHandle();
            isExecuting = false;
            LogDebugEvent("Action Processing Finished");
        }
    }



    private IEnumerator MoveAlongRoute(List<Vector3> route)
    {
        LogDebugEvent($"Starting Movement: {route.Count} points");
        if (_animationSpeedSettings != null && _animationSpeedSettings.IsInstant)
        {
            foreach (var point in route)
            {
                LogDebugEvent($"Teleporting to: {point}");
                transform.position = point;
            }
            Play(UnitAnimationState.Walk);
            LogDebugEvent("Movement Finished (instant)");
            FaceDefaultDirection();
            yield return null;
            yield break;
        }

        _animationController?.SetWalking(true);

        foreach (var point in route)
        {
            LogDebugEvent($"Moving to: {point}");
            FaceTowards(point);
            if (!IsInstantMode())
            {
                yield return null;
            }
            yield return MoveToPosition(point);
        }

        LogDebugEvent("Movement Finished");
        _animationController?.SetWalking(false);
        Play(UnitAnimationState.Idle);
        FaceDefaultDirection();
    }


    private IEnumerator MoveToPosition(Vector3 worldPos)
    {
        if (_animationSpeedSettings != null && _animationSpeedSettings.IsInstant)
        {
            transform.position = worldPos;
            yield return null;
            yield break;
        }

        var speed = _animationSpeedSettings?.PlaybackMultiplier ?? 1f;

        while (Vector3.Distance(transform.position, worldPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, worldPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = worldPos;
    }


    private IEnumerator PlayAnimationRoutine(UnitAnimationState state, UnitAnimationEvent? completionEvent = null)
    {
        LogDebugEvent($"Playing Animation: {state}");
        Play(state);

        if (completionEvent.HasValue && !IsInstantMode())
        {
            yield return WaitForAnimationEvent(completionEvent.Value);
        }
        else
        {
            yield return null;
        }
    }

    private IEnumerator HandleDeathAction()
    {
        LogDebugEvent("Handling Death Animation");
        yield return PlayAnimationRoutine(UnitAnimationState.Die, UnitAnimationEvent.DieFinished);
        LogDebugEvent("Unit Deactivated");
        gameObject.SetActive(false);
    }


    public void Play(UnitAnimationState state)
    {
        if (_animationController == null) return;
        ApplyAnimationSpeed();
        _animationController.PlayAnimation(state);
        LogDebugEvent($"Animation Trigger Set: {state}");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _disposables.Dispose();
        ReleaseGateHandle();
        StopAllCoroutines();
        actionQueue.Clear();
        isExecuting = false;
    }
    
    private void OnDisable()
    {
        ReleaseGateHandle();
        StopAllCoroutines();
        actionQueue.Clear();
        isExecuting = false;
    }
    
    public void HandleAttack(Vector3? targetWorldPosition = null)
    {
        LogDebugEvent("Unit Attacked");
        EnqueueAction(PlayAttackRoutine(targetWorldPosition));
    }

    public void HandleHit(Vector3? attackerWorldPosition = null)
    {
        LogDebugEvent("Unit Hit");
        EnqueueAction(PlayHitRoutine(attackerWorldPosition));
    }

    public void PlayCoordinatedAttack(UnitView3D target)
    {
        var pos = target != null ? target.transform.position : (Vector3?)null;
        EnqueueAction(PlayAttackRoutine(pos));
    }

    public void PlayCoordinatedHit(UnitView3D attacker)
    {
        var pos = attacker != null ? attacker.transform.position : (Vector3?)null;
        EnqueueAction(PlayHitRoutine(pos));
    }

    public void HandleDeath()
    {
        LogDebugEvent("Unit Death");
        if (_vm.Amount <= 0)
        {
            EnqueueAction(HandleDeathAction());
        }
        else
        {
            LogDebugEvent("Unit not fully dead, just playing hit animation");
            EnqueueAction(PlayAnimationRoutine(UnitAnimationState.Hit, UnitAnimationEvent.HitFinished));
        }
    }

    public void HandleTurnStarted()
    {
        LogDebugEvent("Unit Turn Started");
        EnqueueAction(PlayAnimationRoutine(UnitAnimationState.Idle));
    }
    
    public void HandleHealthChanged()
    {
        LogDebugEvent("Unit Health Changed");
    }

    public void Hover()
    {
        LogDebugEvent("Unit Hovered");
        SetMaterial(_teamMaterials.GetHoveredTeamMaterial(_vm.Team));
    }

    public void Unhover()
    {
        LogDebugEvent("Unit Unhovered");
        SetMaterial(_teamMaterials.GetTeamMaterial(_vm.Team));
    }
    
    private void OnAnimationSpeedChanged(AnimationSpeedMode mode)
    {
        ApplyAnimationSpeed();
    }

    private void ApplyAnimationSpeed()
    {
        if (_animationController == null)
            return;
        var multiplier = _animationSpeedSettings?.PlaybackMultiplier ?? 1f;
        var isInstant = _animationSpeedSettings?.IsInstant ?? false;
        _animationController.SetPlaybackSpeed(multiplier, isInstant);
    }

    private float ScaleDuration(float baseDuration)
    {
        if (_animationSpeedSettings == null)
            return baseDuration;
        if (_animationSpeedSettings.IsInstant)
            return 0f;
        return baseDuration / _animationSpeedSettings.PlaybackMultiplier;
    }

    private void LogDebugEvent(string eventMessage)
    {
        UnityLogger.Log($"[UnitView3D Debug] {eventMessage}",LogCategory.Unit);
    }
    
    private void ReleaseGateHandle()
    {
        _gateHandle?.Dispose();
        _gateHandle = null;
    }

    private void SubscribeToViewModel(UnitViewModel viewModel)
    {
        viewModel.OnTeamChangedEnum
            .Subscribe(OnTeamChanged)
            .AddTo(_disposables);

        viewModel.OnMoveByWorldRoute
            .Subscribe(OnWorldRouteReceived)
            .AddTo(_disposables);

        viewModel.OnDeath
            .Subscribe(_ => HandleDeath())
            .AddTo(_disposables);

        viewModel.OnTurnStarted
            .Subscribe(_ => HandleTurnStarted())
            .AddTo(_disposables);

        viewModel.OnHealthChanged
            .Subscribe(_ => HandleHealthChanged())
            .AddTo(_disposables);

        if (_animationSpeedSettings != null)
        {
            _animationSpeedSettings.Mode.Subscribe(OnAnimationSpeedChanged).AddTo(_disposables);
        }
    }

    private void OnTeamChanged(Team team)
    {
        SetMaterial(_teamMaterials.GetTeamMaterial(team));
        UpdateDefaultFacing(team);
        FaceDefaultDirection();
    }

    private void OnWorldRouteReceived(IReadOnlyList<Vector3> route)
    {
        if (route == null || route.Count == 0)
            return;

        Move(new List<Vector3>(route));
    }

    private IEnumerator PlayAttackRoutine(Vector3? targetWorldPosition)
    {
        if (targetWorldPosition.HasValue)
        {
            FaceTowards(targetWorldPosition.Value);
        }
        yield return PlayAnimationRoutine(UnitAnimationState.Attack, UnitAnimationEvent.AttackFinished);
    }

    private IEnumerator PlayHitRoutine(Vector3? attackerWorldPosition)
    {
        if (attackerWorldPosition.HasValue)
        {
            FaceTowards(attackerWorldPosition.Value);
        }
        yield return PlayAnimationRoutine(UnitAnimationState.Hit, UnitAnimationEvent.HitFinished);
    }

    private void FaceTowards(Vector3 worldTarget)
    {
        var direction = worldTarget - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        var lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = lookRotation;
    }

    private void FaceDefaultDirection()
    {
        if (_defaultForward.sqrMagnitude < 0.0001f)
            return;

        var normalized = _defaultForward.normalized;
        if (normalized.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
    }

    private void UpdateDefaultFacing(Team team)
    {
        var desired = team switch
        {
            Team.Red => redDefaultForward,
            Team.Blue => blueDefaultForward,
            _ => _defaultForward
        };

        if (desired.sqrMagnitude < 0.0001f)
        {
            desired = Vector3.forward;
        }

        _defaultForward = desired.normalized;
    }

    private IEnumerator WaitForAnimationEvent(UnitAnimationEvent eventId)
    {
        if (_animationController == null)
            yield break;

        var completed = false;
        void Handler(UnitAnimationEvent evt)
        {
            if (evt == eventId)
            {
                completed = true;
            }
        }

        _animationController.AnimationEventRaised += Handler;
        var safety = 5f;
        var iterationBudget = 2000;
        try
        {
            while (!completed && safety > 0f && iterationBudget-- > 0)
            {
                var delta = Application.isPlaying ? Time.deltaTime : 0.02f;
                safety -= delta;
                yield return null;
            }
        }
        finally
        {
            _animationController.AnimationEventRaised -= Handler;
        }
    }

    private bool IsInstantMode() => _animationSpeedSettings != null && _animationSpeedSettings.IsInstant;

    private void OnDestroy()
    {
        Dispose();
    }
}
 

