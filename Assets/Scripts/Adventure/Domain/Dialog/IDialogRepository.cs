namespace Adventure.Domain.Dialog
{
    public interface IDialogRepository
    {
        bool TryGet(string dialogId, out DialogueGraph graph);
    }
}
