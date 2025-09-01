# Тесты поведения смерти юнита (UnitDeathBehavior Tests)

## 🎯 **Цель тестов**

Эти тесты проверяют **правильное поведение GameObject'а при атаке и смерти юнита**. Основная проблема была в том, что GameObject деактивировался при каждой атаке, а не только при полной смерти.

## 🚨 **Проблема, которую решают тесты**

### **Что происходило раньше:**
1. **Атака на юнита** → вызывалось событие `OnDeath`
2. **UnitView3D.HandleDeath()** → GameObject деактивировался
3. **Результат:** Юнит исчезал с поля даже при незначительном уроне

### **Что должно происходить:**
1. **Атака на юнита** → GameObject остается активным, играется анимация Hit
2. **Полная смерть юнита** → GameObject деактивируется, играется анимация Death

## 📋 **Список тестов**

### **1. EditMode тесты (быстрые)**

#### **UnitModelTests.cs:**
- `RecieveDamage_WithDamageLessThanHealth_DoesNotTriggerDeath` - урон меньше здоровья не вызывает смерть
- `RecieveDamage_WithDamageEqualToHealth_TriggersDeath` - урон равный здоровью вызывает смерть
- `RecieveDamage_WithPartialStackDeath_DoesNotTriggerDeath` - частичная смерть стэка не вызывает смерть
- `RecieveDamage_WithExactHealthKill_TriggersDeath` - точное убийство вызывает смерть
- `RecieveDamage_WithOverkill_TriggersDeath` - избыточный урон вызывает смерть

#### **UnitViewModelTests.cs:**
- `OnDeath_WhenModelPartiallyDamaged_DoesNotEmitEvent` - OnDeath не вызывается при частичном уроне
- `OnDeath_WhenModelFullyKilled_EmitsEvent` - OnDeath вызывается при полной смерти
- `OnHit_WhenModelDamaged_AlwaysEmitsEvent` - OnHit всегда вызывается при уроне
- `EventSequence_WhenModelDamaged_EmitsInCorrectOrder` - правильный порядок событий

### **2. PlayMode тесты (медленные, с GameObject'ами)**

#### **IntegrationTests.cs:**
- `UnitView3D_Attack_DoesNotDeactivateGameObject` - атака не деактивирует GameObject
- `UnitView3D_Death_DeactivatesGameObject` - смерть деактивирует GameObject
- `UnitView3D_PartialDamage_HandlesCorrectly` - частичный урон обрабатывается правильно
- `UnitView3D_Events_TriggerCorrectly` - события вызываются корректно
- `UnitView3D_Animations_PlayCorrectly` - анимации проигрываются правильно
- `UnitView3D_DeathAnimation_PlaysBeforeDeactivation` - анимация смерти играется перед деактивацией

#### **UnitDeathBehaviorTests.cs (специальный файл):**
- `UnitView3D_NonLethalDamage_KeepsGameObjectActive` - нелетальный урон сохраняет GameObject активным
- `UnitView3D_PartialStackDeath_KeepsGameObjectActive` - частичная смерть стэка сохраняет GameObject активным
- `UnitView3D_FullDeath_DeactivatesGameObject` - полная смерть деактивирует GameObject
- `UnitView3D_DeathAnimation_PlaysBeforeDeactivation` - анимация смерти играется перед деактивацией
- `UnitView3D_EventHandling_WorksCorrectly` - обработка событий работает корректно
- `UnitView3D_MultipleAttacks_HandlesCorrectly` - множественные атаки обрабатываются правильно

## 🔍 **Что проверяют тесты**

### **Логика UnitModel:**
- ✅ Событие `OnDeath` вызывается только при `Amount.Value <= 0`
- ✅ Частичная смерть стэка не вызывает полную смерть
- ✅ Правильный расчет урона и здоровья

### **Логика UnitViewModel:**
- ✅ События вызываются в правильном порядке
- ✅ `OnHit` всегда вызывается при уроне
- ✅ `OnDeath` вызывается только при полной смерти

### **Логика UnitView3D:**
- ✅ GameObject остается активным при атаке
- ✅ GameObject деактивируется только при полной смерти
- ✅ Анимации проигрываются корректно
- ✅ События обрабатываются правильно

## 🎮 **Сценарии тестирования**

### **Сценарий 1: Незначительная атака**
- **Урон:** 50 (здоровье: 100)
- **Ожидаемый результат:** GameObject остается активным, играется анимация Hit
- **События:** OnHealthChanged, OnHit

### **Сценарий 2: Частичная смерть стэка**
- **Урон:** 150 (здоровье: 100, количество: 3)
- **Ожидаемый результат:** GameObject остается активным, количество уменьшается до 2
- **События:** OnHealthChanged, OnHit

### **Сценарий 3: Полная смерть**
- **Урон:** 1000 (здоровье: 100, количество: 3)
- **Ожидаемый результат:** GameObject деактивируется, играется анимация Death
- **События:** OnHealthChanged, OnHit, OnDeath

## 🚀 **Как запустить тесты**

### **EditMode тесты (быстро):**
1. Откройте Unity Test Runner
2. Выберите вкладку "EditMode"
3. Запустите тесты в папке "GridContents/Units"

### **PlayMode тесты (медленно):**
1. Откройте Unity Test Runner
2. Выберите вкладку "PlayMode"
3. Запустите тесты в папке "IntegrationTests" и "UnitDeathBehaviorTests"

## 📊 **Ожидаемые результаты**

После исправления кода:
- ✅ **Все EditMode тесты** должны проходить
- ✅ **Все PlayMode тесты** должны проходить
- ✅ **GameObject не деактивируется** при атаке
- ✅ **GameObject деактивируется** только при полной смерти
- ✅ **Анимации проигрываются** корректно
- ✅ **События вызываются** в правильном порядке

## 🔧 **Технические детали**

### **Используемые компоненты:**
- `UnitModel` - логика юнита
- `UnitViewModel` - связующее звено между моделью и представлением
- `UnitView3D` - визуальное представление юнита
- `Animator` - анимации
- `Zenject` - dependency injection для тестов

### **Ключевые проверки:**
- `gameObject.activeSelf` - активность GameObject'а
- `Amount.Value` - количество юнитов в стэке
- `ModifiedStats.Health` - текущее здоровье
- События `OnDeath`, `OnHit`, `OnHealthChanged`
- Состояния аниматора

Эти тесты обеспечивают надежную проверку того, что проблема с преждевременной деактивацией GameObject'а решена и не повторится в будущем! 🎯
