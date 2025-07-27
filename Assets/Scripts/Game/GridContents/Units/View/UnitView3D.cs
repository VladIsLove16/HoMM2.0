using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnitView3D : MonoBehaviour, IDisposable, IHoverable
{
    private CompositeDisposable _disposables = new();
    private Animator _animator;

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
        vm.OnAttacked.Subscribe(_ => EnqueueAction(PlayAnimation(UnitAnimationState.Attack))).AddTo(_disposables);
        vm.OnMoved.Subscribe(route => EnqueueAction(MoveAlongRoute(route))).AddTo(_disposables);
        vm.OnHit.Subscribe(_ => EnqueueAction(PlayAnimation(UnitAnimationState.Hit))).AddTo(_disposables);
        vm.OnDeath.Subscribe(_ => EnqueueAction(HandleDeath())).AddTo(_disposables);
        vm.OnTurnStarted.Subscribe(_ => EnqueueAction(PlayAnimation(UnitAnimationState.Idle))).AddTo(_disposables);
    }


    private void SetMaterial(Material material)
    {
        foreach (var mesh in meshes)
        {
            mesh.material = material;
        }
    }

    private void EnqueueAction(IEnumerator action)
    {
        actionQueue.Enqueue(action);

        if (!isExecuting)
            StartCoroutine(ProcessActions());
    }

    private IEnumerator ProcessActions()
    {
        isExecuting = true;

        while (actionQueue.Count > 0)
        {
            var action = actionQueue.Dequeue();
            yield return StartCoroutine(action);
        }

        isExecuting = false;
    }

    private IEnumerator MoveAlongRoute(List<Vector3> route)
    {
        Play(UnitAnimationState.Walk);

        foreach (var point in route)
        {
            yield return MoveToPosition(point);
        }

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
        Play(state);
        yield return null;
    }

    private IEnumerator HandleDeath()
    {
        Play(UnitAnimationState.Die);
        yield return new WaitForSeconds(1.5f); // подождать перед уничтожением
        Destroy(gameObject);
    }

    private void Play(UnitAnimationState state)
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
            _animator.SetTrigger(trigger);
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
        SetMaterial(_vm.HoveredTeamMaterial);
    }

    public void UnHover()
    {
        SetMaterial(_vm.TeamMaterial);
    }
}
