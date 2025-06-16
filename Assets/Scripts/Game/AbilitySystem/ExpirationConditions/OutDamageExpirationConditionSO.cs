using UnityEngine;
// OutDamageExpirationConditionSO.cs
[CreateAssetMenu(menuName = "Magic/Expiration Conditions/Out Damage")]
public class OutDamageExpirationConditionSO : ExpirationConditionBase
{
    [Tooltip("Сколько исходящих атак может совершить эффект")] public int MaxHits;
    private int remaining;

    public override ExpirationConditionBase CreateRuntimeInstance() => Instantiate(this);
    public override void Initialize() => remaining = MaxHits;
    public override void OnTurn() { }
    public override void OnInDamage(DamageContext ctx) { }
    public override void OnOutDamage(DamageContext ctx) { if (remaining > 0) remaining--; }
    public override bool ShouldRemove => remaining <= 0;
    public override string GetRemaining() => remaining.ToString();

    public override void OnApply()
    {
    }
}