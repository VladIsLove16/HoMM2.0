public class AttackContext
{
    public IDamagable Target;
    public bool IsRanged;
    public AttackContext(IDamagable target, bool isRanged = false)
    {
        Target = target;
        IsRanged = isRanged;
    }
}

