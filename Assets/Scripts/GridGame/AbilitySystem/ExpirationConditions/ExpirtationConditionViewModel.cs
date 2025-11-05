public class ExpirtationConditionViewModel
{
    private ExpirationConditionBase _condition;
    public string RemaingingActions
    {
        get
        {
           return _condition.GetRemaining();
        }
    }
    public ExpirtationConditionViewModel(ExpirationConditionBase condition)
    {
        _condition = condition;
    }

}