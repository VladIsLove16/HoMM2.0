using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyRoomListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private Button joinButton;

    private LobbyRoomInfo _roomInfo;
    private Action<LobbyRoomInfo> _joinAction;

    private void Awake()
    {
        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(HandleJoinClicked);
        }
    }

    public void Setup(LobbyRoomInfo roomInfo, Action<LobbyRoomInfo> joinAction)
    {
        _roomInfo = roomInfo;
        _joinAction = joinAction;

        if (roomNameText != null)
            roomNameText.text = roomInfo.Name;

        if (detailsText != null)
        {
            var hostName = string.IsNullOrWhiteSpace(roomInfo.HostName) ? "Unknown host" : roomInfo.HostName;
            var flow = string.Equals(roomInfo.Flow, "gridfight", StringComparison.OrdinalIgnoreCase)
                ? "GridFight"
                : "Adventure";
            detailsText.text = $"{hostName}  {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}  {flow}";
        }

        if (joinButton != null)
            joinButton.interactable = roomInfo.AvailableSlots > 0 && !roomInfo.IsLocked;
    }

    private void HandleJoinClicked()
    {
        _joinAction?.Invoke(_roomInfo);
    }
}
