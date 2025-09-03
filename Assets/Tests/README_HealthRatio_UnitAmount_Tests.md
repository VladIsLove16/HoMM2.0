# Тесты изменения Ratio полоски здоровья и количества юнитов

## 🎯 **Цель тестов**

Эти тесты проверяют **правильное обновление UI при получении урона**:
- ✅ **Ratio полоски здоровья** обновляется корректно
- ✅ **Количество юнитов** отображается правильно
- ✅ **Интеграция** между UnitModel, UnitViewModel и UI работает

## 📋 **Список тестов**

### **1. EditMode тесты (быстрые)**

#### **UnitModelTests.cs - Логика урона:**
- `RecieveDamage_WithPartialDamage_UpdatesHealthCorrectly` - частичный урон обновляет здоровье
- `RecieveDamage_WithExactHealthDamage_KillsOneUnit` - точный урон убивает одного юнита
- `RecieveDamage_WithOverkillDamage_KillsMultipleUnits` - избыточный урон убивает несколько юнитов
- `RecieveDamage_WithMassiveDamage_KillsAllUnits` - массивный урон убивает всех
- `RecieveDamage_WithZeroDamage_DoesNothing` - нулевой урон ничего не делает
- `RecieveDamage_WithNegativeDamage_ThrowsException` - отрицательный урон вызывает исключение
- `RecieveDamage_WithStatusEffects_AppliesCorrectly` - статус-эффекты применяются корректно
- `RecieveDamage_WithMultipleSmallDamages_AccumulatesCorrectly` - множественные уроны накапливаются

#### **UnitViewModelTests.cs - События и расчеты:**
- `HealthRatio_WhenModelDamaged_CalculatesCorrectly` - Ratio рассчитывается правильно
- `HealthRatio_AfterPartialDamage_CalculatesCorrectly` - Ratio после частичного урона
- `HealthRatio_AfterUnitDeath_CalculatesCorrectly` - Ratio после смерти юнита
- `HealthRatio_WithMultipleUnitDeaths_CalculatesCorrectly` - Ratio при множественных смертях
- `HealthRatio_WithZeroMaxHealth_HandlesCorrectly` - Ratio при нулевом максимальном здоровье
- `UnitAmount_WhenModelDamaged_UpdatesCorrectly` - количество юнитов обновляется
- `UnitAmount_WithMultipleDeaths_UpdatesCorrectly` - количество при множественных смертях
- `HealthRatio_WithStatusEffects_CalculatesCorrectly` - Ratio со статус-эффектами
- `HealthRatio_WithHealing_CalculatesCorrectly` - Ratio при лечении

### **2. PlayMode тесты (медленные, с GameObject'ами)**

#### **IntegrationTests.cs - Интеграция:**
- `UnitViewUI_HealthRatio_UpdatesCorrectlyAfterDamage` - Ratio обновляется после урона
- `UnitViewUI_UnitAmount_UpdatesCorrectlyAfterDamage` - количество обновляется после урона
- `UnitViewUI_HealthRatio_WithMultipleUnitDeaths_UpdatesCorrectly` - Ratio при множественных смертях
- `UnitViewUI_HealthRatio_WithPartialDamage_UpdatesCorrectly` - Ratio при частичном уроне
- `UnitViewUI_HealthRatio_WithExactKill_UpdatesCorrectly` - Ratio при точном убийстве
- `UnitViewUI_HealthRatio_WithOverkill_UpdatesCorrectly` - Ratio при избыточном уроне

#### **UI_HealthRatio_Update_Tests.cs (специальный файл):**
- `HealthBar_InitialState_ShowsFullHealth` - полоска здоровья показывает полное здоровье
- `HealthBar_AfterDamage_UpdatesCorrectly` - полоска здоровья обновляется после урона
- `HealthBar_AfterUnitDeath_ResetsToFullHealth` - полоска здоровья сбрасывается после смерти
- `AmountText_InitialState_ShowsCorrectAmount` - текст количества показывает правильное количество
- `AmountText_AfterUnitDeath_UpdatesCorrectly` - текст количества обновляется после смерти
- `HealthBar_WithMultipleUnitDeaths_UpdatesCorrectly` - полоска здоровья при множественных смертях
- `UI_WithCompleteDestruction_UpdatesCorrectly` - UI при полном уничтожении
- `UI_WithPartialDamage_UpdatesCorrectly` - UI при частичном уроне
- `UI_WithExactKill_UpdatesCorrectly` - UI при точном убийстве
- `UI_WithMultipleSmallDamages_UpdatesCorrectly` - UI при множественных небольших уронах

