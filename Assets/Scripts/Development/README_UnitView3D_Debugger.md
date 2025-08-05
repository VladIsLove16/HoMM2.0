# UnitView3D Debugger System

Система дебаггеров для класса `UnitView3D` включает в себя три различных инструмента для отладки и мониторинга.

## Компоненты системы

### 1. UnitView3DDebugger
**Файл:** `Assets/Scripts/Development/UnitView3DComponentDebugger.cs`

Глобальный дебаггер, который можно добавить на любой GameObject в сцене. Автоматически находит `UnitView3D` в сцене и отображает информацию в UI панели.

**Особенности:**
- Автоматическое создание UI панели
- Переключение по клавише F1 (настраивается)
- Отображение всей важной информации о UnitView3D
- Возможность логирования в консоль

**Использование:**
1. Добавьте компонент `UnitView3DDebugger` на любой GameObject в сцене
2. Настройте параметры в инспекторе
3. Нажмите F1 для переключения панели

### 2. UnitView3DComponentDebugger
**Файл:** `Assets/Scripts/Development/UnitView3DComponentDebugger.cs`

Компонент-дебаггер, который прикрепляется непосредственно к объекту с `UnitView3D`. Требует наличие компонента `UnitView3D` на том же объекте.

**Особенности:**
- Детальное отслеживание событий
- История событий с временными метками
- Статистика анимаций, движений, изменений материалов
- Визуализация в Scene View (Gizmos)
- Автоматическое логирование изменений

**Использование:**
1. Добавьте компонент `UnitView3DComponentDebugger` на объект с `UnitView3D`
2. Настройте параметры в инспекторе
3. Следите за событиями в консоли и Scene View

### 3. UnitView3DEditorDebugger
**Файл:** `Assets/Scripts/Development/Editor/UnitView3DEditorDebugger.cs`

Editor-скрипт, который расширяет инспектор `UnitView3D` в Unity Editor.

**Особенности:**
- Дополнительная информация в инспекторе
- Группированная информация (анимации, материалы, очередь действий)
- История событий
- Кнопки для управления дебагом

**Использование:**
1. Выберите объект с `UnitView3D` в иерархии
2. В инспекторе появится дополнительная секция "UnitView3D Debug Info"
3. Используйте кнопки для управления

### 4. UnitView3DEventTester
**Файл:** `Assets/Scripts/Development/UnitView3DEventTester.cs`

Тестовый компонент для эмуляции событий `UnitView3D` и проверки работы дебаггеров.

**Особенности:**
- Эмуляция всех основных событий (атака, урон, смерть, движение, hover)
- Управление клавишами 1-5
- Визуальный интерфейс с инструкциями
- Автоматическое тестирование дебаггеров

**Использование:**
1. Добавьте компонент на объект с `UnitView3D`
2. Запустите игру
3. Нажмите клавиши 1-5 для тестирования событий
4. Следите за логами в консоли и дебаг панелях

## Отслеживаемые параметры

### Основная информация
- Позиция, поворот, масштаб
- Тип модели юнита
- Состояние аниматора

### Анимации
- Текущая анимация (Idle, Walk, Attack, Hit, Die)
- Скорость анимации
- Состояние воспроизведения

### Материалы
- Количество мешей
- Материалы каждого меша
- Состояние включения мешей

### Очередь действий
- Количество действий в очереди
- Состояние выполнения действий

### ViewModel
- Тип ViewModel
- Команды материалы
- Hovered материалы

### Disposables
- Количество подписок UniRx

## Настройка

### UnitView3DDebugger
```csharp
[Header("Debug Settings")]
[SerializeField] private bool enableDebugger = true;
[SerializeField] private bool showOnScreen = true;
[SerializeField] private bool logToConsole = true;
[SerializeField] private KeyCode toggleKey = KeyCode.F1;
```

### UnitView3DComponentDebugger
```csharp
[Header("Debug Settings")]
[SerializeField] private bool enableDebugger = true;
[SerializeField] private bool logEvents = true;
[SerializeField] private bool logAnimations = true;
[SerializeField] private bool logMovement = true;
[SerializeField] private bool logMaterialChanges = true;
[SerializeField] private bool showGizmos = true;
```

## Публичные методы

### UnitView3DDebugger
```csharp
public void ShowDebugPanel()
public void HideDebugPanel()
public void SetTargetUnitView(UnitView3D unitView)
public void LogDebugInfo()
```

### UnitView3DComponentDebugger
```csharp
public void LogCustomEvent(string eventName, object data = null)
public void LogAnimationEvent(string animationName)
public void LogMovementEvent(Vector3 from, Vector3 to)
public void LogMaterialChangeEvent(int meshIndex, string oldMaterial, string newMaterial)
public Dictionary<string, object> GetDebugStats()
public List<string> GetEventHistory()
public void ClearEventHistory()
public void ResetStats()
```

## Интеграция с существующим кодом

Система дебаггеров теперь **автоматически интегрирована** с классом `UnitView3D`. Все события (атака, получение урона, смерть, движение, hover) автоматически логируются в дебаггеры.

### Автоматически отслеживаемые события:

1. **Атака** - при вызове `vm.OnAttacked`
2. **Получение урона** - при вызове `vm.OnHit`
3. **Смерть** - при вызове `vm.OnDeath`
4. **Начало хода** - при вызове `vm.OnTurnStarted`
5. **Движение** - при вызове `MoveByRoute()`
6. **Изменение материалов** - при вызове `SetMaterial()`
7. **Hover события** - при вызове `Hover()` и `UnHover()`
8. **Очередь действий** - при добавлении и выполнении действий

### Тестирование событий

Для тестирования дебаггеров добавьте компонент `UnitView3DEventTester` на объект с `UnitView3D`:

```csharp
// Клавиши для тестирования:
// 1 - Тест атаки
// 2 - Тест получения урона  
// 3 - Тест смерти
// 4 - Тест движения
// 5 - Тест hover
```

### Ручное логирование событий

Если нужно добавить собственные события, используйте метод `LogDebugEvent()`:

```csharp
// В любом методе UnitView3D
private void SomeMethod()
{
    // Ваш код...
    
    // Логирование события
    LogDebugEvent("Custom Event Description");
}
```

## Рекомендации по использованию

1. **Для быстрой отладки:** Используйте `UnitView3DDebugger` - добавьте на пустой GameObject в сцене
2. **Для детального анализа:** Используйте `UnitView3DComponentDebugger` - добавьте на объект с юнитом
3. **Для разработки:** Используйте `UnitView3DEditorDebugger` - работает автоматически в Editor

## Производительность

- Все дебаггеры можно отключить через параметр `enableDebugger`
- История событий ограничена (50-100 записей)
- Рефлексия используется только при необходимости
- UI создается только при включенном дебаге

## Устранение неполадок

### UnitView3D не найден
- Убедитесь, что в сцене есть объект с компонентом `UnitView3D`
- Проверьте, что дебаггер включен

### UI не отображается
- Проверьте настройки Canvas
- Убедитесь, что `showOnScreen = true`
- Попробуйте нажать F1 для переключения

### Нет информации в Editor
- Убедитесь, что игра запущена (Play Mode)
- Проверьте, что выбран правильный объект в иерархии

## Расширение функциональности

Для добавления новых параметров для отслеживания:

1. В `UpdateDebugInfo()` добавьте новые поля
2. В соответствующих методах отрисовки добавьте отображение
3. При необходимости добавьте новые события

```csharp
// Пример добавления нового параметра
debugInfo["New Parameter"] = someValue;
``` 