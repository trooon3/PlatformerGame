using AudioSystem;
using UnityEngine;

namespace Traps
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class LightningTrapSoundController : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Header("Sound Configuration")]
        [SerializeField] private AudioClip _lightningStrikeSound;

        [Header("Audio Player Integration")]
        [SerializeField] private AudioController _audioController;

        [Header("Distance Settings")]
        [SerializeField] private bool _enableDistanceCheck = true;
        [SerializeField] private float _maxSoundDistance = 20f;

        private IAudioPlayer _audioPlayer;
        private IDistanceChecker _distanceChecker;
        private Transform _playerTransform;

        private void Awake()
        {
            InitializeAudioPlayer();
            InitializePlayerReference();
            InitializeAudioControllerReference();
            InitializeDistanceChecker();
        }

        private void InitializeAudioPlayer()
        {
            AudioSource audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;

            _audioPlayer = new AudioPlayer(audioSource);
        }

        private void InitializePlayerReference()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);

            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        private void InitializeAudioControllerReference()
        {
            if (_audioController == null)
            {
                _audioController = FindFirstObjectByType<AudioController>();
            }
        }

        private void InitializeDistanceChecker()
        {
            _distanceChecker = new DistanceChecker();
        }

        public void PlayLightningStrikeSound()
        {
            if (_lightningStrikeSound == null)
            {
                return;
            }

            if (ShouldPlaySound() == false)
            {
                return;
            }

            float volume = GetSoundVolume();

            _audioPlayer.PlayOneShot(_lightningStrikeSound, volume);
        }

        private bool ShouldPlaySound()
        {
            if (_enableDistanceCheck == false || _playerTransform == null)
            {
                return true;
            }

            return _distanceChecker.IsWithinDistance(transform.position, _playerTransform.position, _maxSoundDistance);
        }

        private float GetSoundVolume()
        {
            float defaultVolume = 1f;

            if (_audioController != null)
            {
                return _audioController.SoundEffectsVolume;
            }

            return defaultVolume;
        }

        public void StopSound()
        {
            _audioPlayer.Stop();
        }

        public void SetLightningSound(AudioClip clip)
        {
            _lightningStrikeSound = clip;
        }

        public void SetMaxDistance(float distance)
        {
            _maxSoundDistance = distance;
        }

        public void SetAudioController(AudioController controller)
        {
            _audioController = controller;
        }
    }
}