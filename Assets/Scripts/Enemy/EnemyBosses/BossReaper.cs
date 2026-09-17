using GeneralLogicEnemies;
using Player;
using Shared.Damage;
using System.Collections;
using UnityEngine;
using UnityEngine.Video; 

namespace EasyBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossReaper : Entity, IDamageable
    {
        private const float InitialDodgeTime = -Mathf.Infinity;
        private const int IdleState = 0;
        private const int AttackState = 1;
        private const int SkillState = 2;
        private const int DeathState = 3;
        private const float DestroyDelay = 3f;
        private const float DefaultSoundEffectVolume = 1f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;

        [Header("Intro Video")]
        [SerializeField] private VideoPlayer _introVideoPlayer;
        [SerializeField] private GameObject _videoPanel;

        [Header("Player Detection")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private float _detectionRadius = 1f;

        [Header("Movement and Attack")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _attackRange = 1.5f;
        [SerializeField] private float _attackCooldown = 2f;
        [SerializeField] private int _attackDamage = 1;

        [Header("Evasion")]
        [SerializeField] private float _dodgeChance = 0.25f;
        [SerializeField] private float _dodgeDistance = 2f;
        [SerializeField] private float _dodgeDuration = 0.4f;
        [SerializeField] private float _dodgeCooldown = 1f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _dodgeSound;
        [SerializeField] private float _soundEffectVolume = DefaultSoundEffectVolume;

        private Animator _animator;
        private Rigidbody2D _rigidbody;
        private Transform _playerTransform;
        private AudioController _audioController;

        private bool _isActivated;
        private bool _isDead;
        private bool _isDodging;
        private float _lastDodgeTime = InitialDodgeTime;
        private float _nextAttackTime;

        private bool _isVideoPlaying;
        private bool _hasPlayedIntro;

        private static readonly int StateParameterHash = Animator.StringToHash("state");

        public float DetectionRadius => _detectionRadius;
        public float MoveSpeed => _moveSpeed;
        public float AttackRange => _attackRange;
        public float AttackCooldown => _attackCooldown;
        public int AttackDamage => _attackDamage;

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

            if (_isActivated == false || _playerTransform == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (IsAttacking() && distanceToPlayer > _attackRange)
            {
                InterruptAttack();
                return;
            }

            if (_isDodging == false && distanceToPlayer <= _attackRange && Time.time >= _nextAttackTime)
            {
                Attack();
                return;
            }

            if (IsAttacking() == false && _isDodging == false)
            {
                MoveTowardPlayer();
            }
            else
            {
                StopHorizontalMovement();
            }
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

        private void OnVideoFinished(VideoPlayer vp) { SkipOrEndVideo(); }

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
            if (_isDead || amount <= 0) return;

            TryDodge();
            if (_isDodging) return;

            PlayTakeDamageSound();
            base.TakeDamage(amount);
        }

        public override void Die()
        {
            if (_isDead) return;

            _isDead = true;
            StopHorizontalMovement();
            SetAnimationState(DeathState);

            PlayDeathSound();
            StopBossMusic();
            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        public void DealDamage()
        {
            if (_playerTransform == null)
            {
                PlayAttackMissSound();
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            if (distanceToPlayer > _attackRange)
            {
                PlayAttackMissSound();
                return;
            }

            IDamageable damageable = _playerTransform.GetComponent<IDamageable>() ?? _playerTransform.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                PlayAttackMissSound();
                return;
            }

            damageable.TakeDamage(_attackDamage);
        }

        private void TryActivate()
        {
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
        }

        private void Attack()
        {
            _nextAttackTime = Time.time + _attackCooldown;
            SetAnimationState(AttackState);
            PlayAttackSound();
        }

        private bool IsAttacking() { return _animator != null && _animator.GetInteger(StateParameterHash) == AttackState; }

        private void InterruptAttack()
        {
            const float interruptionCooldown = 0.5f;
            SetAnimationState(IdleState);
            _nextAttackTime = Time.time + interruptionCooldown;
        }

        private void TryDodge()
        {
            if (_isDead || _isDodging || Time.time < _lastDodgeTime + _dodgeCooldown) return;
            if (Random.value > _dodgeChance) return;

            _lastDodgeTime = Time.time;
            StartCoroutine(DodgeRoutine());
        }

        private IEnumerator DodgeRoutine()
        {
            _isDodging = true;
            SetAnimationState(SkillState);
            PlayDodgeSound();

            Vector2 startPosition = transform.position;
            Vector2 direction = _playerTransform != null ? ((Vector2)transform.position - (Vector2)_playerTransform.position).normalized : Vector2.right;
            Vector2 targetPosition = startPosition + direction * _dodgeDistance;
            float elapsedTime = 0f;

            while (elapsedTime < _dodgeDuration)
            {
                Vector2 newPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / _dodgeDuration);
                transform.position = new Vector2(newPosition.x, transform.position.y);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _isDodging = false;
            SetAnimationState(IdleState);
        }

        private void StartBossMusic() { if (_audioController != null && _bossMusic != null) _audioController.PlayBossMusic(_bossMusic); }
        private void StopBossMusic() { _audioController?.StopBossMusic(); }
        private void StopHorizontalMovement() { if (_rigidbody != null) _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y); }
        private void SetAnimationState(int state) { if (_animator != null) _animator.SetInteger(StateParameterHash, state); }
        private void PlayAttackSound() { PlaySound(_attackSound, _soundEffectVolume); }
        private void PlayAttackMissSound() { PlaySound(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier); }
        private void PlayTakeDamageSound() { PlaySound(_takeDamageSound, _soundEffectVolume); }
        private void PlayDeathSound() { PlaySound(_deathSound, _soundEffectVolume); }
        private void PlayDodgeSound() { PlaySound(_dodgeSound, _soundEffectVolume); }
        private void PlaySound(AudioClip clip, float volumeMultiplier)
        {
            if (clip != null && _audioController != null) _audioController.PlayOneShotWithVolume(clip, volumeMultiplier);
        }
    }
}