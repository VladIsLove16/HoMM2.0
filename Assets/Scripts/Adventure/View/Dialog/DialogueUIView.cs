using System;
using System.Collections.Generic;
using Adventure.Application.Dialog;
using Adventure.Domain.Dialog;
using Adventure.Integration.Battle;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using SharedView;

namespace Adventure.Presentation.Dialog
{
    public sealed class DialogueUIView : CanvasGroupPanelViewBase<DialogVM>
    {
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button choiceButtonPrefab;

        private readonly List<Button> _spawnedButtons = new List<Button>();

        [Inject]
        public override void Construct(DialogVM viewModel)
        {
            base.Construct(viewModel);
        }

        protected override void OnInitialized()
        {
            ViewModel.CurrentNode.Subscribe(OnNodeChanged).AddTo(Bindings);
            ViewModel.EnemyArmy.Subscribe(OnVMEnemyArmyChanged).AddTo(Bindings);
        }

        protected override void OnDestroy()
        {
            ClearChoicesButtons();
            base.OnDestroy();
        }

        private void OnVMEnemyArmyChanged(ArmyLineupSO x)
        {
        }

        private void OnNodeChanged(DialogueNode node)
        {
            if (node == null)
            {
                Debug.LogWarning("DialogueNode setted null");
                return;
            }

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
            button.onClick.AddListener(() => OnButtonClicked(choice));
            _spawnedButtons.Add(button);
        }

        private void OnButtonClicked(DialogueChoice choice)
        {
            ViewModel.SelectChoice(choice.Id);
            Debug.Log("u clicked choice " + choice.Text);   
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
    }
}
