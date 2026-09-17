using GeneralLogicEnemies;
using Shared.Damage;
using System;
using UnityEngine;
using UnityEngine.Video; 

namespace HardBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossKuznets : Entity, IDamageable
    {
        private const float TimerEndThreshold = 0f;
        private const int SpinAttackChance = 2;
        private const float DestroyDelay = 6.3f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;
        private const float SpinSoundVolumeMultiplier = 0.8f;

        [Header("Intro Video")]
        [SerializeField] private VideoPlayer _introVideoPlayer;
        [SerializeField] private GameObject _videoPanel;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Combat & Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _aggroDistance = 18f;
        [SerializeField] private float _meleeRange = 2f;
        [SerializeField] private float _spinRadius = 3f;
        [SerializeField] private float _spinDuration = 1.5f;
        [SerializeField] private float _tauntDuration = 1.2f;

        [Header("Damage")]
        [SerializeField] private int _meleeDamage = 1;
        [SerializeField] private int _spinDamage = 1;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayerMask;

        [Header("Attack Timing")]
        [SerializeField] private float _attackCooldown = 0.5f;

        [Header("Boss Music")]
        [SerializeField] private AudioClip _bossMusic;

        [Header("Boss Sound Effects")]
        [SerializeField] private AudioClip _attackSound;
        [SerializeField] private AudioClip _attackMissSound;
        [SerializeField] private AudioClip _takeDamageSound;
        [SerializeField] private AudioClip _deathSound;
        [SerializeField] private AudioClip _spinStartSound;
        [SerializeField] private AudioClip _spinLoopSound;
        [SerializeField] private AudioClip _spinHitSound;
        [SerializeField] private AudioClip _tauntSound;
        [SerializeField] private AudioClip _moveSound;
        [SerializeField] private AudioClip _aggroSound;
        [SerializeField] private float _soundEffectVolume = 1f;

        [Header("Sound Timing")]
        [SerializeField] private float _moveSoundInterval = 0.6f;
        [SerializeField] private float _spinSoundInterval = 0.3f;

        private Rigidbody2D _rigidbody;
        private Animator _animator;
        private AudioController _audioController;

        private BossState _currentState = BossState.Idle;
        private bool _isAggro;
        private float _stateTimer;
        private bool _shouldTaunt;
        private float _lastAttackTime;
        private float _lastMoveSoundTime;
        private float _lastSpinSoundTime;
        private bool _wasAttackSuccessful;

        private bool _isVideoPlaying;
        private bool _hasPlayedIntro;

        public bool IsAggro => _isAggro;

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
            if (_currentState == BossState.Death) return;

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
            if (_currentState == BossState.Death || amount <= 0) return;

            PlayTakeDamageSound();
            base.TakeDamage(amount);
        }

        public override void Die()
        {
            if (_currentState == BossState.Death) return;

            StopBossMusic();
            PlayDeathSound();
            SwitchState(BossState.Death);
            base.Die();
            Destroy(gameObject, DestroyDelay);
        }

        public void AnimEvent_MeleeHit()
        {
            PlayAttackSound();
            _wasAttackSuccessful = ApplyDamageInRadius(_meleeRange, _meleeDamage);
        }

        public void AnimEvent_SpinTick()
        {
            if (Time.time >= _lastSpinSoundTime + _spinSoundInterval)
            {
                PlaySpinHitSound();
                _lastSpinSoundTime = Time.time;
            }
            ApplyDamageInRadius(_spinRadius, _spinDamage);
        }

        public void AnimEvent_AttackFinished()
        {
            if (_wasAttackSuccessful == false) PlayAttackMissSound();
            _shouldTaunt = true;
            SwitchState(BossState.Idle);
        }

        public void AnimEvent_SpinFinished()
        {
            _shouldTaunt = true;
            SwitchState(BossState.Idle);
        }

        public void AnimEvent_TauntFinished() { SwitchState(BossState.Idle); }
        public void AnimEvent_AttackStart() { PlayAttackSound(); }

        private void UpdateCurrentState()
        {
            switch (_currentState)
            {
                case BossState.Idle: UpdateIdleState(); break;
                case BossState.Walk: UpdateWalkState(); break;
                case BossState.SpinAttack: UpdateSpinAttackState(); break;
                case BossState.Taunt: UpdateTauntState(); break;
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

            PlayAggroSound();
            StartBossMusic();
            SwitchState(BossState.Idle);
        }

        private void UpdateIdleState()
        {
            if (_playerTransform == null) return;

            if (_shouldTaunt)
            {
                _shouldTaunt = false;
                SwitchState(BossState.Taunt);
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _meleeRange && Time.time >= _lastAttackTime + _attackCooldown)
            {
                _lastAttackTime = Time.time;

                if (UnityEngine.Random.Range(0, SpinAttackChance) == 0) SwitchState(BossState.SpinAttack);
                else SwitchState(BossState.Attack);

                return;
            }

            SwitchState(BossState.Walk);
        }

        private void UpdateWalkState()
        {
            MoveTowardsPlayer();
            PlayMoveSound();

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            if (distanceToPlayer <= _meleeRange) SwitchState(BossState.Idle);
        }

        private void UpdateSpinAttackState()
        {
            _stateTimer -= Time.deltaTime;
            MoveTowardsPlayer();
            if (_stateTimer <= TimerEndThreshold) SwitchState(BossState.Idle);
        }

        private void UpdateTauntState()
        {
            _stateTimer -= Time.deltaTime;
            StopMovement();
            if (_stateTimer <= TimerEndThreshold) SwitchState(BossState.Idle);
        }

        private void MoveTowardsPlayer()
        {
            if (_playerTransform == null || _rigidbody == null) return;
            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);
            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);
            transform.localScale = new Vector3(direction, 1f, 1f);
        }

        private bool ApplyDamageInRadius(float radius, int damage)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, _playerLayerMask);
            bool hitSuccessful = false;

            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>() ?? hit.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;

                damageable.TakeDamage(damage);
                hitSuccessful = true;
            }
            return hitSuccessful;
        }

        private void SwitchState(BossState nextState)
        {
            _currentState = nextState;
            if (_animator != null) _animator.SetInteger(StateParameterHash, (int)nextState);

            switch (nextState)
            {
                case BossState.SpinAttack: _stateTimer = _spinDuration; _lastSpinSoundTime = 0f; PlaySpinStartSound(); break;
                case BossState.Taunt: _stateTimer = _tauntDuration; PlayTauntSound(); break;
                case BossState.Death: StopMovement(); break;
                case BossState.Attack: _wasAttackSuccessful = false; StopMovement(); break;
            }
        }

        private void FindPlayerIfNeeded()
        {
            if (_playerTransform != null) return;
            Hero hero = FindFirstObjectByType<Hero>();
            if (hero != null) _playerTransform = hero.transform;
        }

        private void StartBossMusic() { if (_audioController != null && _bossMusic != null) _audioController.PlayBossMusic(_bossMusic); }
        private void StopBossMusic() { _audioController?.StopBossMusic(); }
        private void StopMovement() { if (_rigidbody != null) _rigidbody.velocity = Vector2.zero; }

        private void PlayAttackSound() => PlaySound(_attackSound, _soundEffectVolume);
        private void PlayAttackMissSound() => PlaySound(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier);
        private void PlayTakeDamageSound() => PlaySound(_takeDamageSound, _soundEffectVolume);
        private void PlayDeathSound() => PlaySound(_deathSound, _soundEffectVolume);
        private void PlaySpinStartSound() => PlaySound(_spinStartSound, _soundEffectVolume);
        private void PlaySpinHitSound() => PlaySound(_spinHitSound, _soundEffectVolume * SpinSoundVolumeMultiplier);
        private void PlayTauntSound() => PlaySound(_tauntSound, _soundEffectVolume);
        private void PlayAggroSound() => PlaySound(_aggroSound, _soundEffectVolume);

        private void PlayMoveSound()
        {
            if (Time.time < _lastMoveSoundTime + _moveSoundInterval) return;
            _lastMoveSoundTime = Time.time;
            PlaySound(_moveSound, _soundEffectVolume);
        }

        private void PlaySound(AudioClip clip, float volumeMultiplier)
        {
            if (clip != null && _audioController != null) _audioController.PlayOneShotWithVolume(clip, volumeMultiplier);
        }

        private enum BossState { Idle = 0, Attack = 1, Walk = 2, SpinAttack = 3, Taunt = 4, Death = 5 }
    }
}