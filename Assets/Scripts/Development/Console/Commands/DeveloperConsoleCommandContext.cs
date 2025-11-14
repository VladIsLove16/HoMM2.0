using System;

public readonly struct DeveloperConsoleCommandContext
{
    public GameModel GameModel { get; }
    public IGridViewModel GridViewModel { get; }
    public IGameCommandExecutor CommandExecutor { get; }
    public ITurnService TurnService { get; }
    public ActionResolver ActionResolver { get; }
    public AdventureCommander AdventureCommander { get; }
    public GridCommander GridCommander { get; }

    public DeveloperConsoleCommandContext(
        GameModel gameModel,
        GameViewModel gameViewModel,
        IGameCommandExecutor commandExecutor,
        ITurnService turnService,
        ActionResolver actionResolver,
        AdventureCommander adventureCommander = null,
        GridCommander gridCommander = null)
    {
        GameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
        GridViewModel = gameViewModel ?? throw new ArgumentNullException(nameof(gameViewModel));
        CommandExecutor = commandExecutor ?? throw new ArgumentNullException(nameof(commandExecutor));
        TurnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        ActionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
        AdventureCommander = adventureCommander;
        GridCommander = gridCommander;
    }
}
