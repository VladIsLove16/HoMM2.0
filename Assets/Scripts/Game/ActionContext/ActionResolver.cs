//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UniRx;
//using Unity.VisualScripting;
//using UnityEditor;
//using Zenject;
//public partial class PlayerInputHandler
//{
//    public class ActionResolver
//    {
//        [Inject] protected MoveActionHandlerFactory _moveFactory = new();
//        [Inject] protected RangedAttackHandlerFactory _rangedFactory = new();
//        [Inject] protected MoveThenAttackHandlerFactory _moveThenAttackFactory = new();

//        public List<IActionHandler> _handlers = new();
//        public ReactiveProperty<IActionHandler> CurrentAction = new();
//        public ActionResolver()
//        {
//        }

//        public virtual void SetActions(UnitModel unit)
//        {
//            var list = new List<IActionHandler>()
//            list.Add(moveAction);
//            CurrentAction.SetValueAndForceNotify( moveAction);
//            if (unit.CanAct.Value)
//            {
//                list.Add(_moveThenAttackFactory.Create(unit));
//            }
//            if (unit.ModifiedStats.AttackRange > 1)
//            {
//                list.Add(_rangedFactory.Create(unit));
//            }
//            SetActions(list);
//        }

//        public virtual void SetActions(List<IActionHandler> handlers)
//        {
//            _handlers.Clear();
//            handlers.AddRange(_handlers);
//        }

//        /// <summary>
//        /// GetActionHandler for ctx
//        /// </summary>
//        /// <param name="ctx"></param>
//        /// <param name="handler"></param>
//        /// <returns></returns>
//        public virtual bool Resolve(ActionContext ctx, out IActionHandler handler)
//        {
//            handler = _handlers.FirstOrDefault(h => CanHandle(h,ctx));
//            if (handler == null)
//                return false;

//            return true;
//        }

//        protected bool CanHandle(IActionHandler handler, ActionContext ctx)
//        {
//            return handler.CanHandle(ctx);
//        }
//        /// <summary>
//        /// Execute action forcly
//        /// </summary>
//        /// <param name="handler"></param>
//        /// <param name="ctx"></param>
//        private void Execute(IActionHandler handler, ActionContext ctx)
//        {
//            handler.Execute(ctx);
//        }

//        public void ShowPreview(ActionContext ctx)
//        {
//            var handler = _handlers.FirstOrDefault(h => h.CanShowPreview(ctx));
//            handler?.ShowPreview(ctx);
//        }
//    }
//}

