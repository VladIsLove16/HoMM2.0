# Устранение неполадок в тестах

Этот документ содержит решения для распространенных проблем, которые могут возникнуть при запуске тестов.

## Проблемы компиляции

### Ошибка: "The type or namespace name 'StatusEffect' could not be found"
**Решение:** Убедитесь, что файл `StatusEffect.cs` создан в правильной директории и содержит корректный код.

### Ошибка: "The type or namespace name 'StatusEffectType' could not be found"
**Решение:** Проверьте, что `StatusEffectType.cs` содержит enum, а не class.

### Ошибка: "The type or namespace name 'UnitViewUI' could not be found"
**Решение:** Убедитесь, что класс `UnitViewUI` доступен и наследуется от `MonoBehaviour`.

## Проблемы выполнения тестов

### Тест падает с ошибкой "NullReferenceException"
**Возможные причины:**
1. Mock объекты не инициализированы правильно
2. Зависимости не инжектированы
3. Unity объекты не созданы

**Решение:**
```csharp
[SetUp]
public void SetUp()
{
    // Убедитесь, что все объекты созданы
    _baseStats = ScriptableObject.CreateInstance<UnitStats>();
    // ... остальная инициализация
}
```

### Тест падает с ошибкой "ArgumentException"
**Возможные причины:**
1. Переданы некорректные параметры
2. Логика валидации в коде не соответствует ожиданиям теста

**Решение:** Проверьте логику валидации в тестируемом коде и обновите тест соответственно.

### Тест производительности падает
**Возможные причины:**
1. Слишком строгие ограничения по времени
2. Разные характеристики системы

**Решение:** Настройте пороговые значения в соответствии с вашей системой:
```csharp
Assert.That(averageTime, Is.LessThan(5.0), // Увеличьте порог если нужно
    $"Операция занимает слишком много времени: {averageTime:F3}ms в среднем");
```

## Проблемы с Mock объектами

### Mock не работает как ожидается
**Решение:** Убедитесь, что Mock классы правильно реализуют интерфейсы:
```csharp
private class MockDamagable : IDamagable
{
    public bool IsBlueTeam => true;
    public Vector2Int Position { get; set; }
    public GridContentType GridContentType => GridContentType.unit;

    public void RecieveDamage(DamageContext ctx) { }
    public void SimulateRecieveDamage(DamageContext ctx) { }
}
```

### Проблемы с Unity объектами в тестах
**Решение:** Используйте `ScriptableObject.CreateInstance<T>()` для создания объектов в EditMode:
```csharp
_baseStats = ScriptableObject.CreateInstance<UnitStats>();
```

## Проблемы с зависимостями

### Zenject зависимости не инжектируются
**Решение:** Для тестов используйте reflection или создавайте зависимости вручную:
```csharp
// Инжектируем зависимости через reflection (для тестов)
var field = typeof(UnitViewModel).GetField("_worldToCellProvider", 
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
field?.SetValue(_unitViewModel, _worldToCellProvider);
```

### UniRx не работает
**Решение:** Убедитесь, что UniRx подключен к проекту и добавлен в Assembly Definition.

## Проблемы с PlayMode тестами

### Тест зависает
**Возможные причины:**
1. Бесконечный цикл в коде
2. События не срабатывают

**Решение:** Добавьте таймауты и проверки:
```csharp
yield return new WaitForSeconds(0.1f); // Ждем обработки событий
```

### GameObject не уничтожается
**Решение:** Используйте `UnityEngine.Object.DestroyImmediate()` в TearDown:
```csharp
[TearDown]
public IEnumerator TearDown()
{
    if (_gameObject != null)
    {
        UnityEngine.Object.DestroyImmediate(_gameObject);
    }
    yield return null;
}
```

## Общие рекомендации

### Структура тестов
1. **SetUp** - создание объектов
2. **Тест** - выполнение тестируемой логики
3. **TearDown** - очистка ресурсов

### Именование тестов
Используйте паттерн: `ClassName_MethodName_ExpectedBehavior`
```csharp
[Test]
public void UnitModel_Constructor_WithValidParameters_InitializesCorrectly()
```

### Изоляция тестов
Каждый тест должен быть независимым:
```csharp
[SetUp]
public void SetUp()
{
    // Создаем новый экземпляр для каждого теста
    _unitModel = new UnitModel(_baseStats, UnitType.Archer, 5, 3, 10, true);
}
```

### Обработка исключений
Тестируйте исключения правильно:
```csharp
[Test]
public void Method_WithInvalidInput_ThrowsException()
{
    // Act & Assert
    Assert.Throws<ArgumentException>(() => _unitModel.SomeMethod(invalidInput));
}
```

## Отладка тестов

### Логирование
Добавьте логи для отладки:
```csharp
Debug.Log($"Тест выполняется с параметрами: {parameter}");
```

### Пошаговое выполнение
Используйте точки останова в Unity Test Runner для пошаговой отладки.

### Проверка состояния
Добавьте промежуточные проверки:
```csharp
// Проверяем промежуточное состояние
Assert.That(_unitModel.Amount.Value, Is.EqualTo(expectedAmount));
```

## Полезные ссылки

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/index.html)
- [NUnit Documentation](https://docs.nunit.org/)
- [UniRx Documentation](https://github.com/neuecc/UniRx)

