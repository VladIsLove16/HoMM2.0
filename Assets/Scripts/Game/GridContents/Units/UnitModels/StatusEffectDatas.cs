// StatusEffectDatas.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffectDatas")]
public class StatusEffectDatas : ScriptableObject
{
   [SerializeField] List<StatusEffectData> statusEffectDatas;
    
    public IReadOnlyDictionary<StatusEffectType, StatusEffectData> ToDictionary()
    {
        return statusEffectDatas.ToDictionary(x => x.StatusEffectType);
    }
}