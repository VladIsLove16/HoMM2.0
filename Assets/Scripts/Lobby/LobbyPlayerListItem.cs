using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Компонент для отображения информации об игроке в списке лобби
/// </summary>
public class LobbyPlayerListItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text _playerNameText;
    [SerializeField] private Text _statusText;
    [SerializeField] private Image _statusIcon;
    
    /// <summary>
    /// Настройка отображения данных игрока
    /// </summary>
    /// <param name="playerData">Данные игрока</param>
    public void Setup(LobbyPlayerData playerData)
    {
        if (_playerNameText != null)
        {
            _playerNameText.text = playerData.PlayerName.ToString();
        }
        
        if (_statusText != null)
        {
            _statusText.text = playerData.IsReady ? "Ready" : "Not Ready";
        }
        
        if (_statusIcon != null)
        {
            _statusIcon.color = playerData.IsReady ? Color.green : Color.red;
        }
    }
}
