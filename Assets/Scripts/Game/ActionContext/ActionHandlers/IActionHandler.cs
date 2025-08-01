using System.Collections;

public interface IActionHandler
{
    void Execute(ActionContext ctx);
    void ShowPreview(ActionContext ctx);
    void ShowAvaiableTargetCells();
    void HidePreview();
}
