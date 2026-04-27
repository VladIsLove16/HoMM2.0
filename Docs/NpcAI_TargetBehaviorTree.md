# NPC AI Target Behavior Tree

Этот документ описывает целевую версию дерева поведения NPC. Оно сохраняет текущее поведение прототипа, но раскладывает его по более читаемому паттерну:

```text
Perception -> Desires -> Intent -> Execution
```

Файл не является Unity Behavior Graph asset. Это проектная схема, по которой можно постепенно перестроить текущий `Behavior Graph.asset`.

## Цели

- Сохранить текущее поведение NPC: патрулирование, скука, подход к игроку, диалог, реакция на выбор игрока, реакция на результат боя, поиск и сбор гриба.
- Убрать смешивание проверок расстояния, накопления желаний, выбора действия и исполнения действия в одной большой ветке.
- Сделать дерево объяснимым для диплома, code review и показа работодателю.
- Оставить Behavior Tree как слой исполнения поведения, а вычисление намерения сделать отдельным читаемым этапом.

## Blackboard

### Facts / Perception

```text
PlayerInTalkingDistance : bool
CanSeeMushroom : bool
LastSeenMushroom : Transform
ReachedMushroom : bool
ReachedNavigatingTarget : bool
```

### Desires

```text
MushroomDesire : float
TalkDesire : float
Boredom : float
```

Текущие аналоги:

```text
WannaGrabTime -> MushroomDesire
BoredomTime -> Boredom
```

### Limits / Tuning

```text
TalkingDistance : float
GrabbingDistance : float
MushroomDesireThreshold : float
DialogInterruptThreshold : float
BoredomLimit : float
DialogInterruptChance : float
```

Текущие аналоги:

```text
WannaGrabLimit -> MushroomDesireThreshold
InteruptionDialogChance -> DialogInterruptChance
```

### State

```text
State : NPCState
CurrentIntent : NPCIntent
Last chosen dialog option : DialogueChoiceAction
Last BattleOutcome : BattleOutcome
DialogInterruptCount : int
ReactedToPlayerApproach : bool
```

Текущие аналоги:

```text
Times NPC interupted dialog -> DialogInterruptCount
Reacted to reaching talking distance -> ReactedToPlayerApproach
```

Предлагаемый enum:

```csharp
public enum NPCIntent
{
    None,
    Patrol,
    Talk,
    CollectMushroom,
    Fight
}
```

`State` должен описывать текущую фазу выполнения, а `CurrentIntent` - выбранное намерение.

Пример:

```text
CurrentIntent = CollectMushroom
State = NavigatingToGrabItem
```

## Root

```text
ROOT: Parallel

    EventHandlers
    MainLoop
```

Событийные ветки оставляем отдельно, потому что они приходят из C# или из других сцен:

```text
EventHandlers:
    OnPlayerDialogStateChanged
    OnPlayerChosenDialogOption
    OnBattleFinished
```

Основное поведение NPC находится в одном повторяемом цикле:

```text
MainLoop:
    Repeat
        Sequence
            UpdatePerception
            UpdateDesires
            SelectIntent
            ExecuteIntent
```

## Event Handlers

### OnPlayerDialogStateChanged

Текущий event:

```text
New Player  talking with npc
```

Целевая логика:

```text
OnEvent New Player talking with npc:
    if dialog is opened for this NPC:
        State = Talking
        CurrentIntent = Talk
        Boredom = 0
```

Примечание:

Если один и тот же event отправляется и при открытии, и при закрытии диалога, дерево должно различать это через blackboard-состояние или C# должен отправлять отдельные события. Иначе возможен конфликт: закрытие диалога может снова поставить `State = Talking`.

### OnPlayerChosenDialogOption

Текущий event:

```text
New Player chosen dialog option
```

Целевая логика:

```text
OnEvent New Player chosen dialog option -> Last chosen dialog option:
    if Last chosen dialog option == StartBattle:
        CurrentIntent = Fight
        State = Fighting
        play animation BattleStart

    else if Last chosen dialog option == EndDialogue:
        CurrentIntent = None
        State = None

    else:
        CurrentIntent = Talk
        State = Talking
        play animation Talking
```

В текущем дереве ветка `OnEvent New Player chosen dialog option` без child является лишней и должна быть удалена.

### OnBattleFinished

Текущий event:

```text
BattleFinishedChannel -> Last BattleOutcome
```

Целевая логика:

```text
OnEvent BattleFinishedChannel -> Last BattleOutcome:
    if Last BattleOutcome == PlayerWon:
        play animation BattleLost
        State = Sad
    else:
        play animation BattleWon
        State = Happy
```

