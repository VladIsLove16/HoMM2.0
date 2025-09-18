using System.Collections.Generic;
using UnityEngine;

namespace Game.Network
{
    /// <summary>
    /// Доменная модель для сетевых команд юнита
    /// </summary>
    public class UnitNetworkCommand
    {
        public ulong UnitId { get; set; }
        public CommandType Type { get; set; }
        public List<Vector2Int> MoveRoute { get; set; }
        public Vector2Int TargetPosition { get; set; }
        public ulong TargetUnitId { get; set; }
    }

    public enum CommandType
    {
        Move,
        Attack,
        MoveThenAttack
    }

    /// <summary>
    /// Доменная модель для результата валидации команд
    /// </summary>
    public class CommandValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        
        public static CommandValidationResult Success() => new() { IsValid = true };
        public static CommandValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
    }
}