using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
public class SpellActionHandler : IActionHandler
{
    public ActionType ActionType
    {
        get
        {
            return ActionType.Spell;
        }
    }
    private IEffectApplier caster;
    private SpellCasterService spellCasterService;
    public SpellActionHandler(MovementSystem movementSystem ,GameModel gameModel)
    {
    }

    public void Execute(ActionContext ctx)
    {
        SpellData spellData = null;
        GridXZ<GameCell> gridXZ = null;
        spellCasterService.Cast(spellData, ctx.TargetCell, (pos) => gridXZ.GetGridObject(pos).Unit, caster);
    }


    public bool CanExecute(ActionContext ctx)
    {
        return true;
    }

    public PreviewResult GetPreview(ActionContext actionContext)
    {
        var preview = new PreviewResult();
        preview.Add(CellState.attackTarget,new List<Vector2Int>() { actionContext.TargetCell });
        return preview;
    }
}
