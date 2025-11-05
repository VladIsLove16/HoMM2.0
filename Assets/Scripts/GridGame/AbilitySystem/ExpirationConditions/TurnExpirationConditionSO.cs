using UnityEngine;
// TurnExpirationConditionSO.cs
[CreateAssetMenu(menuName = "Magic/Expiration Conditions/Turn")]
public class TurnExpirationConditionSO : ExpirationConditionBase
{
    [Tooltip("Сколько ходов жить")] public int Duration;
    private int remaining;

    public override ExpirationConditionBase CreateRuntimeInstance() => Instantiate(this);
    public override void Initialize() => remaining = Duration;
    public override void OnTurnStarted() { if (remaining > 0) remaining--; }
    public override void OnTurnEnded() {  }
    public override void OnInDamage(DamageContext ctx) { }
    public override void OnOutDamage(DamageContext ctx) { }
    public override bool ShouldRemove => remaining <= 0;
    public override string GetRemaining() => remaining.ToString();

    public override void OnApply() { }
}
