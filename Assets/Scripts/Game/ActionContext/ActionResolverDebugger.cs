using System.Collections.Generic;
using UnityEngine;
public partial class PlayerInputHandler
{
    public class ActionResolverDebugger : ActionResolver
    {
        public ActionResolverDebugger() : base()
        {
            Debug.Log("ActionResolverDebugger is ready");
        }

        //public override void SetActions(List<IActionHandler> handlers)
        //{
        //    Debug.Log($"Setting handlers: {string.Join(", ", handlers.Select(h => h.ToString()))}");
        //    base.SetActions(handlers);
        //}

        public override bool Resolve(ActionContext ctx, out IActionHandler handler)
        {
            Debug.Log(_handlers.Count);
            foreach (var handlerTemp in _handlers)
            {
                if(CanHandle(handlerTemp, ctx))
                {
                    handler = handlerTemp;
                    Debug.Log("resolving res: " + true);
                    return true;
                }
            }
            handler = null;
            Debug.Log("resolving res: " + false);
            return false;
        }
        protected new bool CanHandle(IActionHandler handler, ActionContext ctx)
        {
            Debug.Log("aaaaa");
            bool res = handler.CanHandle(ctx);
            Debug.Log((res ? "can" : "cant")+ " handle " + handler + " with ctx " + ctx);
            return handler.CanHandle(ctx);
        }
        public override void SetActions(UnitModel unit)
        {
            var list = new List<IActionHandler>();
            if (unit.CanMove.Value)
            {
                list.Add(_moveFactory.Create(unit));
            }
            if (unit.CanAct.Value)
            {
                list.Add(_moveThenAttackFactory.Create(unit));
            }
            if (unit.ModifiedStats.AttackRange > 1)
            {
                list.Add(_rangedFactory.Create(unit));
            }
            SetActions(list);
        }

        public override void SetActions(List<IActionHandler> handlers)
        {
            Debug.Log("setting actions " + handlers.Count);
            _handlers.Clear();
            _handlers.AddRange(handlers);
        }
    }

}

