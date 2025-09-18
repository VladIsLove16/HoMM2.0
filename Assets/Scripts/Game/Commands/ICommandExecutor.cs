    /// <summary>
    /// Интерфейс для выполнения команд юнита
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// Выполняет команду
        /// </summary>
        void ExecuteCommand(UnitCommand command);
        
        /// <summary>
        /// Проверяет, может ли выполнять команды
        /// </summary>
        bool CanExecuteCommands { get; }
    }

    /// <summary>
    /// Интерфейс для валидации команд
    /// </summary>
    public interface ICommandValidator
    {
        /// <summary>
        /// Валидирует команду
        /// </summary>
        CommandValidationResult ValidateCommand(UnitCommand command);
    }

    /// <summary>
    /// Результат валидации команды
    /// </summary>
    public class CommandValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        
        public static CommandValidationResult Success() => new() { IsValid = true };
        public static CommandValidationResult Failure(string error) => new() { IsValid = false, ErrorMessage = error };
    }
