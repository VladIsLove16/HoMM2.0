using System.Collections.Generic;
using UnityEngine;
    /// <summary>
    /// Валидатор команд юнитов
    /// </summary>
    public class CommandValidator : ICommandValidator
    {
        private readonly GameModel _gameModel;
        private readonly MovementSystem _movementSystem;
        
        public CommandValidator(GameModel gameModel, MovementSystem movementSystem)
        {
            _gameModel = gameModel;
            _movementSystem = movementSystem;
        }
        
        public CommandValidationResult ValidateCommand(UnitCommand command)
        {
            return command switch
            {
                MoveCommand moveCommand => ValidateMoveCommand(moveCommand),
                AttackCommand attackCommand => ValidateAttackCommand(attackCommand),
                MoveThenAttackCommand moveThenAttackCommand => ValidateMoveThenAttackCommand(moveThenAttackCommand),
                _ => CommandValidationResult.Failure("Unknown command type")
            };
        }
        
        private CommandValidationResult ValidateMoveCommand(MoveCommand command)
        {
            if (command.Route == null || command.Route.Count == 0)
                return CommandValidationResult.Failure("Invalid route");
                
            // Проверяем, что все точки маршрута в пределах карты
            foreach (var point in command.Route)
            {
                if (!_gameModel.IsInBounds(point))
                    return CommandValidationResult.Failure($"Route point {point} is out of bounds");
            }
            
            // Здесь можно добавить дополнительную валидацию:
            // - Проверка доступности клеток
            // - Проверка очков движения
            // - Проверка состояния юнита
            
            return CommandValidationResult.Success();
        }
        
        private CommandValidationResult ValidateAttackCommand(AttackCommand command)
        {
            if (!_gameModel.IsInBounds(command.TargetPosition))
                return CommandValidationResult.Failure("Target position is out of bounds");
                
            // Здесь можно добавить валидацию атаки:
            // - Проверка дистанции до цели
            // - Проверка видимости цели
            // - Проверка состояния юнита
            
            return CommandValidationResult.Success();
        }
        
        private CommandValidationResult ValidateMoveThenAttackCommand(MoveThenAttackCommand command)
        {
            var moveValidation = ValidateMoveCommand(new MoveCommand(command.UnitId, command.Route));
            if (!moveValidation.IsValid)
                return moveValidation;
                
            var attackValidation = ValidateAttackCommand(new AttackCommand(command.UnitId, command.TargetPosition));
            if (!attackValidation.IsValid)
                return attackValidation;
                
            return CommandValidationResult.Success();
        }
    }
