using TMPro;
using UnityEngine;

public class TurnTextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnNumber;
    [SerializeField] private Animator turnNumberAnimator;
    private int currentTurn;
    public void OnTurnNumberChanged(int turn)
    {
        turnNumberAnimator.SetTrigger("turnChanged");
        currentTurn = turn;
    }
    public void UpdateTurnText()
    {
        turnNumber.text = "Turn " + currentTurn.ToString();
    }
}