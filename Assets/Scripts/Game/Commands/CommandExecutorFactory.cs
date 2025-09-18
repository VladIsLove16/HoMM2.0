using UnityEngine;
using Zenject;
    /// <summary>
    /// Фабрика для создания исполнителей команд
    /// </summary>
    public class CommandExecutorFactory
    {
        private readonly DiContainer _container;
        
        public CommandExecutorFactory(DiContainer container)
        {
            _container = container;
        }
        
        /// <summary>
        /// Создает исполнитель команд в зависимости от режима игры
        /// </summary>
        public ICommandExecutor CreateExecutor(GameMode gameMode)
        {
            return gameMode switch
            {
                GameMode.SinglePlayer => _container.Resolve<LocalCommandExecutor>(),
                GameMode.Multiplayer => _container.Resolve<NetworkCommandExecutor>(),
                _ => throw new System.ArgumentException($"Unknown game mode: {gameMode}")
            };
        }
    }
    
    public enum GameMode
    {
        SinglePlayer,
        Multiplayer,
    }
    
    /// <summary>
    /// Локальный исполнитель команд для одиночной игры
    /// </summary>
    public class LocalCommandExecutor : ICommandExecutor
    {
        private readonly GameModel _gameModel;
        
        public bool CanExecuteCommands => true;
        
        public LocalCommandExecutor(GameModel gameModel)
        {
            _gameModel = gameModel;
        }
        
        public void ExecuteCommand(UnitCommand command)
        {
            // В локальном режиме сразу выполняем команду через GameModel
            var executor = new CommandExecutor(_gameModel);
            executor.ExecuteCommand(command);
        }
    }
    
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
