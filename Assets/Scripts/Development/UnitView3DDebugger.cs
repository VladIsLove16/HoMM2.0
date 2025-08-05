using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

namespace Development
{
    public class UnitView3DDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugger = true;
        [SerializeField] private bool showOnScreen = true;
        [SerializeField] private bool logToConsole = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        
        [Header("UI References")]
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private Button toggleButton;

        [SerializeField] private UnitView3D targetUnitView;
        private Dictionary<string, object> debugInfo = new Dictionary<string, object>();
        private bool isDebugPanelVisible = false;   

        [Button]
        private void SetupView()
        {
            SetupDebugUI();
            UpdateDebugInfo();
        }

        private void Update()
        {
            if (!enableDebugger || targetUnitView == null) return;
            
            // Переключение панели по клавише
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleDebugPanel();
            }
            
            UpdateDebugInfo();
            
            if (showOnScreen && isDebugPanelVisible)
            {
                UpdateDebugText();
            }
        }
        
        private void SetupDebugUI()
        {
            if (debugPanel == null)
            {
                CreateDebugUI();
            }
            
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleDebugPanel);
            }
            
            debugPanel.SetActive(false);
        }
        
        private void CreateDebugUI()
        {
            // Создаем панель для дебага
            GameObject canvasObj = new GameObject("DebugCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Создаем панель
            debugPanel = new GameObject("DebugPanel");
            debugPanel.transform.SetParent(canvasObj.transform, false);
            
            Image panelImage = debugPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform panelRect = debugPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(0.4f, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Создаем текст
            GameObject textObj = new GameObject("DebugText");
            textObj.transform.SetParent(debugPanel.transform, false);
            
            debugText = textObj.AddComponent<TextMeshProUGUI>();
            debugText.fontSize = 12;
            debugText.color = Color.white;
            debugText.alignment = TextAlignmentOptions.TopLeft;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 10);
            textRect.offsetMax = new Vector2(-10, -10);
            
            // Создаем кнопку переключения
            GameObject buttonObj = new GameObject("ToggleButton");
            buttonObj.transform.SetParent(canvasObj.transform, false);
            
            toggleButton = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.9f, 0.9f);
            buttonRect.anchorMax = new Vector2(0.98f, 0.98f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            
            // Текст кнопки
            GameObject buttonTextObj = new GameObject("ButtonText");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Debug";
            buttonText.fontSize = 10;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
        }
        
        private void UpdateDebugInfo()
        {
            if (targetUnitView == null) return;
            
            debugInfo.Clear();
            
            // Основная информация
            debugInfo["Unit Model"] = targetUnitView.Model?.GetType().Name ?? "null";
            debugInfo["Position"] = targetUnitView.transform.position;
            debugInfo["Rotation"] = targetUnitView.transform.rotation.eulerAngles;
            debugInfo["Scale"] = targetUnitView.transform.localScale;
            
            // Информация об аниматоре
            Animator animator = targetUnitView.GetComponent<Animator>();
            if (animator != null)
            {
                debugInfo["Animator Enabled"] = animator.enabled;
                debugInfo["Current Animation"] = GetCurrentAnimationName(animator);
                debugInfo["Animation Speed"] = animator.speed;
                debugInfo["Is Playing"] = animator.GetCurrentAnimatorStateInfo(0).length > 0;
            }
            
            // Информация о мешах
            var meshes = targetUnitView.GetType().GetField("meshes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetUnitView) as SkinnedMeshRenderer[];
            
            if (meshes != null)
            {
                debugInfo["Mesh Count"] = meshes.Length;
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i] != null)
                    {
                        debugInfo[$"Mesh {i} Material"] = meshes[i].material?.name ?? "null";
                        debugInfo[$"Mesh {i} Enabled"] = meshes[i].enabled;
                    }
                }
            }
            
            // Информация о ViewModel
            var vm = targetUnitView.GetType().GetField("_vm", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetUnitView);
            
            if (vm != null)
            {
                debugInfo["ViewModel Type"] = vm.GetType().Name;
                debugInfo["Team Material"] = vm.GetType().GetProperty("TeamMaterial")?.GetValue(vm)?.ToString() ?? "null";
                debugInfo["Hovered Team Material"] = vm.GetType().GetProperty("HoveredTeamMaterial")?.GetValue(vm)?.ToString() ?? "null";
            }
            
            // Информация о очереди действий
            var actionQueue = targetUnitView.GetType().GetField("actionQueue", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetUnitView) as Queue<System.Collections.IEnumerator>;
            
            if (actionQueue != null)
            {
                debugInfo["Action Queue Count"] = actionQueue.Count;
            }
            
            var isExecuting = targetUnitView.GetType().GetField("isExecuting", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetUnitView);
            
            if (isExecuting != null)
            {
                debugInfo["Is Executing Actions"] = isExecuting;
            }
            
            // Информация о Disposables
            var disposables = targetUnitView.GetType().GetField("_disposables", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(targetUnitView);
            
            if (disposables != null)
            {
                debugInfo["Disposables Count"] = disposables.GetType().GetProperty("Count")?.GetValue(disposables) ?? "unknown";
            }
        }
        
        private string GetCurrentAnimationName(Animator animator)
        {
            if (animator.GetCurrentAnimatorStateInfo(0).length > 0)
            {
                return animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") ? "Idle" :
                       animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") ? "Walk" :
                       animator.GetCurrentAnimatorStateInfo(0).IsName("DealDamage") ? "Attack" :
                       animator.GetCurrentAnimatorStateInfo(0).IsName("Hit") ? "Hit" :
                       animator.GetCurrentAnimatorStateInfo(0).IsName("Die") ? "Die" :
                       "Unknown";
            }
            return "None";
        }
        
        private void UpdateDebugText()
        {
            if (debugText == null) return;
            
            string debugString = "=== UnitView3D Debug Info ===\n\n";
            
            foreach (var kvp in debugInfo)
            {
                debugString += $"{kvp.Key}: {kvp.Value}\n";
            }
            
            debugString += "\n=== Controls ===\n";
            debugString += $"Toggle Panel: {toggleKey}\n";
            debugString += $"Panel Visible: {isDebugPanelVisible}\n";
            
            debugText.text = debugString;
        }
        
        private void ToggleDebugPanel()
        {
            if (debugPanel == null) return;
            
            isDebugPanelVisible = !isDebugPanelVisible;
            debugPanel.SetActive(isDebugPanelVisible);
            
            if (logToConsole)
            {
                Debug.Log($"UnitView3D Debug Panel: {(isDebugPanelVisible ? "Shown" : "Hidden")}");
            }
        }
        
        // Публичные методы для внешнего управления
        public void ShowDebugPanel()
        {
            if (debugPanel != null)
            {
                isDebugPanelVisible = true;
                debugPanel.SetActive(true);
            }
        }
        
        public void HideDebugPanel()
        {
            if (debugPanel != null)
            {
                isDebugPanelVisible = false;
                debugPanel.SetActive(false);
            }
        }
        
        public void SetTargetUnitView(UnitView3D unitView)
        {
            targetUnitView = unitView;
        }
        
        public void LogDebugInfo()
        {
            if (!logToConsole) return;
            
            Debug.Log("=== UnitView3D Debug Info ===");
            foreach (var kvp in debugInfo)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
        
        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveAllListeners();
            }
        }
    }
} 