public sealed class ActionPlan
{
    public ActionType ActionType { get; }
    public ActionContext Context { get; }

    public static readonly ActionPlan None = new(ActionType.None, default);

    public ActionPlan(ActionType actionType, ActionContext context)
    {
        ActionType = actionType;
        Context = context;
    }
}
