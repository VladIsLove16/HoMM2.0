using System.Collections.Generic;
using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Adventure.Presentation.Dialog
{
    public sealed class DialogueUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button choiceButtonPrefab;

        private DialogVM _viewModel;
        private readonly List<Button> _spawnedButtons = new List<Button>();
        private readonly CompositeDisposable _bindings = new CompositeDisposable();
        [Inject]
        public void Construct(DialogVM viewModel)
        {
            _viewModel = viewModel;
            _viewModel.CurrentNode.Subscribe(OnNodeChanged).AddTo(_bindings);
            _viewModel.EnemyArmy.Subscribe((x)=>Debug.Log("enemy army count " + x.Convert().Count)).AddTo(_bindings);
            Hide();
        }

        private void OnDestroy()
        {
            _bindings.Dispose();
        }

        private void OnNodeChanged(DialogueNode node)
        {
            if (node == null)
            {
                Hide();
                return;
            }

            Show();
            if (speakerText != null)
                speakerText.text = node.Speaker;
            if (bodyText != null)
                bodyText.text = node.Text;

            RebuildChoices(node);
        }

        private void RebuildChoices(DialogueNode node)
        {
            ClearChoicesButtons();
            SpawnChoicesButtons(node);
        }

        private void SpawnChoicesButtons(DialogueNode node)
        {
            foreach (var choice in node.Choices)
            {
                SpawnButton(choice);
            }
        }

        private void SpawnButton(DialogueChoice choice)
        {
            var button = Instantiate(choiceButtonPrefab, choicesRoot);
            var label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = choice.Text;
            button.onClick.AddListener(() => _viewModel.SelectChoice(choice.Id));
            _spawnedButtons.Add(button);
        }

        private void ClearChoicesButtons()
        {
            foreach (var button in _spawnedButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _spawnedButtons.Clear();
        }

        private void Show()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private void Hide()
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
