namespace Adventure.Domain.Dialog
{
    public interface IDialogStateStore
    {
        void SaveState(DialogStateSnapshot snapshot);
        bool TryLoadState(string dialogId, out DialogStateSnapshot snapshot);
        void ClearState(string dialogId);
    }

    public readonly struct DialogStateSnapshot
    {
        public DialogStateSnapshot(string dialogId, string currentNodeId)
        {
            DialogId = dialogId;
            CurrentNodeId = currentNodeId;
        }

        public string DialogId { get; }
        public string CurrentNodeId { get; }
    }
}
