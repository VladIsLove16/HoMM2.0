using System;
using UnityEngine;
using Zenject;
public class UnitView3D : MonoBehaviour
{
    private UnitViewModel _vm;
    private Animator _animator;
    [SerializeField] private UnitViewUI _viewUI;
    public void Init(UnitViewModel vm)
    {
        _vm = vm;

        _animator = GetComponent<Animator>();

        _vm.OnTakeDamage += OnTakeDamage;
        _vm.OnOutDamage += OnAttack;
        _vm.OnDeath += OnDeath;
        _vm.OnTurnStart += OnTurnStart;

        _viewUI.Init(vm);
    }

    private void OnAttack(DamageContext context)
    {
        _animator.SetTrigger("DealDamage");
    }

    private void OnTakeDamage(int dmg)
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
