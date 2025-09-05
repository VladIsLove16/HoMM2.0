using UnityEngine;
using System.Collections.Generic;
using UniRx;

namespace Development
{
    public class UnitView3DEventTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private KeyCode testAttackKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode testHitKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode testDeathKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode testMoveKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode testHoverKey = KeyCode.Alpha5;
        
        private UnitView3D unitView;
        private UnitView3DComponentDebugger debugger;
        
        private void Start()
        {
            if (!enableTesting) return;
            
            unitView = GetComponent<UnitView3D>();
            debugger = GetComponent<UnitView3DComponentDebugger>();
            
            if (unitView == null)
            {
                Debug.LogWarning("UnitView3DEventTester: UnitView3D не найден!");
                return;
            }
            
            Debug.Log("UnitView3DEventTester: Нажмите клавиши 1-5 для тестирования событий");
        }
        
        private void Update()
        {
            if (!enableTesting || unitView == null) return;
            
            // Тест атаки
            if (Input.GetKeyDown(testAttackKey))
            {
                TestAttack();
            }
            
            // Тест получения урона
            if (Input.GetKeyDown(testHitKey))
            {
                TestHit();
            }
            
            // Тест смерти
            if (Input.GetKeyDown(testDeathKey))
            {
                TestDeath();
            }
            
            // Тест движения
            if (Input.GetKeyDown(testMoveKey))
            {
                TestMove();
            }
            
            // Тест hover
            if (Input.GetKeyDown(testHoverKey))
            {
                TestHover();
            }
        }
        
        private void TestAttack()
        {
            Debug.Log("=== Тест атаки ===");
            
            // Эмулируем атаку через ViewModel
            var vm = GetViewModel();
            if (vm != null)
            {
                TryInvokeSubject(vm, "_onAttacked");
            }
        }
        
        private void TestHit()
        {
            Debug.Log("=== Тест получения урона ===");
            
            // Эмулируем получение урона через ViewModel
            var vm = GetViewModel();
            if (vm != null)
            {
                TryInvokeSubject(vm, "_onHit");
            }
        }
        
        private void TestDeath()
        {
            Debug.Log("=== Тест смерти ===");
            
            // Эмулируем смерть через ViewModel
            var vm = GetViewModel();
            if (vm != null)
            {
                TryInvokeSubject(vm, "_onDeath");
            }
        }
        
        private void TestMove()
        {
            Debug.Log("=== Тест движения ===");
            
            // Создаем тестовый маршрут
            List<Vector3> route = new List<Vector3>
            {
                transform.position + Vector3.forward * 2f,
                transform.position + Vector3.forward * 4f + Vector3.right * 2f,
                transform.position + Vector3.forward * 6f
            };
            
            unitView.RequestMove(route);
        }
        
        private void TestHover()
        {
            Debug.Log("=== Тест hover ===");
            
            // Эмулируем hover
            unitView.Hover();
            
            // Через 2 секунды убираем hover
            StartCoroutine(UnhoverAfterDelay());
        }
        
        private System.Collections.IEnumerator UnhoverAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            unitView.UnHover();
        }
        
        private UnitViewModel GetViewModel()
        {
            // Получаем ViewModel через рефлексию
            var vmField = unitView.GetType().GetField("_vm", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return vmField?.GetValue(unitView) as UnitViewModel;
        }
        
        private void TryInvokeSubject(UnitViewModel vm, string fieldName)
        {
            var field = typeof(UnitViewModel).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var subject = field?.GetValue(vm) as Subject<Unit>;
            subject?.OnNext(Unit.Default);
        }
        
        private void OnGUI()
        {
            if (!enableTesting) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("UnitView3D Event Tester", GUI.skin.box);
            GUILayout.Label($"1 - Test Attack ({testAttackKey})");
            GUILayout.Label($"2 - Test Hit ({testHitKey})");
            GUILayout.Label($"3 - Test Death ({testDeathKey})");
            GUILayout.Label($"4 - Test Move ({testMoveKey})");
            GUILayout.Label($"5 - Test Hover ({testHoverKey})");
            GUILayout.EndArea();
        }
    }
} 