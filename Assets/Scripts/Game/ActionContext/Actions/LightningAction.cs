using System.Collections.Generic;
using UnityEngine;

public class LightningAction : IAction
{
    private ISpellCaster caster;
    private SpellCasterService spellCasterService;
    SpellData spellData = null;
    GridXZ<GameCell> gridXZ = null; 
    public LightningAction(SpellCasterService spellCasterService)
    {
        this.spellCasterService = spellCasterService;
    }
    public void AddTarget(GameCell gameCell)
    {
        return;
    }

    public List<(Vector2Int, bool)> GetRoute(GameCell gameCell)
    {
        return new() { ( gameCell.Position,true) };
    }

    public bool IsAvailable(GameCell gameCell)
    {
        return true;
    }

    public bool Perform(GameCell gameCell)
    {
        spellCasterService.Cast(spellData, gameCell.Position, (pos)=> gridXZ.GetGridObject(pos).GetUnit(), caster);
        return true;
    }
}