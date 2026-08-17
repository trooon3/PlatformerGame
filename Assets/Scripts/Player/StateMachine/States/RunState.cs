using Player.Input;
using Shared.Sensors;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class RunState : IState
    {
        private const float HorizontalDeadZone = 0.1f;
        private const float SprintMultiplier = 3.6f;

        private readonly Hero _hero;
        private readonly IInputProvider _inputProvider;
        private readonly GroundCheck _groundCheck;
        private readonly Rigidbody2D _rigidbody;

        private bool _isSprinting;

        public RunState(Hero hero)
        {
            _hero = hero;
            _inputProvider = hero.GetComponent<IInputProvider>();
            _groundCheck = hero.GetComponentInChildren<GroundCheck>();
            _rigidbody = hero.Rigidbody;
        }

        public void Enter()
        {
            _hero.AnimationService.SetState(States.Run);
        }

        public void Tick()
        {
            if (_inputProvider == null)
            {
                return;
            }

            if (_inputProvider.IsSprintPressed)
            {
                _isSprinting = !_isSprinting;
            }

            if (_inputProvider.IsJumpPressed && IsGrounded())
            {
                _hero.StateMachine.Change<JumpState>();
                return;
            }

            if (_inputProvider.IsSlidePressed && IsGrounded())
            {
                _hero.StateMachine.Change<SlideState>();
                return;
            }

            if (Mathf.Abs(_inputProvider.HorizontalAxis) < HorizontalDeadZone)
            {
                _hero.StateMachine.Change<IdleState>();
                return;
            }

            if (IsGrounded() == false)
            {
                _hero.StateMachine.Change<JumpState>();
            }
        }

        public void FixedTick()
        {
            if (_inputProvider == null || _rigidbody == null)
            {
                return;
            }

            float horizontalInput = _inputProvider.HorizontalAxis;

            if (Mathf.Abs(horizontalInput) < HorizontalDeadZone)
            {
                StopHorizontalMovement();
            }
            else
            {
                MoveHorizontally(horizontalInput);
                _hero.SetFacing(horizontalInput);
            }
        }

        public void Exit()
        {
        }

        private bool IsGrounded()
        {
            return _groundCheck != null && _groundCheck.IsGrounded;
        }

        private void StopHorizontalMovement()
        {
            _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
        }

        private void MoveHorizontally(float direction)
        {
            float currentSpeed = _isSprinting
                ? _hero.Data.MovementSpeed * SprintMultiplier
                : _hero.Data.MovementSpeed;

            _rigidbody.velocity = new Vector2(direction * currentSpeed, _rigidbody.velocity.y);
        }
    }
}