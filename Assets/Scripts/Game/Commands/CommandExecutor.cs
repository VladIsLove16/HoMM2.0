using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    /// <summary>
    /// Исполнитель команд юнитов
    /// </summary>
    public class CommandExecutor : ICommandExecutor
    {
        private readonly GameModel _gameModel;
        
        public bool CanExecuteCommands => true; // В доменной модели всегда можно выполнять команды
        
        public CommandExecutor(GameModel gameModel)
        {
            _gameModel = gameModel;
        }
        
        public void ExecuteCommand(UnitCommand command)
        {
            switch (command)
            {
                case MoveCommand moveCommand:
                    ExecuteMoveCommand(moveCommand);
                    break;
                case AttackCommand attackCommand:
                    ExecuteAttackCommand(attackCommand);
                    break;
                case MoveThenAttackCommand moveThenAttackCommand:
                    ExecuteMoveThenAttackCommand(moveThenAttackCommand);
                    break;
                default:
                    throw new System.ArgumentException($"Unknown command type: {command.GetType()}");
            }
        }
        
        private void ExecuteMoveCommand(MoveCommand command)
        {
            var unit = GetUnitById(command.UnitId);
            if (unit != null)
            {
                _gameModel.ExecuteMoveAction(unit, command.Route);
            }
        }
        
        private void ExecuteAttackCommand(AttackCommand command)
        {
            var unit = GetUnitById(command.UnitId);
            if (unit != null)
            {
                _gameModel.ExecuteAttackAction(unit, command.TargetPosition);
            }
        }
        
        private void ExecuteMoveThenAttackCommand(MoveThenAttackCommand command)
        {
            var unit = GetUnitById(command.UnitId);
            if (unit != null)
            {
                _gameModel.ExecuteMoveThenAttackAction(unit, command.Route, command.TargetPosition);
            }
        }
        
        private UnitModel GetUnitById(ulong unitId)
        {
            var units = _gameModel.GetUnits();
            return units.FirstOrDefault(u => u.GetHashCode() == (int)unitId);
        }
    }
