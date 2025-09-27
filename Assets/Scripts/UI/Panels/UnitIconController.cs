using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitIconController : MonoBehaviour
{
    [SerializeField] Image UnitIcon;
    [SerializeField] Image UnitTeamBorder;
    [SerializeField] Color BlueTeamColor;
    [SerializeField] Color RedTeamColor;
    [SerializeField] UnitType unitType;
    [SerializeField] TextMeshProUGUI turnText;

    public Sprite Sprite => UnitIcon.sprite;
    public Team Team { get; internal set; }
    public ICombatObject CombatUnit { get; internal set; }

    public void SetInfo(Sprite sprite, Team team)
    {
        SetUnitIcon(sprite);
        SetTeamColor(team);
    }

    private void SetUnitIcon(Sprite sprite)
    {
        UnitIcon.sprite = sprite;
    }

    private void SetTeamColor(Team team)
    {
        Team = team;
        UnitTeamBorder.color = team == Team.Blue ? BlueTeamColor : RedTeamColor;
    }

    private void SetInfo(UnitIconController unitIconController)
    {
        SetInfo(unitIconController.Sprite, unitIconController.Team);
        Link(unitIconController.CombatUnit);
    }

    public void Link(ICombatObject model, int turn = 0)
    {
        if (model != null)
        {
            CombatUnit = model;
            unitType = model.UnitType;
        }
        turnText.text = turn.ToString();
    }

    internal void Clear()
    {
        SetUnitIcon(null);
    }
}
