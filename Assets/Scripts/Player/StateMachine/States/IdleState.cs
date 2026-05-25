using Player.Input;
using Shared.Sensors;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class IdleState : IState
    {
        private const float HorizontalDeadZone = 0.1f;

        private readonly Hero _hero;
        private readonly IInputProvider _inputProvider;
        private readonly GroundCheck _groundCheck;
        private readonly EnemyScanner _enemyScanner;
        private readonly Rigidbody2D _rigidbody;

        public IdleState(Hero hero)
        {
            _hero = hero;
            _inputProvider = hero.GetComponent<IInputProvider>();
            _groundCheck = hero.GetComponentInChildren<GroundCheck>();
            _enemyScanner = hero.GetComponentInChildren<EnemyScanner>();
            _rigidbody = hero.Rigidbody;
        }

        public void Enter() => UpdateAnimationState();

        public void Tick()
        {
            UpdateAnimationState();

            if (!_groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<JumpState>();

                return;
            }

            HandleInput();
        }

        public void FixedTick() => StopHorizontalMovement();

        public void Exit() { }

        private void UpdateAnimationState()
        {
            States targetState = _enemyScanner.HasEnemyNearby ? States.Idle2 : States.Idle;

            _hero.AnimationService.SetState(targetState);
        }

        private void HandleInput()
        {
            if (Mathf.Abs(_inputProvider.HorizontalAxis) > HorizontalDeadZone)
            {
                _hero.StateMachine.Change<RunState>();

                return;
            }

            if (_inputProvider.IsJumpPressed)
            {
                _hero.StateMachine.Change<JumpState>();

                return;
            }

            if (_inputProvider.IsSlidePressed)
            {
                _hero.StateMachine.Change<SlideState>();

                return;
            }

            if (_inputProvider.IsAttackPressed)
            {
                _hero.StateMachine.PerformRandomAttack();

                return;
            }

            if (_inputProvider.IsSecondaryAttackPressed)
            {
                _hero.StateMachine.Change<Attack3State>();

                return;
            }
        }

        private void StopHorizontalMovement()
        {
            Vector2 velocity = _rigidbody.velocity;

            _rigidbody.velocity = new Vector2(0.0f, velocity.y);
        }
    }
}