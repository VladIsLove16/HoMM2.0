using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;
[RequireComponent(typeof(Animator))]
public class UnitView3D : MonoBehaviour, IDisposable, IHoverable, IGameViewObject
{
    private CompositeDisposable _disposables = new();
    private Animator _animator;

    [SerializeField] private UnitViewUI unitViewUI;
    [SerializeField] private float animationMoveSpeed = 3f;
    [SerializeField] private SkinnedMeshRenderer[] meshes;
    [Inject] private IMaterialProvider _teamMaterials;

    private Queue<IEnumerator> actionQueue = new();
    private bool isExecuting = false;
    public bool IsHoverable => true;
    public bool IsSelectable => true;

    private UnitViewModel _vm;
    private void Awake()
    {
        // Исполнитель команд назначается позже (через фабрику/вариант префаба) до Init()
        // Безопасная авто-инициализация мешей, если не назначены в инспекторе
        if (meshes == null || meshes.Length == 0)
        {
            meshes = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }
    }
    /// <summary>
    /// UnitView does not change UnitModel at all
    /// </summary>
    /// <param name="vm"></param>
    public void Init(UnitViewModel vm)
    {
        _vm = vm;
        // Резолвим исполнитель команд после того, как фабрика/вариант префаба добавил компонент
        SetMaterial(_teamMaterials.GetTeamMaterial(vm.Model.Team.Value));
        _animator = GetComponent<Animator>();
        // Все события теперь обрабатываются через GameView3D
        // Подписки на события ViewModel удалены для единообразного подхода

        if(unitViewUI == null)
        {
            Debug.LogWarning("unitViewUI null ref");
            return;
        }
        unitViewUI.Init(vm);
        
        // Реакция на смену команды/материалов
        _vm.OnTeamChanged.Subscribe(_ => SetMaterial(_teamMaterials.GetTeamMaterial(_vm.Model.Team.Value))).AddTo(_disposables);
    }
    public void SnapToCell(Vector3 worldPosition)
    {
        StopAllCoroutines();
        actionQueue.Clear();
        isExecuting = false;

        transform.position = worldPosition;
        Play(UnitAnimationState.Idle);

        LogDebugEvent($"Snapped instantly to {worldPosition}");
    }

    /// <summary>
    /// Публичный метод для проигрывания перемещения (вызов из GameView3D или сетевого слоя)
    /// </summary>
    public void RequestMove(List<Vector3> route)
    {
        // Больше не отправляем команду из View: только анимация
        ExecuteMove(route);
    }
    
    /// <summary>
    /// Внутренний метод для выполнения перемещения (вызывается из сетевых команд)
    /// </summary>
    private void ExecuteMove(List<Vector3> route)
    {
        LogDebugEvent($"Unit Moving by Route Manually: {route.Count} points");
        EnqueueAction(MoveAlongRoute(route));
    }
    
    private void HandleAttackCommand(ulong targetUnitId)
    {
        // Здесь будет логика атаки
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
        string oldMaterialName = meshes.Length > 0 ? meshes[0].material?.name ?? "null" : "null";
        string newMaterialName = material?.name ?? "null";
        
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

        while (actionQueue.Count > 0)
        {
            var action = actionQueue.Dequeue();
            LogDebugEvent($"Executing Action. Remaining: {actionQueue.Count}");
            yield return StartCoroutine(action);
        }

        isExecuting = false;
        LogDebugEvent("Action Processing Finished");
    }

    private IEnumerator MoveAlongRoute(List<Vector3> route)
    {
        LogDebugEvent($"Starting Movement: {route.Count} points");
        Play(UnitAnimationState.Walk);

        foreach (var point in route)
        {
            LogDebugEvent($"Moving to: {point}");
            yield return MoveToPosition(point);
        }

        LogDebugEvent("Movement Finished");
        Play(UnitAnimationState.Idle);
    }

    private IEnumerator MoveToPosition(Vector3 worldPos)
    {
        while (Vector3.Distance(transform.position, worldPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, worldPos, animationMoveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = worldPos;
    }

    private IEnumerator PlayAnimation(UnitAnimationState state)
    {
        LogDebugEvent($"Playing Animation: {state}");
        Play(state);
        yield return null;
    }

    private IEnumerator HandleDeathAction()
    {
        LogDebugEvent("Handling Death Animation");
        Play(UnitAnimationState.Die);
        yield return new WaitForSeconds(1.5f);
        LogDebugEvent("Unit Deactivated");
        gameObject.SetActive(false);
    }

    public void Play(UnitAnimationState state)
    {
        if (_animator == null) return;

        string trigger = state switch
        {
            UnitAnimationState.Idle => "Idle",
            UnitAnimationState.Walk => "Walk",
            UnitAnimationState.Attack => "DealDamage",
            UnitAnimationState.Hit => "Hit",
            UnitAnimationState.Die => "Die",
            _ => ""
        };

        if (!string.IsNullOrEmpty(trigger))
        {
            _animator.SetTrigger(trigger);
            LogDebugEvent($"Animation Trigger Set: {trigger}");
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
        StopAllCoroutines();
        actionQueue.Clear();
        isExecuting = false;
    }
    
    public void HandleAttack(DamageContext context)
    {
        LogDebugEvent("Unit Attacked");
        EnqueueAction(PlayAnimation(UnitAnimationState.Attack));
    }
    
    public void HandleHit(DamageContext context)
    {
        LogDebugEvent("Unit Hit");
        EnqueueAction(PlayAnimation(UnitAnimationState.Hit));
    }
    
    public void HandleDeath()
    {
        LogDebugEvent("Unit Death");
        if (_vm.Model.Amount.Value <= 0)
        {
            EnqueueAction(HandleDeathAction());
        }
        else
        {
            LogDebugEvent("Unit not fully dead, just playing hit animation");
            EnqueueAction(PlayAnimation(UnitAnimationState.Hit));
        }
    }
    
    public void HandleTurnStarted()
    {
        LogDebugEvent("Unit Turn Started");
        EnqueueAction(PlayAnimation(UnitAnimationState.Idle));
    }
    
    public void HandleHealthChanged()
    {
        LogDebugEvent("Unit Health Changed");
    }

    public void Hover()
    {
        LogDebugEvent("Unit Hovered");
        SetMaterial(_teamMaterials.GetHoveredTeamMaterial(_vm.Model.Team.Value));
    }

    public void Unhover()
    {
        LogDebugEvent("Unit Unhovered");
        SetMaterial(_teamMaterials.GetHoveredTeamMaterial(_vm.Model.Team.Value));
    }
    
    private void LogDebugEvent(string eventMessage)
    {
        Debug.Log($"[UnitView3D Debug] {eventMessage}");
    }

    

}
 