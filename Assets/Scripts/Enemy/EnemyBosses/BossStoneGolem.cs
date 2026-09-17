using GeneralLogicEnemies;
using Shared.Damage;
using System;
using UnityEngine;
using UnityEngine.Video; 

namespace HardBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossStoneGolem : Entity, IDamageable
    {
        private const float TimerEndThreshold = 0f;
        private const float GroundedVelocityThreshold = 0.05f;
        private const float BoxRotationAngle = 0f;
        private const float MaximumBeamLength = 20f;
        private const float MaximumHealth = 4f;
        private const int BossHealthAfterImmunity = 10;
        private const int BossMeleeDamage = 1;
        private const int BossLaserDamage = 1;
        private const float DestroyDelay = 3f;
        private const float LaserSoundVolumeMultiplier = 0.8f;
        private const float MoveSoundVolumeMultiplier = 0.4f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;

        [Header("Intro Video")]
        [SerializeField] private VideoPlayer _introVideoPlayer;
        [SerializeField] private GameObject _videoPanel;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Combat Settings")]
        [SerializeField] private Transform _laserFirePoint;
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _aggroDistance = 6f;
        [SerializeField] private float _attackDistance = 1.8f;
        [SerializeField] private float _immuneHealthThreshold = 0.4f;
        [SerializeField] private float _immuneDuration = 3f;
        [SerializeField] private float _laserCheckInterval = 0.15f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _laserChargeSound;
        [SerializeField] private AudioClip _laserShootSound;
        [SerializeField] private AudioClip _immuneActivateSound;
        [SerializeField] private AudioClip _moveSound;
        [SerializeField] private float _soundEffectVolume = 1f;

        [Header("Sound Variations")]
        [SerializeField] private AudioClip[] _attackSoundVariations;
        [SerializeField] private AudioClip[] _takeDamageSoundVariations;
        [SerializeField] private AudioClip[] _moveSoundVariations;
        [SerializeField] private bool _useSoundVariations = false;

        [Header("Sound Timing")]
        [SerializeField] private float _moveSoundInterval = 0.5f;
        [SerializeField] private float _laserChargeSoundDelay = 0.2f;

        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private AudioController _audioController;

        private BossStoneGolemState _currentState = BossStoneGolemState.Idle;
        private bool _isAggro;
        private bool _isInvulnerable;
        private bool _canCheckLaser = true;
        private bool _playerWasGrounded = true;
        private bool _wasAttackSuccessful;
        private float _immuneTimer;
        private float _lastLaserCheckTime;
        private float _lastMoveSoundTime;

        private bool _isVideoPlaying;
        private bool _hasPlayedIntro;

        private static readonly int StateParameterHash = Animator.StringToHash("state");

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioController = FindFirstObjectByType<AudioController>();

            if (_videoPanel != null) _videoPanel.SetActive(false);

            FindPlayerIfNeeded();
        }

        private void Update()
        {
            if (_currentState == BossStoneGolemState.Death) return;

            if (_isVideoPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SkipOrEndVideo();
                return;
            }

            FindPlayerIfNeeded();
            CheckAggro();

            if (_isAggro == false) return;

            UpdateCurrentState();
        }

        private void OnDestroy()
        {
            if (_isVideoPlaying) Time.timeScale = 1f;
            if (_introVideoPlayer != null) _introVideoPlayer.loopPointReached -= OnVideoFinished;
            if (_isAggro) StopBossMusic();
        }

        // --- ЛОГИКА ВИДЕО ---
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
        // --------------------

        public override void TakeDamage(int amount)
        {
            if (_isInvulnerable || _currentState == BossStoneGolemState.Death || amount <= 0) return;

            PlayTakeDamageSound();
            base.TakeDamage(amount);
        }

        public override void Die()
        {
            if (_currentState == BossStoneGolemState.Death) return;

            StopBossMusic();
            SwitchState(BossStoneGolemState.Death);
            base.Die();
            Destroy(gameObject, DestroyDelay);
        }

        public void AnimEvent_MeleeHit()
        {
            PlayAttackSound();
            _wasAttackSuccessful = ApplyMeleeDamage();
        }

        public void AnimEvent_AttackFinished()
        {
            if (_wasAttackSuccessful == false) PlayAttackMissSound();
            SwitchState(BossStoneGolemState.Idle);
        }

        public void AnimEvent_LaserCharge() { PlayLaserChargeSound(); }
        public void AnimEvent_LaserShoot() { PlayLaserShootSound(); ApplyLaserDamage(); }
        public void AnimEvent_LaserFinished() { _canCheckLaser = true; SwitchState(BossStoneGolemState.Idle); }
        public void AnimEvent_ImmuneFinished() { EndImmunity(); }

        private void UpdateCurrentState()
        {
            switch (_currentState)
            {
                case BossStoneGolemState.Idle: UpdateIdleState(); break;
                case BossStoneGolemState.Attack: StopMovement(); break;
                case BossStoneGolemState.Immune: UpdateImmuneState(); break;
                case BossStoneGolemState.LaserAttack: StopMovement(); break;
            }
        }

        private void CheckAggro()
        {
            if (_isAggro || _playerTransform == null) return;
            if (Vector2.Distance(transform.position, _playerTransform.position) > _aggroDistance) return;

            _isAggro = true;

            if (!_hasPlayedIntro && _introVideoPlayer != null && _videoPanel != null)
            {
                PlayIntroVideo();
            }

            StartBossMusic();
            SwitchState(BossStoneGolemState.Idle);
        }

        private void UpdateIdleState()
        {
            if (_playerTransform == null) return;

            MoveTowardsPlayer();
            PlayMoveSound();

            if (_isInvulnerable == false && (float)lives / MaximumHealth <= _immuneHealthThreshold)
            {
                SwitchState(BossStoneGolemState.Immune);
                return;
            }

            if (_canCheckLaser && Time.time - _lastLaserCheckTime > _laserCheckInterval && IsPlayerJumping())
            {
                _lastLaserCheckTime = Time.time;
                _canCheckLaser = false;
                SwitchState(BossStoneGolemState.LaserAttack);
                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= _attackDistance)
            {
                _wasAttackSuccessful = false;
                SwitchState(BossStoneGolemState.Attack);
            }
        }

        private void UpdateImmuneState()
        {
            _immuneTimer -= Time.deltaTime;
            if (_immuneTimer <= TimerEndThreshold) EndImmunity();
        }

        private void MoveTowardsPlayer()
        {
            if (_playerTransform == null || _rigidbody == null) return;

            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        private bool IsPlayerJumping()
        {
            if (_playerTransform == null) return false;

            Rigidbody2D playerRigidbody = _playerTransform.GetComponent<Rigidbody2D>();
            if (playerRigidbody == null) return false;

            bool isGrounded = Mathf.Abs(playerRigidbody.velocity.y) < GroundedVelocityThreshold;
            bool justJumped = _playerWasGrounded && isGrounded == false;
            _playerWasGrounded = isGrounded;
            return justJumped;
        }

        private bool ApplyMeleeDamage()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _attackDistance, _playerLayerMask);
            if (hit == null) return false;

            IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
            if (damageable == null) return false;

            damageable.TakeDamage(BossMeleeDamage);
            return true;
        }

        private void ApplyLaserDamage()
        {
            if (_laserFirePoint == null) return;

            float direction = Mathf.Sign(transform.localScale.x);
            Vector2 rayDirection = direction >= 0f ? Vector2.right : Vector2.left;

            RaycastHit2D hit = Physics2D.Raycast(_laserFirePoint.position, rayDirection, MaximumBeamLength, _playerLayerMask);
            if (hit.collider == null) return;

            IDamageable damageable = hit.collider.GetComponent<IDamageable>() ?? hit.collider.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(BossLaserDamage);
        }

        private void SwitchState(BossStoneGolemState newState)
        {
            _currentState = newState;
            if (_animator != null) _animator.SetInteger(StateParameterHash, (int)newState);

            switch (newState)
            {
                case BossStoneGolemState.Immune: StartImmunity(); break;
                case BossStoneGolemState.Death: StopMovement(); PlayDeathSound(); break;
                case BossStoneGolemState.Attack: StopMovement(); _wasAttackSuccessful = false; break;
                case BossStoneGolemState.LaserAttack: StopMovement(); Invoke(nameof(PlayLaserChargeSound), _laserChargeSoundDelay); break;
            }
        }

        private void StartImmunity()
        {
            _isInvulnerable = true;
            _immuneTimer = _immuneDuration;
            lives = Mathf.Max(lives, BossHealthAfterImmunity);
            PlayImmuneActivateSound();
            StopMovement();
        }

        private void EndImmunity() { _isInvulnerable = false; SwitchState(BossStoneGolemState.Idle); }

        private void FindPlayerIfNeeded()
        {
            if (_playerTransform != null) return;
            Hero hero = FindFirstObjectByType<Hero>();
            if (hero != null) _playerTransform = hero.transform;
        }

        private void StartBossMusic() { if (_audioController != null && _bossMusic != null) _audioController.PlayBossMusic(_bossMusic); }
        private void StopBossMusic() { _audioController?.StopBossMusic(); }
        private void StopMovement() { if (_rigidbody != null) _rigidbody.velocity = Vector2.zero; }

        private AudioClip GetRandomClip(AudioClip fallbackClip, AudioClip[] variations)
        {
            if (_useSoundVariations == false || variations == null || variations.Length == 0) return fallbackClip;
            return variations[UnityEngine.Random.Range(0, variations.Length)];
        }

        private void PlayAttackSound() => PlaySound(GetRandomClip(_attackSound, _attackSoundVariations), _soundEffectVolume);
        private void PlayAttackMissSound() => PlaySound(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier);
        private void PlayTakeDamageSound() => PlaySound(GetRandomClip(_takeDamageSound, _takeDamageSoundVariations), _soundEffectVolume);
        private void PlayDeathSound() => PlaySound(_deathSound, _soundEffectVolume);
        private void PlayLaserChargeSound() => PlaySound(_laserChargeSound, _soundEffectVolume * LaserSoundVolumeMultiplier);
        private void PlayLaserShootSound() => PlaySound(_laserShootSound, _soundEffectVolume * LaserSoundVolumeMultiplier);
        private void PlayImmuneActivateSound() => PlaySound(_immuneActivateSound, _soundEffectVolume);

        private void PlayMoveSound()
        {
            if (Time.time < _lastMoveSoundTime + _moveSoundInterval) return;
            _lastMoveSoundTime = Time.time;
            PlaySound(GetRandomClip(_moveSound, _moveSoundVariations), _soundEffectVolume * MoveSoundVolumeMultiplier);
        }

        private void PlaySound(AudioClip clip, float volumeMultiplier)
        {
            if (clip != null && _audioController != null) _audioController.PlayOneShotWithVolume(clip, volumeMultiplier);
        }

        private enum BossStoneGolemState { Idle = 0, Attack = 1, Immune = 2, LaserAttack = 3, Death = 4 }
    }
}