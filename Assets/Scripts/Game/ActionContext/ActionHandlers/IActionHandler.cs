using System.Collections;

public interface IActionHandler
{
    bool CanHandle(ActionContext ctx);
    bool CanShowPreview(ActionContext ctx);
    void Execute(ActionContext ctx);
    void ShowPreview(ActionContext ctx); // Подсказка для UI
    void ShowAvaiableTargetCells();
}
