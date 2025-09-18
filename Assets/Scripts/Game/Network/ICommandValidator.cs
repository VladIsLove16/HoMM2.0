using System.Collections.Generic;
using UnityEngine;

namespace Game.Network
{
    /// <summary>
    /// Доменный интерфейс для валидации сетевых команд
    /// </summary>
    public interface ICommandValidator
    {
        CommandValidationResult ValidateMoveCommand(ulong unitId, List<Vector2Int> route);
        CommandValidationResult ValidateAttackCommand(ulong unitId, Vector2Int targetPosition);
        CommandValidationResult ValidateMoveThenAttackCommand(ulong unitId, List<Vector2Int> route, Vector2Int targetPosition);
    }

    /// <summary>
    /// Доменная реализация валидатора команд
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
        
        public CommandValidationResult ValidateMoveCommand(ulong unitId, List<Vector2Int> route)
        {
            if (route == null || route.Count == 0)
                return CommandValidationResult.Failure("Invalid route");
                
            // Здесь можно добавить дополнительную валидацию:
            // - Проверка доступности клеток
            // - Проверка очков движения
            // - Проверка состояния юнита
            
            return CommandValidationResult.Success();
        }
        
        public CommandValidationResult ValidateAttackCommand(ulong unitId, Vector2Int targetPosition)
        {
            if (!_gameModel.IsInBounds(targetPosition))
                return CommandValidationResult.Failure("Target position is out of bounds");
                
            // Здесь можно добавить валидацию атаки:
            // - Проверка дистанции до цели
            // - Проверка видимости цели
            // - Проверка состояния юнита
            
            return CommandValidationResult.Success();
        }
        
        public CommandValidationResult ValidateMoveThenAttackCommand(ulong unitId, List<Vector2Int> route, Vector2Int targetPosition)
        {
            var moveValidation = ValidateMoveCommand(unitId, route);
            if (!moveValidation.IsValid)
                return moveValidation;
                
            var attackValidation = ValidateAttackCommand(unitId, targetPosition);
            if (!attackValidation.IsValid)
                return attackValidation;
                
            return CommandValidationResult.Success();
        }
    }
}