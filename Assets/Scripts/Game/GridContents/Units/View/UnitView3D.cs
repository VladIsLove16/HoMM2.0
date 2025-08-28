using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnitView3D : MonoBehaviour, IDisposable, global::IHoverable
{
    private CompositeDisposable _disposables = new();
    private Animator _animator;

    [SerializeField] private global::UnitViewUI unitViewUI;
    [SerializeField] private float animationMoveSpeed = 3f;
    [SerializeField] private SkinnedMeshRenderer[] meshes;

    private Queue<IEnumerator> actionQueue = new();
    private bool isExecuting = false;
    public UnitModel Model { get; private set; }
    private Dictionary<bool, Material> _teamMaterials;
    private UnitViewModel _vm;
    /// <summary>
    /// UnitView does not change UnitModel at all
    /// </summary>
    /// <param name="vm"></param>
    public void Init(UnitViewModel vm)
    {
        Model = vm.Model;
        _vm = vm;
        SetMaterial(vm.TeamMaterial);
        _animator = GetComponent<Animator>();
        vm.OnAttacked.Subscribe(_ => 
        {
            LogDebugEvent("Unit Attacked");
            EnqueueAction(PlayAnimation(UnitAnimationState.Attack));
        }).AddTo(_disposables);
        //vm.OnMoved.Subscribe(route => EnqueueAction(MoveAlongRoute(route))).AddTo(_disposables);
        vm.OnHit.Subscribe(_ => 
        {
            LogDebugEvent("Unit Hit");
            EnqueueAction(PlayAnimation(UnitAnimationState.Hit));
        }).AddTo(_disposables);
        vm.OnDeath.Subscribe(_ => 
        {
            LogDebugEvent("Unit Death");
            EnqueueAction(HandleDeath());
        }).AddTo(_disposables);
        vm.OnTurnStarted.Subscribe(_ => 
        {
            LogDebugEvent("Unit Turn Started");
            EnqueueAction(PlayAnimation(UnitAnimationState.Idle));
        }).AddTo(_disposables);

        unitViewUI.Init(vm);
    }

    public void MoveByRoute(List<Vector3> route)
    {
        LogDebugEvent($"Unit Moving by Route: {route.Count} points");
        EnqueueAction(MoveAlongRoute(route));
    }

    private void SetMaterial(Material material)
    {
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

    private IEnumerator HandleDeath()
    {
        LogDebugEvent("Handling Death Animation");
        Play(UnitAnimationState.Die);
        yield return new WaitForSeconds(1.5f); // подождать перед уничтожением
        LogDebugEvent("Unit Deactivated");
        gameObject.SetActive(false);
        //Destroy(gameObject);
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

    public void Hover()
    {
        LogDebugEvent("Unit Hovered");
        SetMaterial(_vm.HoveredTeamMaterial);
    }

    public void UnHover()
    {
        LogDebugEvent("Unit Unhovered");
        SetMaterial(_vm.TeamMaterial);
    }
    
    private void LogDebugEvent(string eventMessage)
    {
        // Логируем в консоль
        Debug.Log($"[UnitView3D Debug] {eventMessage}");
        
        // Отправляем событие в дебаггеры
        // var componentDebugger = GetComponent<UnitView3DComponentDebugger>();
        // componentDebugger?.LogCustomEvent(eventMessage);
        
        // // Также можно добавить логирование в Editor дебаггер
        // #if UNITY_EDITOR
        // var editorDebugger = UnityEditor.Editor.CreateEditor(this) as Development.Editor.UnitView3DEditorDebugger;
        // editorDebugger?.AddEventToHistory(eventMessage);
        // #endif
    }
}
 