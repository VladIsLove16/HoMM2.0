using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDebugPanelBtn : MonoBehaviour
{
    Button Button;
    TextMeshProUGUI textMeshProUGUI;
    [SerializeField]
    public GameObject DebugPanel;
    private void Awake()
    {
        Button = GetComponent<Button>();
        textMeshProUGUI = GetComponentInChildren<TextMeshProUGUI>();
        Button.onClick.AddListener(() => { ToggleDebugPanel(!DebugPanel.activeInHierarchy); });
        ToggleDebugPanel(true);

    }
    private void ToggleDebugPanel(bool newState)
    {
        DebugPanel.SetActive(newState);
        if(!newState)
            textMeshProUGUI.text = "Open Debug Panel";
        else
            textMeshProUGUI.text = "Hide Debug Panel";
    }
}
