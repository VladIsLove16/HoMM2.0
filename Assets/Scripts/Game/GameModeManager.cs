using UnityEngine;
using Game.Network;
using Zenject;

/// <summary>
/// Менеджер режимов игры, отвечающий за переключение между одиночной и сетевой игрой
/// </summary>
public class GameModeManager : MonoBehaviour
{
    [Header("Game Mode Settings")]
    [SerializeField] private GameMode currentGameMode = GameMode.Singleplayer;
    [SerializeField] private bool autoSetupUnits = true;
    
    public GameMode CurrentGameMode => currentGameMode;
    
    /// <summary>
    /// Устанавливает режим игры и настраивает все юниты
    /// </summary>
    public void SetGameMode(GameMode gameMode)
    {
        if (currentGameMode == gameMode) return;
        
        Debug.Log($"[GameModeManager] Switching from {currentGameMode} to {gameMode}");
        
        currentGameMode = gameMode;

        OnGameModeChanged?.Invoke(gameMode);
    }
    
    /// <summary>
    /// Событие изменения режима игры
    /// </summary>
    public System.Action<GameMode> OnGameModeChanged { get; set; }
    
    /// <summary>
    /// Переключается в одиночный режим
    /// </summary>
    [ContextMenu("Switch to Singleplayer")]
    public void SwitchToSingleplayer()
    {
        SetGameMode(GameMode.Singleplayer);
    }
    
    /// <summary>
    /// Переключается в сетевой режим
    /// </summary>
    [ContextMenu("Switch to Multiplayer")]
    public void SwitchToMultiplayer()
    {
        SetGameMode(GameMode.Multiplayer);
    }
}
