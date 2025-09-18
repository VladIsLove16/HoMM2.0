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
        
        /// <summary>
        /// Выполняет команду 
        /// </summary>
        public void ExecuteCommand(UnitCommand command)
        {
            if (!_executor.CanExecuteCommands)
            {
                CommandFailed?.Invoke(command, "Cannot execute commands");
                return;
            }
            
            // Выполнение команды
            try
            {
                _executor.ExecuteCommand(command);
                CommandExecuted?.Invoke(command);
            }
            catch (Exception ex)
            {
                CommandFailed?.Invoke(command, ex.Message);
            }
        }
        
        /// <summary>
        /// Выполняет команду перемещения
        /// </summary>
        public void ExecuteMoveCommand(ulong unitId, System.Collections.Generic.List<Vector2Int> route)
        {
            var command = new MoveCommand(unitId, route);
            ExecuteCommand(command);
        }
        
        /// <summary>
        /// Выполняет команду атаки
        /// </summary>
        public void ExecuteAttackCommand(ulong unitId, Vector2Int targetPosition)
        {
            var command = new AttackCommand(unitId, targetPosition);
            ExecuteCommand(command);
        }
        
        /// <summary>
        /// Выполняет команду перемещения с атакой
        /// </summary>
        public void ExecuteMoveThenAttackCommand(ulong unitId, System.Collections.Generic.List<Vector2Int> route, Vector2Int targetPosition)
        {
            var command = new MoveThenAttackCommand(unitId, route, targetPosition);
            ExecuteCommand(command);
        }
    }
