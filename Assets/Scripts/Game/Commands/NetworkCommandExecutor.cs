using UnityEngine;
/// <summary>
/// Сетевой исполнитель команд для мультиплеера
/// </summary>
public class NetworkCommandExecutor : ICommandExecutor
    {
        private readonly GameNetworkCommandGateway _networkGateway;
        
        public bool CanExecuteCommands => true; // В сетевом режиме тоже можно выполнять команды
        
        public NetworkCommandExecutor(GameNetworkCommandGateway networkGateway)
        {
            _networkGateway = networkGateway;
        }
        
        public void ExecuteCommand(UnitCommand command)
        {
            // В сетевом режиме отправляем команду через сетевой шлюз
            switch (command)
            {
                case MoveCommand moveCommand:
                    _networkGateway.TrySendMoveRequest(Vector2Int.zero, moveCommand.Route);
                    break;
                case AttackCommand attackCommand:
                    _networkGateway.TrySendAttackRequest(Vector2Int.zero, attackCommand.TargetPosition);
                    break;
                case MoveThenAttackCommand moveThenAttackCommand:
                    // Для комбинированных команд можно разбить на отдельные
                    _networkGateway.TrySendMoveRequest(Vector2Int.zero, moveThenAttackCommand.Route);
                    _networkGateway.TrySendAttackRequest(Vector2Int.zero, moveThenAttackCommand.TargetPosition);
                    break;
            }
        }
    }
