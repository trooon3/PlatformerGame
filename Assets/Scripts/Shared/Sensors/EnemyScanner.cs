using UnityEngine;

namespace Shared.Sensors
{
    public sealed class EnemyScanner : MonoBehaviour
    {
        private const float DefaultSoundCooldown = 3f;
        private const float DefaultDetectionRange = 5f;

        [SerializeField] private float _detectionRange = DefaultDetectionRange;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private SfxPlayer _sfxPlayer;
        [SerializeField] private SoundConfiguration _soundConfiguration;

        private bool _hadEnemyNearby;
        private float _lastSoundTime;

        public bool HasEnemyNearby => Physics2D.OverlapCircle(
            transform.position,
            _detectionRange,
            _enemyLayerMask);

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
            if (_sfxPlayer == null)
            {
                _sfxPlayer = GetComponent<SfxPlayer>();
            }

            if (_sfxPlayer == null)
            {
                _sfxPlayer = FindFirstObjectByType<SfxPlayer>();
            }
        }

        private void UpdateEnemyDetection()
        {
            bool hasEnemyNearby = HasEnemyNearby;

            if (ShouldPlayDetectionSound(hasEnemyNearby))
            {
                _lastSoundTime = Time.time;
                PlayDetectionSound(); // ← Добавить
            }

            _hadEnemyNearby = hasEnemyNearby;
        }

        private void PlayDetectionSound()
        {
            if (_sfxPlayer != null && _soundConfiguration != null)
            {
                _sfxPlayer.Play(_soundConfiguration.EnemyDetectedSound);
            }
        }

        private bool ShouldPlayDetectionSound(bool hasEnemyNearby)
        {
            return hasEnemyNearby &&
                   _hadEnemyNearby == false &&
                   Time.time >= _lastSoundTime + DefaultSoundCooldown;
        }
    }
}