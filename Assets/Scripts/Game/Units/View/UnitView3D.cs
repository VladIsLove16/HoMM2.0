using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEditor.Playables;
using UnityEngine;
using Zenject;

public class UnitView3D : MonoBehaviour, IDisposable
{
    private UnitViewModel _viewModel;
    private Animator _animator;
    private CompositeDisposable _disposables = new();
    [Inject] private IGridCellRenderer _cellRenderer;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private UnitViewUI _viewUI;
    [SerializeField] private List<SkinnedMeshRenderer> meshes;

    public void Init(UnitViewModel vm)
    {
        _viewModel = vm;
        _animator = GetComponent<Animator>();

        _viewModel.OnStartMovingTo.Subscribe(StartMovement).AddTo(_disposables);
        _viewModel.OnAttacked.Subscribe(_ => PlayAttackAnim()).AddTo(_disposables);
        _viewModel.OnHit.Subscribe(_ => PlayHitAnim()).AddTo(_disposables);
        _viewModel.OnDeath.Subscribe(_ => PlayDeathAnim()).AddTo(_disposables);
        _viewModel.OnTurnStarted.Subscribe(_ => PlayIdleAnim()).AddTo(_disposables);
    }

    public void SetMaterial(Material material)
    {
        foreach (var item in meshes)
        {
            item.material = material;
        }
    }
    private void StartMovement(Vector2Int targetPos)
    {
        StopAllCoroutines();
        Vector3 worldTarget = GridToWorld(targetPos); // Метод, переводящий grid -> world
        StartCoroutine(MoveToPosition(worldTarget));
    }

    private IEnumerator MoveToPosition(Vector3 targetWorldPos)
    {
        _animator.SetTrigger("Walk");

        while (Vector3.Distance(transform.position, targetWorldPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetWorldPos;
        _animator.SetTrigger("Idle");

        // Синхронизация позиции модели
        Vector2Int gridPos = WorldToGrid(targetWorldPos);
        _viewModel.Model.Position.Value = gridPos;
    }

    private Vector3 GridToWorld(Vector2Int pos) => _cellRenderer.ToWorld(pos.x, pos.y);
    private Vector2Int WorldToGrid(Vector3 world)
    {
        if (_cellRenderer.ToGrid(world, out Vector2Int gridPos))
            return gridPos;
        else 
            throw new ArgumentException(world + " not found on grid");
    }

    private void PlayAttackAnim() => _animator.SetTrigger("DealDamage");
    private void PlayHitAnim() => _animator.SetTrigger("Hit");
    private void PlayDeathAnim() => _animator.SetTrigger("Die");
    private void PlayIdleAnim() => _animator.SetTrigger("Idle");

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
