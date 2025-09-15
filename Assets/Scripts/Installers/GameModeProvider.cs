public     class GameModeProvider : IGameModeProvider
{
    public GameMode CurrentGameMode => SceneTransitionDataService.Instance.CurrentGameMode;
}