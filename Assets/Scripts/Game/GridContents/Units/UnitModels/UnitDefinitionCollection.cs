using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/UnitDefinition Collection")]
public class UnitDefinitionCollection : ScriptableObject
{
    [SerializeField] private List<UnitDefinitionSO> items = new List<UnitDefinitionSO>();

    public IReadOnlyList<UnitDefinitionSO> Items => items;
    public int Count => items.Count;

    public UnitDefinitionSO GetByIndex(int index)
    {
        if (items == null || items.Count == 0) return null;
        var clamped = Mathf.Clamp(index, 0, items.Count - 1);
        return items[clamped];
    }

    public IEnumerable<UnitDefinitionSO> GetPage(int pageIndex, int pageSize)
    {
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
        var start = pageIndex * pageSize;
        for (int i = 0; i < pageSize; i++)
        {
            var idx = start + i;
            if (idx >= 0 && idx < items.Count)
                yield return items[idx];
        }
    }

    public int GetTotalPages(int pageSize)
    {
        if (pageSize <= 0) return 0;
        return Mathf.CeilToInt((float)Count / pageSize);
    }
}


