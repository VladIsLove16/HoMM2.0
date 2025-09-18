
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
