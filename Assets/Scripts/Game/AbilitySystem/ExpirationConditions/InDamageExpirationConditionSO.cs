using UnityEngine;
// InDamageExpirationConditionSO.cs
[CreateAssetMenu(menuName = "Magic/Expiration Conditions/In Damage")]
public class InDamageExpirationConditionSO : ExpirationConditionBase
{
    [Tooltip("Сколько входящих атак выдерживает эффект")] public int MaxHits;
    private int remaining;

    public override ExpirationConditionBase CreateRuntimeInstance() => Instantiate(this);
    public override void Initialize() => remaining = MaxHits;
    public override void OnTurn() { }
    public override void OnInDamage(DamageContext ctx) { if (remaining > 0) remaining--; }
    public override void OnOutDamage(DamageContext ctx) { }
    public override bool ShouldRemove => remaining <= 0;
    public override string GetRemaining() => remaining.ToString();

    public override void OnApply()
    {
    }
}