Сейчас в дереве есть только выбор анимации. Состояние лучше тоже выставлять явно.

## Main Loop

### UpdatePerception

Отвечает только за факты о мире. Здесь не выбирается действие.

```text
UpdatePerception:
    PlayerInTalkingDistance =
        distance(Player Movement Controller, Self) < TalkingDistance

    CanSeeMushroom =
        IsSeeingMushroom(Self, mushroomLayer)

    if CanSeeMushroom:
        LastSeenMushroom = seen mushroom

    ReachedMushroom =
        LastSeenMushroom != null
        and distance(Self, LastSeenMushroom) < GrabbingDistance

    ReachedNavigatingTarget =
        NavigatingTarget != null
        and distance(Self, NavigatingTarget) < StopDistance
```

Текущий аналог:

```text
IsSeeingMushroomCondition
CheckDistanceCondition(Player, Self)
CheckDistanceCondition(Self, LastSeenMushroom)
CheckDistanceCondition(Self, NavigatingTarget)
```

### UpdateDesires

Отвечает только за накопление и спад мотиваций.

```text
UpdateDesires:
    if CanSeeMushroom:
        MushroomDesire += MushroomDesireGain * deltaTime
    else:
        MushroomDesire -= MushroomDesireDecay * deltaTime

    if PlayerInTalkingDistance:
        TalkDesire += TalkDesireGain * deltaTime
    else:
        TalkDesire -= TalkDesireDecay * deltaTime

    if State != Talking and State != Grabbing and State != Fighting:
        Boredom += PatrolBoredAmount * deltaTime

    MushroomDesire = clamp(MushroomDesire, 0, 100)
    TalkDesire = clamp(TalkDesire, 0, 100)
    Boredom = clamp(Boredom, 0, 100)
```

Текущий аналог:

```text
WannaGrabTime += PatrolingWannaGrabAmount * deltaTime
BoredomTime += PatrolingBoredAmount * deltaTime
```

Важное отличие:

В целевой версии `MushroomDesire` должен уменьшаться, если NPC перестал видеть гриб. Иначе NPC будет прерывать диалог из-за гриба, который он видел давно.

### SelectIntent

Отвечает только за выбор намерения.

```text
SelectIntent:
    if State == Grabbing:
        CurrentIntent = CollectMushroom

    else if State == Fighting:
        CurrentIntent = Fight

    else if State == Talking:
        if CanInterruptDialog
           and CanSeeMushroom
           and MushroomDesire >= DialogInterruptThreshold
           and DialogInterruptCount == 0
           and random(0, 100) < DialogInterruptChance:
               CurrentIntent = CollectMushroom
        else:
               CurrentIntent = Talk

    else if CanSeeMushroom
        and MushroomDesire >= MushroomDesireThreshold:
            CurrentIntent = CollectMushroom

    else if PlayerInTalkingDistance
        and ReactedToPlayerApproach == false:
            CurrentIntent = Talk

    else if Boredom >= BoredomLimit:
        CurrentIntent = Talk

    else:
        CurrentIntent = Patrol
```

Текущий аналог:

```text
if State == Talking and WannaGrabTime > WannaGrabLimit:
    random < InteruptionDialogChance
    StopTalking
    State = NavigatingToGrabItem

if WannaGrabTime > WannaGrabLimit:
    State = NavigatingToGrabItem

if BoredomTime >= BoredomLimit:
    State = NavigatingToTalk
```

### ExecuteIntent

Исполняет выбранное намерение. Здесь не должны накапливаться желания и не должен приниматься основной выбор поведения.

```text
ExecuteIntent:
    if CurrentIntent == CollectMushroom:
        ExecuteCollectMushroom

    else if CurrentIntent == Talk:
        ExecuteTalk

    else if CurrentIntent == Fight:
        ExecuteFight

    else:
        ExecutePatrol
```

## Execution Subtrees

### ExecuteCollectMushroom

```text
ExecuteCollectMushroom:
    if State == Grabbing:
        wait AnimationEndedGrabbing
        State = None
        CurrentIntent = None
        MushroomDesire = 0

    else if LastSeenMushroom == null:
        CurrentIntent = Patrol

    else:
        State = NavigatingToGrabItem
        navigate Self -> LastSeenMushroom

        if ReachedMushroom:
            trigger grab animation
            State = Grabbing
```

Текущий аналог:

```text
State == NavigatingToGrabItem:
    navigate Self -> LastSeenMushroom
    if distance < GrabbingDistance:
        trigger animation
        State = Grabbing

State == Grabbing:
    wait AnimationEndedGrabbing
    State = None
```

