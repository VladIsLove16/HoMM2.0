using System.Collections.Generic;
using UnityEngine;
namespace Adventure.Domain.Dialog
{
    [CreateAssetMenu(menuName = "Adventure/Dialogue/Database", fileName = "DialogueDatabase")]
    public sealed class DialogueDatabaseSO : ScriptableObject, IDialogRepository
    {
        [SerializeField] private List<DialogueGraphSO> graphs = new List<DialogueGraphSO>();

        public bool TryGet(string dialogId, out DialogueGraph graph)
        {
            foreach (var g in graphs)
            {
                if (g != null && g.Id == dialogId)
                {
                    graph = g.ToDomain();
                    return true;
                }
            }

            graph = null;
            return false;
        }
    }
}
