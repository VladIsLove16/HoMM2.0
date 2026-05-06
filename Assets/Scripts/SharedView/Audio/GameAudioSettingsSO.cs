using UnityEngine;
using UnityEngine.Audio;

namespace SharedView.Audio
{
    [CreateAssetMenu(menuName = "Audio/Game Audio Settings")]
    public sealed class GameAudioSettingsSO : ScriptableObject
    {
        [Header("Routing")]
        [SerializeField] private AudioMixerGroup effectsMixerGroup;

        [Header("UI")]
        [SerializeField] private GameAudioClipSet buttonHover = new();
        [SerializeField] private GameAudioClipSet buttonClick = new();

        [Header("Adventure")]
        [SerializeField] private GameAudioClipSet playerFootsteps = new();
        [SerializeField] [Min(0.05f)] private float walkStepInterval = 0.48f;
        [SerializeField] [Min(0.05f)] private float sprintStepInterval = 0.34f;

        [Header("Combat")]
        [SerializeField] private GameAudioClipSet meleeImpact = new();
        [SerializeField] private GameAudioClipSet rangedImpact = new();
        [SerializeField] private GameAudioClipSet magicImpact = new();
        [SerializeField] private GameAudioClipSet genericImpact = new();

        public AudioMixerGroup EffectsMixerGroup => effectsMixerGroup;
        public GameAudioClipSet ButtonHover => buttonHover;
        public GameAudioClipSet ButtonClick => buttonClick;
        public GameAudioClipSet PlayerFootsteps => playerFootsteps;
        public float WalkStepInterval => walkStepInterval;
        public float SprintStepInterval => sprintStepInterval;
        public GameAudioClipSet MeleeImpact => meleeImpact;
        public GameAudioClipSet RangedImpact => rangedImpact;
        public GameAudioClipSet MagicImpact => magicImpact;
        public GameAudioClipSet GenericImpact => genericImpact;
    }
}
