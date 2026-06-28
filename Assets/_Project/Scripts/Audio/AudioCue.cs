using UnityEngine;

namespace StrategyDemo.Audio
{
    /// <summary>
    /// Data-driven definition of a sound effect: one or more interchangeable clips plus mix settings.
    /// Keeping cues as ScriptableObjects lets designers tune volume/pitch and swap clips without code,
    /// and lets <see cref="AudioManager"/> stay a thin player that knows nothing about specific sounds.
    /// </summary>
    [CreateAssetMenu(menuName = "StrategyDemo/Audio Cue", fileName = "AudioCue")]
    public sealed class AudioCue : ScriptableObject
    {
        [Tooltip("One is picked at random per play, so repeated sounds don't feel mechanical.")]
        [SerializeField] private AudioClip[] _clips;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [Tooltip("A random pitch in this range is applied per play for subtle variation.")]
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.96f, 1.04f);

        public bool HasClips => _clips != null && _clips.Length > 0;

        public float Volume => _volume;

        /// <summary>A random clip from the set (null if none assigned).</summary>
        public AudioClip PickClip()
        {
            return HasClips ? _clips[Random.Range(0, _clips.Length)] : null;
        }

        /// <summary>A random pitch within the configured range.</summary>
        public float RandomPitch()
        {
            return Random.Range(_pitchRange.x, _pitchRange.y);
        }
    }
}
