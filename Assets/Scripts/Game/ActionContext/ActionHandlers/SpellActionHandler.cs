using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellActionHandler : IActionHandler
{
    private IEffectApplier caster;
    private SpellCasterService spellCasterService;
    public SpellActionHandler(SpellCasterService spellCasterService)
    {
        this.spellCasterService = spellCasterService;
    }
    public void AddTarget(GameCell gameCell)
    {
        return;
    }

    public bool CanHandle(ActionContext ctx)
    {
        throw new System.NotImplementedException();
    }

    public bool CanShowPreview(ActionContext ctx)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Execute(ActionContext ctx)
    {
        throw new System.NotImplementedException();
    }

    public List<(Vector2Int, bool)> GetRoute(GameCell gameCell)
    {
        return new() { ( gameCell.Position,true) };
    }

    public void HidePreview()
    {
        throw new System.NotImplementedException();
    }

    public bool IsAvailable(GameCell gameCell)
    {
        return true;
    }

    public bool Perform(GameCell gameCell)
    {
        SpellData spellData = null;
        GridXZ< GameCell> gridXZ = null;    
        spellCasterService.Cast(spellData, gameCell.Position, (pos)=> gridXZ.GetGridObject(pos).Unit, caster);
        return true;
    }

    public void ShowAvaiableTargetCells()
    {
        throw new System.NotImplementedException();
    }

    public void ShowPreview(ActionContext ctx)
    {
        throw new System.NotImplementedException();
    }

    void IActionHandler.Execute(ActionContext ctx)
    {
        throw new System.NotImplementedException();
    }
}
