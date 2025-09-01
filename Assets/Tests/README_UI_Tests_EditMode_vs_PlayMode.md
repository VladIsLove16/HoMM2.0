# UI тесты: EditMode vs PlayMode - Правильное разделение

## Миф: "Все UI тесты должны быть в PlayMode"

**НЕТ!** UI тесты могут и должны быть в ОБОИХ режимах в зависимости от того, что они тестируют.

## Правильное разделение UI тестов

### EditMode UI тесты (быстрые, без рендеринга)

**Что тестировать:**
- ✅ Логику UI компонентов
- ✅ Расчеты и вычисления
- ✅ События и их обработку
- ✅ Валидацию данных
- ✅ Бизнес-логику UI

**Примеры:**
```csharp
// EditMode - тестируем только логику
[Test]
public void UnitHealthBar_SetRatio_CalculatesCorrectly()
{
    // Arrange
    float ratio = 0.75f;
    
    // Act - тестируем логику без создания GameObject'ов
    float expectedFillAmount = ratio;
    
    // Assert
    Assert.That(expectedFillAmount, Is.EqualTo(0.75f));
}

[Test]
public void UnitViewUI_UpdateHealth_CalculatesCorrectRatio()
{
    // Arrange
    int currentHealth = 75;
    int maxHealth = 100;
    
    // Act
    float ratio = (float)currentHealth / maxHealth;
    
    // Assert
    Assert.That(ratio, Is.EqualTo(0.75f));
}
```

### PlayMode UI тесты (медленные, с рендерингом)

**Что тестировать:**
- ✅ Визуальное отображение
- ✅ Интеграцию с Unity компонентами
- ✅ Анимации и переходы
- ✅ Взаимодействие с Canvas
- ✅ Реальные GameObject'ы

**Примеры:**
```csharp
// PlayMode - тестируем визуальное отображение
[UnityTest]
public IEnumerator UnitHealthBar_VisualUpdate_WorksCorrectly()
{
    // Arrange
    var canvas = new GameObject().AddComponent<Canvas>();
    var healthBar = canvas.gameObject.AddComponent<UnitHealthBar>();
    var image = canvas.gameObject.AddComponent<Image>();
    
    // Act
    healthBar.SetRatio(0.5f);
    yield return new WaitForEndOfFrame();
    
    // Assert - проверяем реальное визуальное отображение
    Assert.That(image.fillAmount, Is.EqualTo(0.5f));
}
```

## Структура файлов

```
Assets/Tests/
├── EditMode/
│   └── UI/
│       ├── UnitHealthBarTests_EditMode.cs     ← Логика без рендеринга
│       └── UnitViewUITests_EditMode.cs        ← Логика без рендеринга
└── PlayMode/
    └── UI/
        ├── UnitHealthBarTests_PlayMode.cs     ← Визуальное отображение
        └── UnitHealthBarIntegrationTests.cs   ← Полная интеграция
```

## Ключевые различия

### EditMode UI тесты:
- ✅ Используют `[Test]`, `[SetUp]`, `[TearDown]`
- ✅ НЕ создают GameObject'ы
- ✅ НЕ используют Canvas, Image, Text
- ✅ Тестируют только логику и вычисления
- ✅ Выполняются быстро в редакторе

### PlayMode UI тесты:
- ✅ Используют `[UnityTest]`, `[UnitySetUp]`, `[UnityTearDown]`
- ✅ Создают реальные GameObject'ы
- ✅ Используют Canvas, Image, Text, Transform
- ✅ Тестируют визуальное отображение
- ✅ Выполняются медленно в игровом режиме

## Примеры правильного разделения

### UnitHealthBar

**EditMode тесты:**
- Расчет соотношения здоровья
- Логика скрытия/показа при полном здоровье
- Валидация входных параметров

**PlayMode тесты:**
- Визуальное обновление fillAmount
- Отображение/скрытие Image компонента
- Интеграция с Canvas

### UnitViewUI

**EditMode тесты:**
- Расчет соотношения здоровья
- Обработка событий OnHealthChanged
- Логика обновления количества юнитов

**PlayMode тесты:**
- Визуальное обновление Text компонента
- Изменение цвета при смерти
- Интеграция с UnitHealthBar

## Преимущества такого разделения

1. **Скорость:** EditMode тесты выполняются мгновенно
2. **Надежность:** PlayMode тесты проверяют реальное отображение
3. **Покрытие:** Тестируем и логику, и визуализацию
4. **Отладка:** Легче найти проблемы в логике vs визуализации

## Рекомендации

1. **Начните с EditMode** - тестируйте логику без рендеринга
2. **Добавьте PlayMode** - для критически важной визуализации
3. **Используйте префиксы** в названиях: `_EditMode`, `_PlayMode`
4. **Разделяйте ответственность** - логика vs визуализация

## Заключение

UI тесты должны быть в **ОБОИХ** режимах:
- **EditMode** для быстрого тестирования логики
- **PlayMode** для проверки реального отображения

Это обеспечивает максимальное покрытие и эффективность тестирования.
