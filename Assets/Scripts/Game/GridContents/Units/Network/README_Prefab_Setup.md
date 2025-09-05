# 🎯 Настройка Prefab Variants для Multiplayer

## 📋 Обзор

Система использует **Prefab Variants** для разделения локальных и сетевых версий юнитов. Это обеспечивает:
- ✅ **DRY принцип** - общая логика в базовом префабе
- ✅ **Производительность** - нет runtime операций
- ✅ **Четкая структура** - явно видно различия
- ✅ **Enterprise стандарт** - используется в крупных проектах

---

## 🏗️ Структура префабов

```
Assets/Prefabs/Units/
├── CharacterWarrok.prefab (базовый префаб)
├── CharacterWarrok_Local.prefab (variant без NetworkObject)
├── CharacterWarrok_Network.prefab (variant с NetworkObject)
├── CharacterArcher.prefab (базовый префаб)
├── CharacterArcher_Local.prefab (variant без NetworkObject)
└── CharacterArcher_Network.prefab (variant с NetworkObject)
```

---

## 🛠️ Пошаговая настройка

### **Шаг 1: Создание базового префаба**

1. **Создайте базовый префаб** `CharacterWarrok.prefab`
2. **Добавьте компоненты:**
   - `RectTransform`
   - `Animator`
   - `UnitView3D`
   - `NetworkObject` (для базового префаба)
   - `NetworkTransform`

### **Шаг 2: Создание Local Variant**

1. **Создайте Local variant:**
   - Правый клик на базовом префабе → `Create → Prefab Variant`
   - Переименуйте в `CharacterWarrok_Local`

2. **Удалите сетевые компоненты:**
   - Удалите `NetworkObject`
   - Удалите `NetworkTransform`

### **Шаг 3: Создание Network Variant**

1. **Создайте Network variant:**
   - Правый клик на базовом префабе → `Create → Prefab Variant`
   - Переименуйте в `CharacterWarrok_Network`

2. **Настройте NetworkObject:**
   - Убедитесь, что `GlobalObjectIdHash` уникален
   - Настройте `Ownership` и `SynchronizeTransform`

### **Шаг 4: Настройка UnitPrefabManagerConfig**

1. **Откройте** `Assets/ScriptableObjects/Game/UnitPrefabManagerConfig.asset`
2. **Настройте префабы:**
   ```csharp
   UnitType: Warrok (1)
   LocalVariant: CharacterWarrok_Local.prefab
   NetworkVariant: CharacterWarrok_Network.prefab
   ```

### **Шаг 5: Настройка UnitDefinitionSO**

1. **Откройте** `Assets/ScriptableObjects/Units/Warrok/Warrok.asset`
2. **Настройте основной префаб:**
   ```csharp
   UnitViewPrefab: CharacterWarrok.prefab (базовый)
   ```

---

## 🔧 Конфигурация компонентов

### **NetworkObject настройки:**
```csharp
GlobalObjectIdHash: 2906396352 (уникальный для каждого префаба)
Ownership: Owner
SynchronizeTransform: true
ActiveSceneSynchronization: false
SceneMigrationSynchronization: true
```

### **NetworkTransform настройки:**
```csharp
SyncPositionX/Y/Z: true
SyncRotAngleX/Y/Z: true
SyncScaleX/Y/Z: true
PositionThreshold: 0.001
RotAngleThreshold: 0.01
ScaleThreshold: 0.01
```

---

## ✅ Валидация

### **Автоматическая валидация:**
- `UnitPrefabManager` автоматически валидирует конфигурацию
- Проверяет наличие всех необходимых префабов
- Выводит ошибки в консоль при проблемах

### **Ручная валидация:**
```csharp
// В UnitPrefabManagerConfig
[ContextMenu("Validate All Prefabs")]
public void ValidateAllPrefabs()
```

---

## 🚀 Использование

### **В коде:**
```csharp
// Получение префаба для режима игры
var prefab = _prefabManager.GetPrefab(UnitType.Warrok, GameMode.Multiplayer);

// Проверка наличия префаба
if (_prefabManager.HasPrefab(UnitType.Warrok))
{
    // Создание юнита
}
```

### **В Unity Editor:**
1. **Настройте** `UnitPrefabManager` на сцене
2. **Привяжите** `UnitPrefabManagerConfig`
3. **Проверьте** валидацию в консоли

---

## 🎨 Материалы и внешний вид

### **Настройка материалов:**
- **Каждый юнит** имеет свой `MaterialProvider`
- **Материалы настраиваются** в `UnitDefinitionSO`
- **Разные команды** могут иметь разные материалы

### **Пример конфигурации:**
```csharp
_materialProvider:
  _blueTeamMaterial: BlueTeamMaterial
  _hoveredBlueTeamMaterial: BlueTeamHoveredMaterial
  _redTeamMaterial: RedTeamMaterial
  _hoveredRedTeamMaterial: RedTeamHoveredMaterial
```

---

## 🔍 Отладка

### **Частые проблемы:**

1. **GlobalObjectIdHash дублируется:**
   - Убедитесь, что каждый NetworkObject имеет уникальный hash
   - Проверьте, что не создаете несколько экземпляров одного префаба

2. **Префаб не найден:**
   - Проверьте конфигурацию в `UnitPrefabManagerConfig`
   - Убедитесь, что все префабы назначены

3. **Компоненты отсутствуют:**
   - Проверьте, что Local variant не имеет NetworkObject
   - Убедитесь, что Network variant имеет все сетевые компоненты

### **Логи для отладки:**
```csharp
Debug.Log($"[UnitPrefabManager] Getting prefab for {unitType} in {gameMode}");
Debug.LogError($"[UnitPrefabManager] Prefab not found for unit type: {unitType}");
```

---

## 📚 Дополнительные ресурсы

- **Unity Prefab Variants:** [Документация](https://docs.unity3d.com/Manual/PrefabVariants.html)
- **Netcode for GameObjects:** [Документация](https://docs-multiplayer.unity3d.com/)
- **ScriptableObjects:** [Документация](https://docs.unity3d.com/Manual/class-ScriptableObject.html)

---

## 🎯 Результат

После настройки у вас будет:
- ✅ **Правильная структура префабов** с variants
- ✅ **Централизованное управление** через конфигурацию
- ✅ **Автоматическая валидация** конфигурации
- ✅ **Enterprise-уровень архитектуры** для multiplayer
- ✅ **Легкость поддержки** и масштабирования
