using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace Development.Editor
{
    [CustomEditor(typeof(UnitView3D))]
    public class UnitView3DEditorDebugger : UnityEditor.Editor
    {
        private bool showDebugInfo = true;
        private bool showAnimationInfo = true;
        private bool showMaterialInfo = true;
        private bool showActionQueueInfo = true;
        private bool showViewModelInfo = true;
        private bool showEventHistory = false;
        
        private Vector2 scrollPosition;
        private List<string> eventHistory = new List<string>();
        private Dictionary<string, object> debugStats = new Dictionary<string, object>();
        
        public override void OnInspectorGUI()
        {
            UnitView3D unitView = (UnitView3D)target;
            
            // Рисуем стандартный инспектор
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== UnitView3D Debug Info ===", EditorStyles.boldLabel);
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug info доступен только в Play Mode", MessageType.Info);
                return;
            }
            
            // Кнопки управления
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Debug Info"))
            {
                UpdateDebugInfo(unitView);
            }
            if (GUILayout.Button("Clear Event History"))
            {
                eventHistory.Clear();
            }
            if (GUILayout.Button("Log to Console"))
            {
                LogDebugInfoToConsole(unitView);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Основная информация
            showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Basic Info", true);
            if (showDebugInfo)
            {
                EditorGUI.indentLevel++;
                DrawBasicInfo(unitView);
                EditorGUI.indentLevel--;
            }
            
            // Информация об анимациях
            showAnimationInfo = EditorGUILayout.Foldout(showAnimationInfo, "Animation Info", true);
            if (showAnimationInfo)
            {
                EditorGUI.indentLevel++;
                DrawAnimationInfo(unitView);
                EditorGUI.indentLevel--;
            }
            
            // Информация о материалах
            showMaterialInfo = EditorGUILayout.Foldout(showMaterialInfo, "Material Info", true);
            if (showMaterialInfo)
            {
                EditorGUI.indentLevel++;
                DrawMaterialInfo(unitView);
                EditorGUI.indentLevel--;
            }
            
            // Информация об очереди действий
            showActionQueueInfo = EditorGUILayout.Foldout(showActionQueueInfo, "Action Queue Info", true);
            if (showActionQueueInfo)
            {
                EditorGUI.indentLevel++;
                DrawActionQueueInfo(unitView);
                EditorGUI.indentLevel--;
            }
            
            // Информация о ViewModel
            showViewModelInfo = EditorGUILayout.Foldout(showViewModelInfo, "ViewModel Info", true);
            if (showViewModelInfo)
            {
                EditorGUI.indentLevel++;
                DrawViewModelInfo(unitView);
                EditorGUI.indentLevel--;
            }
            
            // История событий
            showEventHistory = EditorGUILayout.Foldout(showEventHistory, "Event History", true);
            if (showEventHistory)
            {
                EditorGUI.indentLevel++;
                DrawEventHistory();
                EditorGUI.indentLevel--;
            }
            
            // Автообновление
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
        
        private void UpdateDebugInfo(UnitView3D unitView)
        {
            debugStats.Clear();
            
            // Основная информация
            debugStats["Position"] = unitView.transform.position;
            debugStats["Rotation"] = unitView.transform.rotation.eulerAngles;
            debugStats["Scale"] = unitView.transform.localScale;
            debugStats["Model"] = unitView.Model?.GetType().Name ?? "null";
            
            // Информация об аниматоре
            Animator animator = unitView.GetComponent<Animator>();
            if (animator != null)
            {
                debugStats["Animator Enabled"] = animator.enabled;
                debugStats["Animation Speed"] = animator.speed;
                debugStats["Current Animation"] = GetCurrentAnimationName(animator);
                debugStats["Is Playing"] = animator.GetCurrentAnimatorStateInfo(0).length > 0;
            }
            
            // Информация о мешах
            var meshes = GetPrivateField<SkinnedMeshRenderer[]>(unitView, "meshes");
            if (meshes != null)
            {
                debugStats["Mesh Count"] = meshes.Length;
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i] != null)
                    {
                        debugStats[$"Mesh {i} Material"] = meshes[i].material?.name ?? "null";
                        debugStats[$"Mesh {i} Enabled"] = meshes[i].enabled;
                    }
                }
            }
            
            // Информация о ViewModel
            var vm = GetPrivateField<object>(unitView, "_vm");
            if (vm != null)
            {
                debugStats["ViewModel Type"] = vm.GetType().Name;
                debugStats["Team Material"] = GetPropertyValue(vm, "TeamMaterial")?.ToString() ?? "null";
                debugStats["Hovered Team Material"] = GetPropertyValue(vm, "HoveredTeamMaterial")?.ToString() ?? "null";
            }
            
            // Информация о очереди действий
            var actionQueue = GetPrivateField<Queue<System.Collections.IEnumerator>>(unitView, "actionQueue");
            if (actionQueue != null)
            {
                debugStats["Action Queue Count"] = actionQueue.Count;
            }
            
            var isExecuting = GetPrivateField<bool>(unitView, "isExecuting");
            debugStats["Is Executing Actions"] = isExecuting;
            
            // Информация о Disposables
            var disposables = GetPrivateField<object>(unitView, "_disposables");
            if (disposables != null)
            {
                debugStats["Disposables Count"] = GetPropertyValue(disposables, "Count") ?? "unknown";
            }
        }
        
        private void DrawBasicInfo(UnitView3D unitView)
        {
            foreach (var kvp in debugStats)
            {
                if (kvp.Key.StartsWith("Position") || kvp.Key.StartsWith("Rotation") || 
                    kvp.Key.StartsWith("Scale") || kvp.Key.StartsWith("Model"))
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value?.ToString() ?? "null");
                }
            }
        }
        
        private void DrawAnimationInfo(UnitView3D unitView)
        {
            foreach (var kvp in debugStats)
            {
                if (kvp.Key.StartsWith("Animator") || kvp.Key.StartsWith("Animation") || 
                    kvp.Key.StartsWith("Current Animation") || kvp.Key.StartsWith("Is Playing"))
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value?.ToString() ?? "null");
                }
            }
        }
        
        private void DrawMaterialInfo(UnitView3D unitView)
        {
            foreach (var kvp in debugStats)
            {
                if (kvp.Key.StartsWith("Mesh") || kvp.Key.StartsWith("Material"))
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value?.ToString() ?? "null");
                }
            }
        }
        
        private void DrawActionQueueInfo(UnitView3D unitView)
        {
            foreach (var kvp in debugStats)
            {
                if (kvp.Key.StartsWith("Action") || kvp.Key.StartsWith("Is Executing"))
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value?.ToString() ?? "null");
                }
            }
        }
        
        private void DrawViewModelInfo(UnitView3D unitView)
        {
            foreach (var kvp in debugStats)
            {
                if (kvp.Key.StartsWith("ViewModel") || kvp.Key.StartsWith("Team Material"))
                {
                    EditorGUILayout.LabelField(kvp.Key, kvp.Value?.ToString() ?? "null");
                }
            }
        }
        
        private void DrawEventHistory()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            if (eventHistory.Count == 0)
            {
                EditorGUILayout.LabelField("История событий пуста");
            }
            else
            {
                foreach (string eventText in eventHistory)
                {
                    EditorGUILayout.LabelField(eventText, EditorStyles.miniLabel);
                }
            }
            
            EditorGUILayout.EndScrollView();
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
        
        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null ? (T)field.GetValue(obj) : default(T);
        }
        
        private object GetPropertyValue(object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj);
        }
        
        private void LogDebugInfoToConsole(UnitView3D unitView)
        {
            UpdateDebugInfo(unitView);
            
            Debug.Log("=== UnitView3D Debug Info ===");
            foreach (var kvp in debugStats)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value}");
            }
        }
        
        // Методы для добавления событий в историю
        public void AddEventToHistory(string eventText)
        {
            string timeStamp = $"[{Time.time:F2}s]";
            string fullEvent = $"{timeStamp} {eventText}";
            eventHistory.Add(fullEvent);
            
            // Ограничиваем историю
            if (eventHistory.Count > 100)
            {
                eventHistory.RemoveAt(0);
            }
        }
        
        // Методы для вызова из других скриптов
        public void LogAnimationEvent(string animationName)
        {
            AddEventToHistory($"Анимация: {animationName}");
        }
        
        public void LogMovementEvent(Vector3 from, Vector3 to)
        {
            AddEventToHistory($"Движение: {from} -> {to}");
        }
        
        public void LogMaterialChangeEvent(string oldMaterial, string newMaterial)
        {
            AddEventToHistory($"Материал: {oldMaterial} -> {newMaterial}");
        }
        
        public void LogActionQueueEvent(string actionType)
        {
            AddEventToHistory($"Действие в очереди: {actionType}");
        }
    }
} 