### ExecuteTalk

```text
ExecuteTalk:
    if State == Talking:
        look at Player

    else if PlayerInTalkingDistance:
        if ReactedToPlayerApproach == false:
            activate dialog
            State = Talking
            CurrentIntent = Talk
            Boredom = 0
        else:
            look at Player

    else:
        State = NavigatingToTalk
        navigate Self -> Player
```

Текущий аналог:

```text
if distance(Player, Self) < TalkingDistance:
    if State == NavigatingToTalk:
        activate dialog
        State = Talking
        BoredomTime = 0
    else:
        look at Player
else if BoredomTime >= BoredomLimit:
    State = NavigatingToTalk
    navigate Self -> Player
```

### ExecuteDialogInterrupt

Это частный случай `ExecuteCollectMushroom`, когда NPC был в диалоге.

```text
ExecuteDialogInterrupt:
    if State == Talking
       and CurrentIntent == CollectMushroom:
           StopTalking
           play animation Bye
           State = NavigatingToGrabItem
           DialogInterruptCount += 1
```

В текущем дереве это находится внутри ветки:

```text
State == Talking
and WannaGrabTime > WannaGrabLimit
and Times NPC interupted dialog == 0
and random < InteruptionDialogChance
```

### ExecutePatrol

```text
ExecutePatrol:
    if NavMeshPatroling == true:
        if NavigatingTarget == Self or NavigatingTarget == null:
            random = random(0, 100)

            if random < 50:
                NavigatingTarget = PatrolPointA
            else:
                NavigatingTarget = PatrolPointB

        else:
            navigate NavMeshAgent -> NavigatingTarget

            if ReachedNavigatingTarget:
                NavigatingTarget = random from AvailableNavigatingTargets

    else:
        NpcPatrolAction(A, B)
```

Текущий аналог:

```text
if NavMeshPatroling == true:
    if NavigatingTarget == Self:
        choose A/B
    else:
        navigate to NavigatingTarget
        if reached:
            choose random target
else:
    NpcPatrolAction
```

### ExecuteFight

```text
ExecuteFight:
    play animation BattleStart
    wait battle result from C#
```

Сейчас запуск боя в основном идёт через выбор диалога и C# `DialogVM`, а BT только реагирует анимацией.

## Suggested Unity Behavior Graph Layout

В Unity Behavior Graph это лучше разложить визуально так:

```text
ParallelAll

    EventHandlers
        OnPlayerDialogStateChanged
        OnPlayerChosenDialogOption
        OnBattleFinished

    MainLoop
        Repeat
            Sequence
                UpdatePerception
                UpdateDesires
                SelectIntent
                ExecuteIntent
```

Внутри `ExecuteIntent`:

```text
Selector / Branching
    if CurrentIntent == CollectMushroom:
        ExecuteCollectMushroom

    else if CurrentIntent == Talk:
        ExecuteTalk

    else if CurrentIntent == Fight:
        ExecuteFight

    else:
        ExecutePatrol
```

## Minimal Refactor Plan

Чтобы не переписывать всё сразу:

1. Переименовать переменные в blackboard:
   - `WannaGrabTime` -> `MushroomDesire`
   - `WannaGrabLimit` -> `MushroomDesireThreshold`
   - `BoredomTime` -> `Boredom`
   - `InteruptionDialogChance` -> `DialogInterruptChance`
   - `Times NPC interupted dialog` -> `DialogInterruptCount`
   - `PantrolPointA` -> `PatrolPointA`

2. Добавить `NPCIntent` и `CurrentIntent`.

3. Вынести выбор поведения в один блок `SelectIntent`.

4. Оставить текущие execution-действия:
   - `NpcPatrolAction`
   - `NavigateToTarget`
   - `ActivateDialogWindowAction`
   - `StopTalkingAction`
   - `NpcPlayAnimationAction`

5. Удалить no-op ветку:
   - `OnEvent New Player chosen dialog option` без child.

6. Разделить событие открытия и закрытия диалога или добавить явный флаг, чтобы event закрытия не ставил `State = Talking`.

## Notes For Diploma / Portfolio

В дипломе или портфолио не стоит показывать полный raw Behavior Graph. Вместо этого лучше показывать эту архитектурную схему:

```text
Perception -> Desires -> Intent -> Execution
```

И пояснить:

```text
NPC сначала обновляет факты о мире, затем накапливает мотивации,
после этого выбирает намерение и только затем выполняет выбранное поведение
через Behavior Tree.
```

Это показывает не только наличие NPC AI, но и понимание того, как его масштабировать.
