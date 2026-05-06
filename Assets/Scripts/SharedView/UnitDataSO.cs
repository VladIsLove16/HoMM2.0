using System;
using SharedView.Audio;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/Unit Data", fileName = "UnitData")]
public class UnitDataSO : ScriptableObject
{
    [SerializeField] private UnitType type;
    [SerializeField] private UnitSharedDataSO sharedData;
    [SerializeField] private UnitStatsSource unitStats;
    [SerializeField] private UnitAudioProfile audioProfile = new();

    public UnitType Type => type;
    public UnitSharedDataSO SharedData => sharedData;
    public UnitAudioProfile AudioProfile => audioProfile;
    public UnitStats UnitStats
    {
        get
        {
            if(unitStats.TryGetUnitStats(out var stats))
            {
                return stats;
            }
            else
                throw new InvalidOperationException($"UnitStatsSource for UnitType {type} is not properly configured in {name}.");
        }
    }
}
