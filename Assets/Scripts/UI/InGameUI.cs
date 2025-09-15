using System;
using TMPro;
using UnityEngine;
using Zenject;
using UniRx;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] Button Button;
    [Inject] GameController gameController;
    private void Start()
    {
        Button.onClick.AddListener(() => gameController.RunBattle());
    }
}
