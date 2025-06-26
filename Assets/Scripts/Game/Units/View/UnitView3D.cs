using System;
using UnityEngine;
using Zenject;
public class UnitView3D : MonoBehaviour
{
    private UnitModel _model;
    private Animator _animator;
    [SerializeField] private UnitViewUI _viewUI;
    public void Init(UnitModel vm)
    {
        _model = vm;

        _animator = GetComponent<Animator>();

        _model.OnHit += OnHit;
        _model.OnAttack += OnAttack;
        _model.OnDeath += OnDeath;
        _model.OnTurnStart += OnTurnStart;
    }

    private void OnAttack()
    {
        _animator.SetTrigger("DealDamage");
    }

    private void OnHit(int dmg)
    {
        _animator.SetTrigger("Hit");
    }

    private void OnDeath()
    {
        _animator.SetTrigger("Die");
    }
     
    private void OnTurnStart()
        => _animator.SetTrigger("Idle");
}
