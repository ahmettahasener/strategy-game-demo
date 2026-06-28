using StrategyDemo.Core;
using UnityEngine;

namespace StrategyDemo.Audio
{
    /// <summary>
    /// The game's ear: a single service that turns semantic <see cref="GameEvents"/> into sound. It is a
    /// pure listener (a "view" on game events, like the HUD) — gameplay code raises events and never
    /// references audio, so sound stays fully decoupled and can be muted or replaced in one place.
    /// Deliberately a plain scene component, not a Singleton: nothing calls it directly, so it needs no
    /// global access point — it just listens.
    ///
    /// Playback uses a small round-robin pool of 2D <see cref="AudioSource"/> voices so overlapping
    /// sounds don't cut each other off, and a short warm-up window swallows the burst of spawn/place
    /// events fired while the scene populates on load.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AudioManager : MonoBehaviour
    {
        [Header("Cues")]
        [SerializeField] private AudioCue _select;
        [SerializeField] private AudioCue _place;
        [SerializeField] private AudioCue _spawn;
        [SerializeField] private AudioCue _hit;
        [SerializeField] private AudioCue _death;
        [SerializeField] private AudioCue _click;
        [SerializeField] private AudioCue _command;
        [SerializeField] private AudioCue _deny;

        [Header("Mix")]
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 0.8f;
        [SerializeField, Min(1)] private int _voiceCount = 8;
        [Tooltip("Sounds are ignored for this long after load, to mute the initial spawn/place burst.")]
        [SerializeField, Min(0f)] private float _warmupSeconds = 0.3f;

        private AudioSource[] _voices;
        private int _nextVoice;
        private float _readyTime;

        private void Awake()
        {
            BuildVoices();
            _readyTime = Time.unscaledTime + _warmupSeconds;
        }

        private void OnEnable()
        {
            GameEvents.SelectionChanged += OnSelectionChanged;
            GameEvents.DamageTaken += OnDamageTaken;
            GameEvents.EntityDied += OnEntityDied;
            GameEvents.UnitSpawned += OnUnitSpawned;
            GameEvents.BuildingPlaced += OnBuildingPlaced;
            GameEvents.UiClicked += OnUiClicked;
            GameEvents.CommandIssued += OnCommandIssued;
            GameEvents.ActionDenied += OnActionDenied;
        }

        private void OnDisable()
        {
            GameEvents.SelectionChanged -= OnSelectionChanged;
            GameEvents.DamageTaken -= OnDamageTaken;
            GameEvents.EntityDied -= OnEntityDied;
            GameEvents.UnitSpawned -= OnUnitSpawned;
            GameEvents.BuildingPlaced -= OnBuildingPlaced;
            GameEvents.UiClicked -= OnUiClicked;
            GameEvents.CommandIssued -= OnCommandIssued;
            GameEvents.ActionDenied -= OnActionDenied;
        }

        // Selection fires with null on deselect; only the act of selecting something should click.
        private void OnSelectionChanged(ISelectable selected)
        {
            if (selected != null)
            {
                Play(_select);
            }
        }

        private void OnDamageTaken(IDamageable entity, int amount) => Play(_hit);

        private void OnEntityDied(IDamageable entity) => Play(_death);

        private void OnUnitSpawned(IDamageable unit) => Play(_spawn);

        private void OnBuildingPlaced(IDamageable building) => Play(_place);

        private void OnUiClicked() => Play(_click);

        private void OnCommandIssued() => Play(_command);

        private void OnActionDenied() => Play(_deny);

        /// <summary>Plays a cue on the next free voice with its configured volume and a random pitch.</summary>
        public void Play(AudioCue cue)
        {
            if (cue == null || !cue.HasClips || _voices == null || Time.unscaledTime < _readyTime)
            {
                return;
            }

            AudioClip clip = cue.PickClip();
            if (clip == null)
            {
                return;
            }

            AudioSource voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            voice.pitch = cue.RandomPitch();
            voice.PlayOneShot(clip, cue.Volume * _masterVolume);
        }

        private void BuildVoices()
        {
            _voices = new AudioSource[_voiceCount];
            for (int i = 0; i < _voiceCount; i++)
            {
                var voiceObject = new GameObject($"Voice{i}");
                voiceObject.transform.SetParent(transform, false);
                var source = voiceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f; // 2D, top-down board — no positional falloff
                _voices[i] = source;
            }
        }
    }
}
