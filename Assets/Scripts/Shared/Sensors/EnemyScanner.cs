using UnityEngine;

namespace Shared.Sensors
{
    public sealed class EnemyScanner : MonoBehaviour
    {
        private const float DefaultSoundCooldown = 3f;
        private const float DefaultDetectionRange = 5f;

        [SerializeField] private float _detectionRange = DefaultDetectionRange;
        [SerializeField] private LayerMask _enemyLayerMask;

        private AudioController _audioController;
        private bool _hasEnemyNearby;
        private float _lastSoundTime;
        private float _soundCooldown = DefaultSoundCooldown;

        public bool HasEnemyNearby => Physics2D.OverlapCircleAll(transform.position, _detectionRange, _enemyLayerMask).Length > 0;

        private void Start()
        {
            InitializeAudioController();
        }

        private void Update()
        {
            UpdateEnemyDetection();
        }

        private void InitializeAudioController()
        {
            _audioController = GetComponent<AudioController>() ?? FindObjectOfType<AudioController>();
        }

        private void UpdateEnemyDetection()
        {
            bool currentEnemyDetection = HasEnemyNearby;

            if (ShouldPlayDetectionSound(currentEnemyDetection))
            {
                PlayDetectionSound();
                _lastSoundTime = Time.time;
            }

            _hasEnemyNearby = currentEnemyDetection;
        }

        private bool ShouldPlayDetectionSound(bool currentEnemyDetection)
        {
            return currentEnemyDetection && !_hasEnemyNearby && Time.time >= _lastSoundTime + _soundCooldown;
        }

        private void PlayDetectionSound()
        {
            _audioController?.PlayEnemyDetectedSound();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _detectionRange);
        }
    }
}