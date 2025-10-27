using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Adventure/StatIcons", fileName = "StatIcons")]
public class StatIcons : ScriptableObject
{
    [SerializeField]public List<StatIconBinding> statIconBindings = new List<StatIconBinding>();
}
