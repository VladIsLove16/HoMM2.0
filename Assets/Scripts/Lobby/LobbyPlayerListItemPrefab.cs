using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Пример создания префаба для элемента списка игроков в лобби
/// </summary>
public class LobbyPlayerListItemPrefab : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Text _playerNameText;
    [SerializeField] private Text _statusText;
    [SerializeField] private Image _statusIcon;
    [SerializeField] private Image _backgroundImage;
    
    [Header("Visual Settings")]
    [SerializeField] private Color _readyColor = Color.green;
    [SerializeField] private Color _notReadyColor = Color.red;
    [SerializeField] private Color _hostColor = Color.blue;
    
    /// <summary>
    /// Настройка отображения данных игрока
    /// </summary>
    public void Setup(LobbyPlayerData playerData, bool isHost = false)
    {
        if (_playerNameText != null)
        {
            _playerNameText.text = isHost ? $"{playerData.PlayerName} (Host)" : playerData.PlayerName.ToString();
        }
        
        if (_statusText != null)
        {
            _statusText.text = playerData.IsReady ? "Ready" : "Not Ready";
        }
        
        if (_statusIcon != null)
        {
            _statusIcon.color = playerData.IsReady ? _readyColor : _notReadyColor;
        }
        
        if (_backgroundImage != null)
        {
            _backgroundImage.color = isHost ? _hostColor : Color.white;
        }
    }
    
    /// <summary>
    /// Обновление статуса готовности
    /// </summary>
    public void UpdateReadyStatus(bool isReady)
    {
        if (_statusText != null)
        {
            _statusText.text = isReady ? "Ready" : "Not Ready";
        }
        
        if (_statusIcon != null)
        {
            _statusIcon.color = isReady ? _readyColor : _notReadyColor;
        }
    }
}
