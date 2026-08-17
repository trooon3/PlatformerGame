using Player.Input;
using Shared.Sensors;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class JumpState : IState
    {
        private const float VelocityThreshold = 0.1f;
        private const float HorizontalDeadZone = 0.1f;

        private readonly Hero _hero;
        private readonly IInputProvider _inputProvider;
        private readonly GroundCheck _groundCheck;
        private readonly Rigidbody2D _rigidbody;

        private bool _isJumpCutApplied;

        public JumpState(Hero hero)
        {
            _hero = hero;
            _inputProvider = hero.GetComponent<IInputProvider>();
            _groundCheck = hero.GetComponentInChildren<GroundCheck>();
            _rigidbody = hero.Rigidbody;
        }

        public void Enter()
        {
            _isJumpCutApplied = false;

            if (IsGrounded())
            {
                ApplyJumpForce();
            }

            _hero.AnimationService.SetState(States.Jump);
        }

        public void Tick()
        {
            if (_inputProvider != null && _inputProvider.IsAttackPressed && IsGrounded() == false)
            {
                _hero.StateMachine.Change<AirAttackState>();
                return;
            }

            if (_isJumpCutApplied == false && _rigidbody != null && _rigidbody.velocity.y > 0f)
            {
                if (_inputProvider != null && !_inputProvider.IsJumpHeld)
                {
                    _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _rigidbody.velocity.y * _hero.Data.JumpCutMultiplier);
                    _isJumpCutApplied = true;
                }
            }

            if (_rigidbody != null && _rigidbody.velocity.y <= VelocityThreshold && IsGrounded())
            {
                if (_inputProvider != null && Mathf.Abs(_inputProvider.HorizontalAxis) >= HorizontalDeadZone)
                {
                    _hero.StateMachine.Change<RunState>();
                }
                else
                {
                    _hero.StateMachine.Change<IdleState>();
                }
            }
        }

        public void FixedTick()
        {
            if (_inputProvider == null || _rigidbody == null)
            {
                return;
            }

            HandleMovement(_inputProvider.HorizontalAxis);
        }

        public void Exit() { }

        private bool IsGrounded()
        {
            return _groundCheck != null && _groundCheck.IsGrounded;
        }

        private void ApplyJumpForce()
        {
            if (_rigidbody == null)
            {
                return;
            }

            _rigidbody.AddForce(Vector2.up * _hero.Data.JumpForce, ForceMode2D.Impulse);
        }

        private void HandleMovement(float direction)
        {
            if (Mathf.Abs(direction) < HorizontalDeadZone)
            {
                StopHorizontalMovement();
                return;
            }

            if (_hero.IsTouchingWall() && Mathf.Sign(direction) == Mathf.Sign(_hero.FacingDirection))
            {
                StopHorizontalMovement();
                return;
            }

            MoveHorizontally(direction);
            _hero.SetFacing(direction);
        }

        private void StopHorizontalMovement()
        {
            _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
        }

        private void MoveHorizontally(float direction)
        {
            _rigidbody.velocity = new Vector2(direction * _hero.Data.MovementSpeed, _rigidbody.velocity.y);
        }
    }
}