## 🔍 **Что проверяют тесты**

### **Логика UnitModel:**
- ✅ Правильный расчет урона и здоровья
- ✅ Корректное уменьшение количества юнитов
- ✅ Обработка различных типов урона
- ✅ Валидация входных параметров

### **Логика UnitViewModel:**
- ✅ Правильный расчет HealthRatio
- ✅ Корректное обновление событий
- ✅ Обработка статус-эффектов
- ✅ Математические вычисления

### **UI компоненты:**
- ✅ Обновление полоски здоровья (fillAmount)
- ✅ Обновление текста количества
- ✅ Корректное отображение Ratio
- ✅ Интеграция с моделью данных

## 🎮 **Сценарии тестирования**

### **Сценарий 1: Частичный урон**
- **Урон:** 50 (здоровье: 100)
- **Ожидаемый результат:** 
  - Ratio: 0.5 (50%)
  - Количество: не изменилось
  - Полоска здоровья: 50%

### **Сценарий 2: Точное убийство**
- **Урон:** 100 (здоровье: 100)
- **Ожидаемый результат:**
  - Ratio: 1.0 (100%)
  - Количество: уменьшилось на 1
  - Полоска здоровья: 100%

### **Сценарий 3: Избыточный урон**
- **Урон:** 250 (здоровье: 100, количество: 3)
- **Ожидаемый результат:**
  - Ratio: 0.5 (50%)
  - Количество: уменьшилось на 2
  - Полоска здоровья: 50%

### **Сценарий 4: Полное уничтожение**
- **Урон:** 1000 (здоровье: 100, количество: 3)
- **Ожидаемый результат:**
  - Ratio: 0.0 (0%)
  - Количество: 0
  - Полоска здоровья: 0%

## 🚀 **Как запустить тесты**

### **EditMode тесты (быстро):**
1. Откройте Unity Test Runner
2. Выберите вкладку "EditMode"
3. Запустите тесты в папке "GridContents/Units"

### **PlayMode тесты (медленно):**
1. Откройте Unity Test Runner
2. Выберите вкладку "PlayMode"
3. Запустите тесты в папках:
   - "IntegrationTests"
   - "UI_HealthRatio_Update_Tests"

## 📊 **Ожидаемые результаты**

После исправления кода:
- ✅ **Все EditMode тесты** должны проходить
- ✅ **Все PlayMode тесты** должны проходить
- ✅ **Ratio полоски здоровья** обновляется корректно
- ✅ **Количество юнитов** отображается правильно
- ✅ **UI синхронизирован** с моделью данных
- ✅ **Математические вычисления** точны

## 🔧 **Технические детали**

### **Ключевые проверки:**
- `_unitViewModel.HealthRatio` - соотношение здоровья
- `_unitModel.Amount.Value` - количество юнитов
- `_unitModel.ModifiedStats.Health` - текущее здоровье
- `healthImage.fillAmount` - заполнение полоски здоровья
- `_amountText.text` - текст количества

### **Математические формулы:**
- **HealthRatio = CurrentHealth / MaxHealth**
- **Damage Distribution** - распределение урона по юнитам
- **Unit Death Logic** - логика смерти юнитов в стэке

### **Интеграционные моменты:**
- События `OnHealthChanged`, `OnHit`, `OnDeath`
- Обновление UI через ViewModel
- Синхронизация между моделью и представлением

## 📝 **Примеры тестовых данных**

### **Базовые характеристики:**
- Здоровье: 100
- Максимальное здоровье: 100
- Количество юнитов: 3
- Тип: Archer

### **Тестовые уроны:**
- 30 - частичный урон (30%)
- 50 - частичный урон (50%)
- 100 - точное убийство (100%)
- 150 - убийство + частичный урон
- 250 - убийство 2 юнитов + частичный урон
- 1000 - полное уничтожение

Эти тесты обеспечивают полное покрытие логики изменения здоровья и количества юнитов при получении урона! 🎯

