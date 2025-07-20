using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Zenject;

public class ActionResolver
{
    private readonly List<IActionHandler> _handlers;
    private readonly TurnSystem _turnSystem;
    [Inject]
    public ActionResolver(TurnSystem turnSystem)
    {
        _turnSystem = turnSystem;
    }
    
    public void SetHandlers(List<IActionHandler> handlers)
    {
        _handlers.Clear();
        handlers.AddRange(_handlers);
    }

    public bool Resolve(ActionContext ctx, out IActionHandler handler)
    {
        handler = _handlers.FirstOrDefault(h => h.CanHandle(ctx));
        if (handler == null) return false;

        CoroutineRunner.instance.StartCoroutine(Execute(handler, ctx));
        return true;
    }

    private IEnumerator Execute(IActionHandler handler, ActionContext ctx)
    {
        yield return handler.Execute(ctx);
        _turnSystem.EndTurn();
    }

    public void ShowPreview(ActionContext ctx)
    {
        var handler = _handlers.FirstOrDefault(h => h.CanShowPreview(ctx));
        handler?.ShowPreview(ctx);
    }
}
