using System.Collections;

public interface IActionHandler
{
    void Execute(ActionContext ctx);
    bool CanExecute(ActionContext ctx);
    PreviewResult GetPreview(ActionContext actionContext);
    ActionType ActionType { get; }
}
