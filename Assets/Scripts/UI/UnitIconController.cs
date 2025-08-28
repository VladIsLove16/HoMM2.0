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
    public bool IsBlueTeam { get; internal set; }
    public ICombatObject CombatUnit { get; internal set; }

    public void SetInfo(Sprite sprite, bool isBlueTeam)
    {
        SetUnitIcon(sprite);
        SetTeamColor(isBlueTeam);
    }

    private void SetUnitIcon(Sprite sprite)
    {
        UnitIcon.sprite = sprite;
    }

    private void SetTeamColor(bool isBlueTeam)
    {
        IsBlueTeam = isBlueTeam;
        UnitTeamBorder.color = isBlueTeam ? BlueTeamColor : RedTeamColor;
    }

    private void SetInfo(UnitIconController unitIconController)
    {
        SetInfo(unitIconController.Sprite, unitIconController.IsBlueTeam);
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
