using UnityEngine;


[CreateAssetMenu(menuName = "GameConfiguration/GameConfiguration data")]
public class GameConfigurationProviderSO : ScriptableObject
{
    [SerializeField] public int Height = 8;
    [SerializeField] public int Width = 8;
    [SerializeField] public GridContentEntrySO gridContentEntrySO;

}