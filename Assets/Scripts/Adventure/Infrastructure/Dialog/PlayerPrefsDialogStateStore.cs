using Adventure.Domain.Dialog;
using UnityEngine;

namespace Adventure.Infrastructure.Dialog
{
    public sealed class PlayerPrefsDialogStateStore : IDialogStateStore
    {
        public void SaveState(DialogStateSnapshot snapshot)
        {
            if (string.IsNullOrEmpty(snapshot.DialogId))
                return;

            PlayerPrefs.SetString(GetKey(snapshot.DialogId), snapshot.CurrentNodeId ?? string.Empty);
        }

        public bool TryLoadState(string dialogId, out DialogStateSnapshot snapshot)
        {
            if (PlayerPrefs.HasKey(GetKey(dialogId)))
            {
                var nodeId = PlayerPrefs.GetString(GetKey(dialogId));
                snapshot = new DialogStateSnapshot(dialogId, nodeId);
                return true;
            }

            snapshot = default;
            return false;
        }

        public void ClearState(string dialogId)
        {
            if (PlayerPrefs.HasKey(GetKey(dialogId)))
            {
                PlayerPrefs.DeleteKey(GetKey(dialogId));
            }
        }

        private static string GetKey(string dialogId) => $"dialog_state_{dialogId}";
    }
}
