using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Development
{
    [RequireComponent(typeof(UnitView3D))]
    public class UnitView3DComponentDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugger = true;
        [SerializeField] private bool logEvents = true;
        [SerializeField] private bool logAnimations = true;
        [SerializeField] private bool logMovement = true;
        [SerializeField] private bool logMaterialChanges = true;
        [SerializeField] private bool showGizmos = true;
        
        [Header("Gizmo Settings")]
        [SerializeField] private Color gizmoColor = Color.yellow;
        [SerializeField] private float gizmoSize = 0.5f;
        
        private UnitView3D unitView;
        private Animator animator;
        private SkinnedMeshRenderer[] meshes;
        
        // История событий
        private List<string> eventHistory = new List<string>();
        private int maxEventHistory = 50;
        
        // Статистика
        private int totalAnimationsPlayed = 0;
        private int totalMovements = 0;
        private int totalMaterialChanges = 0;
        private float lastEventTime = 0f;
        
        private void Start()
        {
            if (!enableDebugger) return;
            
            unitView = GetComponent<UnitView3D>();
            animator = GetComponent<Animator>();
            
            if (unitView == null)
            {
                Debug.LogError("UnitView3DComponentDebugger: UnitView3D не найден на этом объекте!");
                return;
            }
            
            // Получаем ссылку на меши через рефлексию
            var meshesField = unitView.GetType().GetField("meshes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            meshes = meshesField?.GetValue(unitView) as SkinnedMeshRenderer[];

            LogEvent("UnitView3DComponentDebugger инициализирован");
            
            // Подписываемся на события UnitView3D
            SubscribeToUnitViewEvents();
        }
        
        private void SubscribeToUnitViewEvents()
        {
            // Здесь можно добавить подписку на события, если они есть
            LogEvent("Подписка на события UnitView3D завершена");
        }

        private void Update()
        {
            if (!enableDebugger || unitView == null) return;
            
            // Отслеживаем изменения анимации
            if (logAnimations && animator != null)
            {
                var currentState = animator.GetCurrentAnimatorStateInfo(0);
                if (currentState.length > 0)
                {
                    string animationName = GetAnimationName(currentState);
                    if (animationName != lastAnimationName)
                    {
                        LogEvent($"Анимация изменена: {animationName}");
                        lastAnimationName = animationName;
                        totalAnimationsPlayed++;
                    }
                }
            }
            
            // Отслеживаем изменения позиции
            if (logMovement)
            {
                if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
                {
                    LogEvent($"Позиция изменена: {transform.position}");
                    lastPosition = transform.position;
                    totalMovements++;
                }
            }
            
            // Отслеживаем изменения материалов
            if (logMaterialChanges && meshes != null)
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i] != null && meshes[i].material != null)
                    {
                        string materialName = meshes[i].material.name;
                        if (!lastMaterialNames.ContainsKey(i) || lastMaterialNames[i] != materialName)
                        {
                            LogEvent($"Материал меша {i} изменен: {materialName}");
                            lastMaterialNames[i] = materialName;
                            totalMaterialChanges++;
                        }
                    }
                }
            }
        }
        
        private string lastAnimationName = "";
        private Vector3 lastPosition;
        private Dictionary<int, string> lastMaterialNames = new Dictionary<int, string>();
        
        private string GetAnimationName(AnimatorStateInfo stateInfo)
        {
            if (stateInfo.IsName("Idle")) return "Idle";
            if (stateInfo.IsName("Walk")) return "Walk";
            if (stateInfo.IsName("DealDamage")) return "Attack";
            if (stateInfo.IsName("Hit")) return "Hit";
            if (stateInfo.IsName("Die")) return "Die";
            return "Unknown";
        }
        
        private void LogEvent(string message)
        {
            if (!logEvents) return;
            
            float timeSinceStart = Time.time;
            string timeStamp = $"[{timeSinceStart:F2}s]";
            string fullMessage = $"{timeStamp} {message}";
            
            eventHistory.Add(fullMessage);
            
            // Ограничиваем историю событий
            if (eventHistory.Count > maxEventHistory)
            {
                eventHistory.RemoveAt(0);
            }
            
            Debug.Log($"[UnitView3D Debug] {fullMessage}"); 
            lastEventTime = timeSinceStart;
        }
        
        // Публичные методы для внешнего вызова
        public void LogCustomEvent(string eventName, object data = null)
        {
            string message = data != null ? $"{eventName}: {data}" : eventName;
            LogEvent(message);
        }
        
        public void LogAnimationEvent(string animationName)
        {
            LogEvent($"Анимация воспроизведена: {animationName}");
            totalAnimationsPlayed++;
        }
        
        public void LogMovementEvent(Vector3 from, Vector3 to)
        {
            LogEvent($"Движение: {from} -> {to}");
            totalMovements++;
        }
        
        public void LogMaterialChangeEvent(int meshIndex, string oldMaterial, string newMaterial)
        {
            LogEvent($"Материал меша {meshIndex}: {oldMaterial} -> {newMaterial}");
            totalMaterialChanges++;
        }
        
        // Методы для получения статистики
        public Dictionary<string, object> GetDebugStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["Total Animations"] = totalAnimationsPlayed,
                ["Total Movements"] = totalMovements,
                ["Total Material Changes"] = totalMaterialChanges,
                ["Event History Count"] = eventHistory.Count,
                ["Last Event Time"] = lastEventTime,
                ["Current Position"] = transform.position,
                ["Current Animation"] = lastAnimationName,
                ["Is Executing Actions"] = GetIsExecutingStatus()
            };
            
            return stats;
        }
        
        private bool GetIsExecutingStatus()
        {
            var isExecutingField = unitView.GetType().GetField("isExecuting", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return isExecutingField?.GetValue(unitView) is bool executing && executing;
        }
        
        public List<string> GetEventHistory()
        {
            return new List<string>(eventHistory);
        }
        
        public void ClearEventHistory()
        {
            eventHistory.Clear();
            LogEvent("История событий очищена");
        }
        
        public void ResetStats()
        {
            totalAnimationsPlayed = 0;
            totalMovements = 0;
            totalMaterialChanges = 0;
            lastEventTime = 0f;
            LogEvent("Статистика сброшена");
        }
        
        // Gizmos для визуализации в Scene View
        private void OnDrawGizmos()
        {
            if (!showGizmos || unitView == null) return;
            
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
            
            // Рисуем стрелку направления
            Gizmos.color = Color.blue;
            Vector3 forward = transform.forward * gizmoSize * 1.5f;
            Gizmos.DrawRay(transform.position, forward);
            
            // Рисуем точку в начале стрелки
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + forward, gizmoSize * 0.2f);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos || unitView == null) return;
            
            // Показываем дополнительную информацию при выделении
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize * 2);
        }
        
        // Методы для интеграции с другими системами
        public void OnUnitAttacked()
        {
            LogEvent("Юнит атакован");
        }
        
        public void OnUnitHit()
        {
            LogEvent("Юнит получил урон");
        }
        
        public void OnUnitDeath()
        {
            LogEvent("Юнит умер");
        }
        
        public void OnUnitTurnStarted()
        {
            LogEvent("Ход юнита начался");
        }
        
        public void OnUnitMoved(List<Vector3> route)
        {
            LogEvent($"Юнит движется по маршруту из {route.Count} точек");
        }
        
        public void OnUnitHovered()
        {
            LogEvent("Юнит в фокусе (hover)");
        }
        
        public void OnUnitUnhovered()
        {
            LogEvent("Юнит потерял фокус (unhover)");
        }
        
        private void OnDestroy()
        {
            if (enableDebugger)
            {
                LogEvent("UnitView3DComponentDebugger уничтожен");
            }
        }
    }
} 