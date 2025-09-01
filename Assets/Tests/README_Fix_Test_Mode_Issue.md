# Исправление проблемы с разделением тестов на EditMode и PlayMode

## Проблема
Все тесты в Unity Test Runner отображаются как PlayMode, хотя должны разделяться на EditMode и PlayMode.

## Причины проблемы

### 1. Неправильные атрибуты в EditMode тестах
- **НЕ используйте** `[UnityTest]`, `[UnitySetUp]`, `[UnityTearDown]` в EditMode тестах
- **НЕ используйте** `using UnityEngine.TestTools;` в EditMode тестах
- **НЕ используйте** `yield return` в EditMode тестах

### 2. Неправильная структура ассембли
- Отсутствуют отдельные ассембли для EditMode и PlayMode
- Общий ассембли тестов может конфликтовать

### 3. Неправильные namespace'ы
- Тесты должны быть в правильных папках: `Tests/EditMode/` и `Tests/PlayMode/`

## Решение

### Шаг 1: Проверьте атрибуты в EditMode тестах

**Правильно для EditMode:**
```csharp
using NUnit.Framework;
using UnityEngine;

namespace Tests.EditMode
{
    [TestFixture]
    public class MyEditModeTest
    {
        [SetUp]
        public void SetUp() { }

        [TearDown]
        public void TearDown() { }

        [Test]
        public void MyTest() { }
    }
}
```

**Неправильно для EditMode:**
```csharp
using UnityEngine.TestTools; // ❌ НЕ используйте

[UnityTest]           // ❌ НЕ используйте
[UnitySetUp]          // ❌ НЕ используйте  
[UnityTearDown]       // ❌ НЕ используйте

public IEnumerator MyTest() // ❌ НЕ используйте
{
    yield return null;      // ❌ НЕ используйте
}
```

### Шаг 2: Проверьте атрибуты в PlayMode тестах

**Правильно для PlayMode:**
```csharp
using UnityEngine.TestTools;
using System.Collections;

namespace Tests.PlayMode
{
    public class MyPlayModeTest
    {
        [UnitySetUp]
        public IEnumerator SetUp() 
        {
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown() 
        {
            yield return null;
        }

        [UnityTest]
        public IEnumerator MyTest() 
        {
            yield return null;
        }
    }
}
```

### Шаг 3: Структура папок

```
Assets/
├── Tests/
│   ├── EditMode/
│   │   ├── EditModeTests.asmdef
│   │   ├── GridContents/
│   │   │   └── Units/
│   │   │       ├── UnitModelTests.cs
│   │   │       ├── UnitViewModelTests.cs
│   │   │       └── ...
│   │   └── UI/
│   │       ├── UnitHealthBarTests.cs
│   │       └── UnitViewUITests.cs
│   └── PlayMode/
│       ├── PlayModeTests.asmdef
│       ├── GridContents/
│       │   └── Units/
│       │       └── IntegrationTests.cs
│       └── UI/
│           └── UnitHealthBarIntegrationTests.cs
```

### Шаг 4: Assembly Definition Files

**EditModeTests.asmdef:**
```json
{
    "name": "EditModeTests",
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

**PlayModeTests.asmdef:**
```json
{
    "name": "PlayModeTests", 
    "includePlatforms": [],
    "excludePlatforms": ["Editor"],
    "defineConstraints": ["UNITY_INCLUDE_TESTS"]
}
```

## Проверка исправления

1. **Перезапустите Unity** после внесения изменений
2. **Откройте Window > General > Test Runner**
3. **Проверьте разделение тестов:**
   - EditMode тесты должны быть в группе "EditMode"
   - PlayMode тесты должны быть в группе "PlayMode"

## Частые ошибки

### ❌ Ошибка: Использование Unity атрибутов в EditMode
```csharp
// ❌ НЕПРАВИЛЬНО
[UnityTest]
public IEnumerator EditModeTest()
{
    yield return null;
}
```

### ❌ Ошибка: Отсутствие Unity атрибутов в PlayMode
```csharp
// ❌ НЕПРАВИЛЬНО
[Test]
public void PlayModeTest() { }
```

### ❌ Ошибка: Смешивание атрибутов
```csharp
// ❌ НЕПРАВИЛЬНО - смешивание EditMode и PlayMode атрибутов
[TestFixture]
public class MixedTest
{
    [SetUp]
    public void SetUp() { }

    [UnityTest]
    public IEnumerator MixedTest() 
    {
        yield return null;
    }
}
```

## После исправления

После правильного разделения тестов:
- **EditMode тесты** будут выполняться быстро в редакторе
- **PlayMode тесты** будут выполняться в игровом режиме
- Unity Test Runner будет правильно группировать тесты
- Тесты будут выполняться в правильном контексте

## Дополнительные рекомендации

1. **Используйте префиксы в названиях тестов:**
   - EditMode: `UnitModel_Constructor_InitializesCorrectly`
   - PlayMode: `UnitModel_Integration_WorksCorrectly`

2. **Разделяйте логику:**
   - EditMode: тестирование логики без игрового контекста
   - PlayMode: тестирование интеграции и игрового контекста

3. **Проверяйте компиляцию:**
   - EditMode тесты должны компилироваться в редакторе
   - PlayMode тесты должны компилироваться в игровом режиме
