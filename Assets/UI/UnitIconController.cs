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
    public ICombatUnit CombatUnit { get; internal set; }

    public void SetInfo(Sprite sprite, bool isBlueTeam)
    {
        Debug.Log(name + " setted to " + sprite.name);
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

    public void Link(ICombatUnit model, int turn = 0)
    {
        if (model != null)
        {
            Debug.Log("linking UnitIconController" + name + " with " + model.UnitType);
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
