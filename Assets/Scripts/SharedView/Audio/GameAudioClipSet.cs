using System;
using UnityEngine;

namespace SharedView.Audio
{
    [Serializable]
    public sealed class GameAudioClipSet
    {
        [SerializeField] private AudioClip[] clips;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float pitchMin = 1f;
        [SerializeField] [Range(0.1f, 3f)] private float pitchMax = 1f;

        public AudioClip[] Clips => clips;
        public float Volume => volume;
        public float PitchMin => pitchMin;
        public float PitchMax => Mathf.Max(pitchMin, pitchMax);

        public bool HasClips => clips != null && clips.Length > 0;

        public AudioClip SelectClip()
        {
            if (!HasClips)
                return null;

            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }

        public float SelectPitch()
        {
            return Mathf.Approximately(PitchMin, PitchMax)
                ? PitchMin
                : UnityEngine.Random.Range(PitchMin, PitchMax);
        }
    }
}
