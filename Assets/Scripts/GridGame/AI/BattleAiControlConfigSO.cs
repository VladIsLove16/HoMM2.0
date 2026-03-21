using UnityEngine;

[CreateAssetMenu(menuName = "GridGame/Battle AI Control Config", fileName = "BattleAiControlConfig")]
public sealed class BattleAiControlConfigSO : ScriptableObject
{
    [Header("Decision Rules")]
    [SerializeField] private bool preferNearestEnemy = true;
    [SerializeField] private bool tryDirectAttackFirst = true;
    [SerializeField] private bool tryMoveThenAttack = true;
    [SerializeField] private bool tryMoveTowardsEnemy = true;
    [SerializeField] private bool autoEndTurnWhenNoPlan = true;
    [SerializeField, Min(0f)] private float aiTurnDelaySeconds = 0.8f;
    [SerializeField] private bool randomizeEnemyDeployment = true;

    [Header("Fast Resolve")]
    [SerializeField, Min(1f)] private float fastResolveAnimationSpeedMultiplier = 10f;
    [SerializeField] private bool fastResolveIsInstant = false;
    [SerializeField] private bool fastResolveControlsLocalTeam = true;
    [SerializeField] private bool fastResolveControlsEnemyTeams = true;

    public bool PreferNearestEnemy => preferNearestEnemy;
    public bool TryDirectAttackFirst => tryDirectAttackFirst;
    public bool TryMoveThenAttack => tryMoveThenAttack;
    public bool TryMoveTowardsEnemy => tryMoveTowardsEnemy;
    public bool AutoEndTurnWhenNoPlan => autoEndTurnWhenNoPlan;
    public float AiTurnDelaySeconds => aiTurnDelaySeconds;
    public bool RandomizeEnemyDeployment => randomizeEnemyDeployment;
    public float FastResolveAnimationSpeedMultiplier => fastResolveAnimationSpeedMultiplier;
    public bool FastResolveIsInstant => fastResolveIsInstant;
    public bool FastResolveControlsLocalTeam => fastResolveControlsLocalTeam;
    public bool FastResolveControlsEnemyTeams => fastResolveControlsEnemyTeams;
}
