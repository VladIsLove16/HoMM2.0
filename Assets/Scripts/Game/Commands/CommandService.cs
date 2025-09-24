using System;
using UnityEngine;
    /// <summary>
    /// Сервис для выполнения команд юнитов
    /// </summary>
    public class CommandService
    {
        private readonly ICommandExecutor _executor;
        
        public event Action<UnitCommand> CommandExecuted;
        public event Action<UnitCommand, string> CommandFailed;
        
        public CommandService( ICommandExecutor executor)
        {
            _executor = executor;
        }

    public void Execute(IActionHandler actionHandler, ActionContext actionContext)
    {
        
    }
}
