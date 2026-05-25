using GeneralLogicEnemies;
using Shared.Damage;
using System;
using UnityEngine;

namespace HardBossLogic
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class BossKuznets : Entity, IDamageable
    {
        private const float TimerEndThreshold = 0.0f;
        private const int SpinAttackChance = 2;
        private const float DestroyDelay = 6.3f;
        private const float AttackMissSoundVolumeMultiplier = 0.7f;
        private const float SpinSoundVolumeMultiplier = 0.8f;

        [Header("References")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private LayerMask _playerLayerMask;

        [Header("Combat & Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _aggroDistance = 8f;
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
        private BossHealthBar _healthBar;

        private BossState _currentState;
        private bool _isAggro;
        private float _stateTimer;
        private readonly Action[] _stateUpdateMethods;
        private bool _shouldTaunt;
        private float _lastAttackTime;
        private float _lastMoveSoundTime;
        private float _lastSpinSoundTime;
        private bool _wasAttackSuccessful;
        private bool _isPlayingSpinLoop;

        public bool IsAggro => _isAggro;

        public BossKuznets()
        {
            _currentState = BossState.Idle;
            _stateUpdateMethods = new Action[Enum.GetValues(typeof(BossState)).Length];

            _stateUpdateMethods[(int)BossState.Idle] = UpdateIdleState;
            _stateUpdateMethods[(int)BossState.Attack] = UpdateNothing;
            _stateUpdateMethods[(int)BossState.Walk] = UpdateWalkState;
            _stateUpdateMethods[(int)BossState.SpinAttack] = UpdateSpinAttackState;
            _stateUpdateMethods[(int)BossState.Taunt] = UpdateTauntState;
            _stateUpdateMethods[(int)BossState.Death] = () => { };
        }

        private void Start()
        {
            _healthBar = GetComponent<BossHealthBar>();
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _audioController = FindFirstObjectByType<AudioController>();
        }

        private void Update()
        {
            if (_currentState == BossState.Death)
            {
                return;
            }

            CheckAggro();

            if (!_isAggro)
            {
                return;
            }

            _stateUpdateMethods[(int)_currentState]?.Invoke();
        }

        public override void TakeDamage(int amount)
        {
            PlayTakeDamageSound();

            base.TakeDamage(amount);
        }

        public override void Die()
        {
            StopBossMusic();
            PlayDeathSound();
            SwitchState(BossState.Death);

            base.Die();

            Destroy(gameObject, DestroyDelay);
        }

        public void AnimEvent_MeleeHit()
        {
            PlayAttackSound();

            bool hitSuccessful = ApplyDamageInRadius(_meleeRange, _meleeDamage);

            _wasAttackSuccessful = hitSuccessful;
        }

        public void AnimEvent_SpinTick()
        {
            PlaySpinHitSound();
            ApplyDamageInRadius(_spinRadius, _spinDamage);
        }

        public void AnimEvent_AttackFinished()
        {
            if (!_wasAttackSuccessful)
            {
                PlayAttackMissSound();
            }

            _shouldTaunt = true;

            SwitchState(BossState.Idle);
        }

        public void AnimEvent_SpinFinished()
        {
            _shouldTaunt = true;

            SwitchState(BossState.Idle);
        }

        public void AnimEvent_TauntFinished()
        {
            SwitchState(BossState.Idle);
        }

        public void AnimEvent_AttackStart()
        {
            PlayAttackSound();
        }

        private void CheckAggro()
        {
            if (_isAggro)
            {
                return;
            }

            if (Vector2.Distance(transform.position, _playerTransform.position) <= _aggroDistance)
            {
                _isAggro = true;

                StartBossMusic();

                SwitchState(BossState.Idle);
            }
        }

        private void StartBossMusic()
        {
            if (_audioController != null && _bossMusic != null)
            {
                _audioController.PlayBossMusic(_bossMusic);
            }
        }

        private void StopBossMusic()
        {
            _audioController?.StopBossMusic();
        }

        private void SwitchState(BossState nextState)
        {
            _currentState = nextState;
            _animator.SetInteger("state", (int)nextState);

            switch (nextState)
            {
                case BossState.SpinAttack:
                    _stateTimer = _spinDuration;

                    break;

                case BossState.Taunt:
                    _stateTimer = _tauntDuration;

                    break;

                case BossState.Death:
                    _rigidbody.velocity = Vector2.zero;

                    break;

                case BossState.Attack:
                    _wasAttackSuccessful = false;

                    break;
            }
        }

        private void MoveTowardsPlayer()
        {
             float scaleY = 1f;
             float scaleZ = 1f;

            float direction = Mathf.Sign(_playerTransform.position.x - transform.position.x);

            _rigidbody.velocity = new Vector2(direction * _moveSpeed, _rigidbody.velocity.y);

            transform.localScale = new Vector3(direction, scaleY, scaleZ);
        }

        private bool IsGrounded()
        {
            return Physics2D.OverlapCircle(_groundCheckPoint.position, _groundCheckRadius, _groundLayerMask);
        }

        private bool ApplyDamageInRadius(float radius, int damage)
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, _playerLayerMask);

            bool hitSuccess = hit?.GetComponent<IDamageable>() != null;

            if (hitSuccess)
            {
                hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            }

            return hitSuccess;
        }

        private void UpdateIdleState()
        {
            int randomMinimumValue = 0;

            if (_shouldTaunt)
            {
                _shouldTaunt = false;

                _lastAttackTime = Time.time;

                SwitchState(BossState.Taunt);

                return;
            }

            if (Time.time - _lastAttackTime < _attackCooldown)
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _meleeRange)
            {
                _lastAttackTime = Time.time;

                bool shouldSpin = UnityEngine.Random.Range(randomMinimumValue, SpinAttackChance) == 0;

                SwitchState(shouldSpin ? BossState.SpinAttack : BossState.Attack);

                return;
            }

            SwitchState(BossState.Walk);
        }

        private void UpdateWalkState()
        {
            int randomMinimumValue = 0;

            if (!IsGrounded())
            {
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

            if (distanceToPlayer <= _meleeRange)
            {
                bool shouldSpin = UnityEngine.Random.Range(randomMinimumValue, SpinAttackChance) == 0;

                SwitchState(shouldSpin ? BossState.SpinAttack : BossState.Attack);

                return;
            }

            MoveTowardsPlayer();
        }

        private void UpdateSpinAttackState()
        {
            _stateTimer -= Time.deltaTime;

            if (_isPlayingSpinLoop && Time.time - _lastSpinSoundTime >= _spinSoundInterval)
            {
                PlaySpinHitSound();

                _lastSpinSoundTime = Time.time;
            }

            if (_stateTimer <= TimerEndThreshold)
            {
                SwitchState(BossState.Idle);
            }
        }

        private void UpdateTauntState()
        {
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= TimerEndThreshold)
            {
                SwitchState(BossState.Idle);
            }
        }

        private void PlayAttackSound()
        {
            if (_attackSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackSound, _soundEffectVolume);
            }
        }

        private void PlayAttackMissSound()
        {
            if (_attackMissSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_attackMissSound, _soundEffectVolume * AttackMissSoundVolumeMultiplier);
            }
        }

        private void PlayTakeDamageSound()
        {
            if (_takeDamageSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_takeDamageSound, _soundEffectVolume);
            }
        }

        private void PlayDeathSound()
        {
            if (_deathSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_deathSound, _soundEffectVolume);
            }
        }

        private void PlaySpinHitSound()
        {
            if (_spinHitSound != null && _audioController != null)
            {
                _audioController.PlayOneShotWithVolume(_spinHitSound, _soundEffectVolume * SpinSoundVolumeMultiplier);
            }
        }

        private void UpdateNothing() { }

        private void OnDestroy()
        {
            if (_isAggro)
            {
                StopBossMusic();
            }
        }

        public enum BossState
        {
            Idle,
            Attack,
            Walk,
            SpinAttack,
            Taunt,
            Death
        }
    }
}