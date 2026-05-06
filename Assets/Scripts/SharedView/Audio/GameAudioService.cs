using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SharedView.Audio
{
    public sealed class GameAudioService : IGameAudioService, IDisposable
    {
        private const string DefaultSettingsResourcePath = "Audio/GameAudioSettings";

        private readonly GameAudioSettingsSO _settings;
        private readonly GameObject _root;
        private readonly AudioSource _uiSource;
        private readonly Transform _spatialRoot;
        private bool _disposed;

        public GameAudioSettingsSO Settings => _settings;

        public GameAudioService(GameAudioSettingsSO settings)
        {
            _settings = settings != null
                ? settings
                : Resources.Load<GameAudioSettingsSO>(DefaultSettingsResourcePath);

            _root = new GameObject("[GameAudioService]");
            _spatialRoot = _root.transform;
            _uiSource = _root.AddComponent<AudioSource>();
            ConfigureSource(_uiSource, 0f, ResolveMixerGroup());
        }

        public void PlayButtonHover() => Play(_settings != null ? _settings.ButtonHover : null);

        public void PlayButtonClick() => Play(_settings != null ? _settings.ButtonClick : null);

        public void PlayPlayerFootstep(Vector3 position)
        {
            Play(_settings != null ? _settings.PlayerFootsteps : null, position);
        }

        public void PlayCombatImpact(CombatSfxType type, Vector3 position)
        {
            if (_settings == null)
                return;

            var clipSet = type switch
            {
                CombatSfxType.Melee => _settings.MeleeImpact,
                CombatSfxType.Ranged => _settings.RangedImpact,
                CombatSfxType.Magic => _settings.MagicImpact,
                _ => _settings.GenericImpact
            };

            if (clipSet == null || !clipSet.HasClips)
            {
                clipSet = _settings.GenericImpact;
            }

            Play(clipSet, position);
        }

        public void Play(GameAudioClipSet clipSet, Vector3? worldPosition = null)
        {
            if (_disposed)
                return;

            if (clipSet == null || !clipSet.HasClips)
                return;

            var clip = clipSet.SelectClip();
            if (clip == null)
                return;

            if (!worldPosition.HasValue)
            {
                _uiSource.pitch = clipSet.SelectPitch();
                _uiSource.PlayOneShot(clip, clipSet.Volume);
                return;
            }

            var source = CreateSpatialSource(worldPosition.Value, clipSet.SelectPitch());
            source.PlayOneShot(clip, clipSet.Volume);
            UnityEngine.Object.Destroy(source.gameObject, clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch)) + 0.1f);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root);
            }
        }

        private AudioSource CreateSpatialSource(Vector3 position, float pitch)
        {
            var go = new GameObject("[SFX]");
            go.transform.SetParent(_spatialRoot, false);
            go.transform.position = position;

            var source = go.AddComponent<AudioSource>();
            ConfigureSource(source, 1f, ResolveMixerGroup());
            source.pitch = pitch;
            return source;
        }

        private AudioMixerGroup ResolveMixerGroup()
        {
            return _settings != null ? _settings.EffectsMixerGroup : null;
        }

        private static void ConfigureSource(AudioSource source, float spatialBlend, AudioMixerGroup mixerGroup)
        {
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2f;
            source.maxDistance = 28f;
            source.outputAudioMixerGroup = mixerGroup;
        }
    }
}
