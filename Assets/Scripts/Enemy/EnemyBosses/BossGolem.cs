using GeneralLogicEnemies;
using Shared.Damage;
using System.Collections;
using UnityEngine;
using UnityEngine.Video; 

namespace MiddleBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossGolem : Entity, IDamageable
    {
        private const float DestroyDelay = 1f;
        private const float DefaultSoundEffectVolume = 1f;

        [Header("Intro Video")]
        [SerializeField] private VideoPlayer _introVideoPlayer;
        [SerializeField] private GameObject _videoPanel;

        [Header("Player Detection")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _detectionRadius = 1f;

        [Header("Movement and Attack")]
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private int _attackDamage = 1;

        [Header("Taking Damage")]
        [SerializeField] private float _hitStunDuration = 0.4f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private float _soundEffectVolume = DefaultSoundEffectVolume;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Transform _playerTransform;
        private AudioController _audioController;

        private float _nextAttackTime;
        private bool _isActivated;
        private bool _isHit;
        private bool _isDead;
        private bool _wasAttackSuccessful;

        private bool _isVideoPlaying;
        private bool _hasPlayedIntro;

        private static readonly int StateParameterHash = Animator.StringToHash("state");

        private BossGolemState CurrentAnimatorState
        {
            get => _animator != null ? (BossGolemState)_animator.GetInteger(StateParameterHash) : BossGolemState.Idle;
            set
            {
                if (_animator != null)
                {
                    _animator.SetInteger(StateParameterHash, (int)value);
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioController = FindFirstObjectByType<AudioController>();

            if (_videoPanel != null) _videoPanel.SetActive(false);
        }

        private void Update()
        {
            if (_isDead) return;

            if (_isVideoPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SkipOrEndVideo();
                return;
            }

            if (_isActivated == false) TryActivate();

            if (_isActivated == false || _isHit || _playerTransform == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _attackRange && Time.time >= _nextAttackTime)
            {
                StartAttack();
                return;
            }

            MoveTowardPlayer();
        }

        private void OnDestroy()
        {
            if (_isVideoPlaying) Time.timeScale = 1f;
            if (_introVideoPlayer != null) _introVideoPlayer.loopPointReached -= OnVideoFinished;
            if (_isActivated) StopBossMusic();
        }

        private void PlayIntroVideo()
        {
            _hasPlayedIntro = true;
            _isVideoPlaying = true;

            if (_videoPanel != null) _videoPanel.SetActive(true);
            if (_introVideoPlayer != null)
            {
                _introVideoPlayer.loopPointReached += OnVideoFinished;
                _introVideoPlayer.Play();
            }

            Time.timeScale = 0f; 
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            SkipOrEndVideo();
        }

        private void SkipOrEndVideo()
        {
            if (!_isVideoPlaying) return;
            _isVideoPlaying = false;

            if (_introVideoPlayer != null)
            {
                _introVideoPlayer.Stop();
                _introVideoPlayer.loopPointReached -= OnVideoFinished;
            }

            if (_videoPanel != null) _videoPanel.SetActive(false);

            Time.timeScale = 1f; 
        }

        public override void TakeDamage(int amount)
        {
            if (_isDead || _isHit || amount <= 0) return;

            PlayTakeDamageSound();
            StartCoroutine(HitRoutine());
            base.TakeDamage(amount);
        }

        public override void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _isHit = true;

            StopMovement();
            CurrentAnimatorState = BossGolemState.Hit;

            PlayDeathSound();
            StopBossMusic();
            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        public void DealDamage() { AnimEvent_AttackHit(); }

        public void AnimEvent_AttackHit()
        {
            PlayAttackSound();

            Collider2D hit = Physics2D.OverlapCircle(transform.position, _attackRange, _playerLayer);
            if (hit == null)
            {
                _wasAttackSuccessful = false;
                return;
            }

            IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                _wasAttackSuccessful = false;
                return;
            }

            damageable.TakeDamage(_attackDamage);
            _wasAttackSuccessful = true;
        }

        public void AnimEvent_AttackEnd()
        {
            if (_wasAttackSuccessful == false) PlayAttackMissSound();
            _nextAttackTime = Time.time + _attackCooldown;
            CurrentAnimatorState = BossGolemState.Idle;
            _wasAttackSuccessful = false;
        }

        public void AnimEvent_AttackStart() { PlayAttackSound(); }
        public void AnimEvent_HitEnd() { _isHit = false; }
        public void AnimEvent_DeathEnd() { Destroy(gameObject); }

        private void TryActivate()
        {
            if (_groundCheckPoint == null) return;

            Collider2D playerCollider = Physics2D.OverlapCircle(_groundCheckPoint.position, _detectionRadius, _playerLayer);
            if (playerCollider == null) return;

            _isActivated = true;
            _playerTransform = playerCollider.transform;

            if (!_hasPlayedIntro && _introVideoPlayer != null && _videoPanel != null)
            {
                PlayIntroVideo();
            }

            StartBossMusic();
        }

        private void MoveTowardPlayer()
        {
            if (_playerTransform == null || _rigidbody == null) return;

            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
            CurrentAnimatorState = BossGolemState.Move;
        }

        private void StartAttack()
        {
            StopMovement();
            CurrentAnimatorState = BossGolemState.Attack;
            _wasAttackSuccessful = false;
        }

        private IEnumerator HitRoutine()
        {
            _isHit = true;
            StopMovement();
            CurrentAnimatorState = BossGolemState.Hit;
            yield return new WaitForSeconds(_hitStunDuration);
            _isHit = false;
        }

        private void StartBossMusic()
        {
            if (_audioController != null && _bossMusic != null) _audioController.PlayBossMusic(_bossMusic);
        }

        private void StopBossMusic() { _audioController?.StopBossMusic(); }
        private void StopMovement() { if (_rigidbody != null) _rigidbody.velocity = Vector2.zero; }
        private void PlayAttackSound() { PlaySound(_attackSound, _soundEffectVolume); }
        private void PlayAttackMissSound() { PlaySound(_attackMissSound, _soundEffectVolume); }
        private void PlayTakeDamageSound() { PlaySound(_takeDamageSound, _soundEffectVolume); }
        private void PlayDeathSound() { PlaySound(_deathSound, _soundEffectVolume); }
        private void PlaySound(AudioClip clip, float volumeMultiplier)
        {
            if (clip != null && _audioController != null) _audioController.PlayOneShotWithVolume(clip, volumeMultiplier);
        }

        private enum BossGolemState { Idle = 0, Move = 1, Attack = 2, Hit = 3 }
    }
}