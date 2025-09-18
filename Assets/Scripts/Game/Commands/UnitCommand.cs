using System.Collections.Generic;
using UnityEngine;
    /// <summary>
    /// Доменная модель команды юнита
    /// </summary>
    public abstract class UnitCommand
    {
        public ulong UnitId { get; protected set; }
        public CommandType Type { get; protected set; }
        
        protected UnitCommand(ulong unitId, CommandType type)
        {
            UnitId = unitId;
            Type = type;
        }
    }

    /// <summary>
    /// Команда перемещения юнита
    /// </summary>
    public class MoveCommand : UnitCommand
    {
        public List<Vector2Int> Route { get; }
        
        public MoveCommand(ulong unitId, List<Vector2Int> route) : base(unitId, CommandType.Move)
        {
            Route = route ?? new List<Vector2Int>();
        }
    }

    /// <summary>
    /// Команда атаки юнита
    /// </summary>
    public class AttackCommand : UnitCommand
    {
        public Vector2Int TargetPosition { get; }
        
        public AttackCommand(ulong unitId, Vector2Int targetPosition) : base(unitId, CommandType.Attack)
        {
            TargetPosition = targetPosition;
        }
    }

    /// <summary>
    /// Команда перемещения с последующей атакой
    /// </summary>
    public class MoveThenAttackCommand : UnitCommand
    {
        public List<Vector2Int> Route { get; }
        public Vector2Int TargetPosition { get; }
        
        public MoveThenAttackCommand(ulong unitId, List<Vector2Int> route, Vector2Int targetPosition) 
            : base(unitId, CommandType.MoveThenAttack)
        {
            Route = route ?? new List<Vector2Int>();
            TargetPosition = targetPosition;
        }
    }

    public enum CommandType
    {
        Move,
        Attack,
        MoveThenAttack
    }
