using UnityEngine;
using Zenject;
public class UnitView : MonoBehaviour
{
    private UnitViewModel unitViewModel;
    private Animator animator;
    [SerializeField] private UnitHealthBar healthBar;
    [Inject]
    public void Construct(UnitViewModel unit)
    {
        this.unitViewModel = unit;
        animator = GetComponent<Animator>();
        unitViewModel.OnTakeDamage += OnTakeDamage;
        unitViewModel.OnOutDamage += OnAttack;
        unitViewModel.OnDeath += OnDeath;
        unitViewModel.OnTurnStart += OnTurnStart;
        healthBar.Init();
    }

    private void OnAttack(DamageContext context)
    {
        animator.SetTrigger("DealDamage");
    }

    private void OnTakeDamage(int dmg)
    {
        animator.SetTrigger("Hit");
        healthBar.SetRatio((float)unitViewModel.Health / unitViewModel.MaxHealth);
    }

    private void OnDeath()
    {
        animator.SetTrigger("Die");
        healthBar.Hide();
    }
     
    private void OnTurnStart()
        => animator.SetTrigger("Idle");
}
