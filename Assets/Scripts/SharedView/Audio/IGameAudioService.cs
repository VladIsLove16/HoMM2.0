using UnityEngine;

namespace SharedView.Audio
{
    public interface IGameAudioService
    {
        GameAudioSettingsSO Settings { get; }
        void PlayButtonHover();
        void PlayButtonClick();
        void PlayPlayerFootstep(Vector3 position);
        void PlayCombatImpact(CombatSfxType type, Vector3 position);
        void Play(GameAudioClipSet clipSet, Vector3? worldPosition = null);
    }
}
