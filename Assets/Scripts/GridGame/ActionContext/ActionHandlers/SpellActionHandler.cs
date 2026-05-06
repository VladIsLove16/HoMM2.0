using System;
using System.Collections;
using System.Collections.Generic;
using SharedView.Audio;
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
    private readonly GameModel _gameModel;
    private readonly IGameAudioService _audioService;
    private readonly IUnitAudioProfileProvider _unitAudioProfileProvider;
    private readonly IWorldToCellProvider _worldToCellProvider;

    public SpellActionHandler(
        MovementSystem movementSystem,
        GameModel gameModel,
        IGameAudioService audioService = null,
        IUnitAudioProfileProvider unitAudioProfileProvider = null,
        IWorldToCellProvider worldToCellProvider = null)
    {
        _gameModel = gameModel;
        _audioService = audioService;
        _unitAudioProfileProvider = unitAudioProfileProvider;
        _worldToCellProvider = worldToCellProvider;
    }

    public void Execute(ActionContext ctx)
    {
        PlayMagicAudio(ctx);

        SpellData spellData = null;
        GridXZ<GameCell> gridXZ = null;
        if (spellCasterService == null || gridXZ == null || caster == null)
        {
            Debug.LogWarning("[SpellActionHandler] Spell execution dependencies are not configured.");
            return;
        }

        spellCasterService.Cast(spellData, ctx.TargetCell, (pos) => gridXZ.GetGridObject(pos).Unit, caster);
    }

    private void PlayMagicAudio(ActionContext ctx)
    {
        if (_audioService == null)
            return;

        var position = _worldToCellProvider != null
            ? _worldToCellProvider.ToWorld(ctx.TargetCell.x, ctx.TargetCell.y)
            : new Vector3(ctx.TargetCell.x, 0f, ctx.TargetCell.y);

        var casterCell = _gameModel?.GetCell(ctx.FromCell);
        var casterUnit = casterCell?.Unit;
        if (casterUnit != null &&
            _unitAudioProfileProvider != null &&
            _unitAudioProfileProvider.TryGetAudioProfile(casterUnit.UnitType.Value, out var profile))
        {
            var clipSet = profile?.GetAttackClips(CombatSfxType.Magic);
            if (clipSet != null && clipSet.HasClips)
            {
                _audioService.Play(clipSet, position);
                return;
            }
        }

        _audioService.PlayCombatImpact(CombatSfxType.Magic, position);
    }


    public bool CanExecute(ActionContext ctx)
    {
        return ctx.AbilityUsed != SpellType.None;
    }

    public PreviewResult GetPreview(ActionContext actionContext)
    {
        var preview = new PreviewResult();
        preview.Add(CellState.attackTarget,new List<Vector2Int>() { actionContext.TargetCell });
        return preview;
    }
}
