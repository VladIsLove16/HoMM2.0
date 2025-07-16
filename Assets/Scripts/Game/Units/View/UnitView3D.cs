using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class UnitView3D : MonoBehaviour
{
    private UnitModel _model;
    private Animator _animator;
    [SerializeField] private UnitViewUI _viewUI;
    [SerializeField] private List<SkinnedMeshRenderer> meshes;
    public void Init(UnitModel vm)
    {
        _model = vm;

        _animator = GetComponent<Animator>();

        _model.Hitted += OnHit;
        _model.Attacked += OnAttack;
        _model.Died += OnDeath;
        _model.TurnStarted += OnTurnStart;
    }
    public void SetMaterial(Material material)
    {
        foreach (var item in meshes)
        {
            item.material = material;
        }
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
