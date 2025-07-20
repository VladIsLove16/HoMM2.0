using System.Collections;

public interface IActionHandler
{
    bool CanHandle(ActionContext ctx);
    bool CanShowPreview(ActionContext ctx);
    IEnumerator Execute(ActionContext ctx);
    void ShowPreview(ActionContext ctx); // Подсказка для UI
}
