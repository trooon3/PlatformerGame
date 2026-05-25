using Player.Input;
using Shared.Sensors;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class JumpState : IState
    {
        private const float VelocityThreshold = 0.0f;

        private readonly Hero _hero;
        private readonly IInputProvider _inputProvider;
        private readonly GroundCheck _groundCheck;
        private readonly Rigidbody2D _rigidbody;

        public JumpState(Hero hero)
        {
            _hero = hero;

            _inputProvider = hero.GetComponent<IInputProvider>();
            _groundCheck = hero.GetComponentInChildren<GroundCheck>();

            _rigidbody = hero.Rigidbody;
        }

        public void Enter()
        {
            if (_groundCheck.IsGrounded)
            {
                ApplyJumpForce();
            }

            _hero.AnimationService.SetState(States.Jump);
        }

        public void Tick()
        {
            if (_inputProvider.IsAttackPressed && !_groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<AirAttackState>();

                return;
            }

            if (_rigidbody.velocity.y <= VelocityThreshold && _groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<IdleState>();
            }
        }

        public void FixedTick()
        {
            float horizontalInput = _inputProvider.HorizontalAxis;

            HandleMovement(horizontalInput);
        }

        public void Exit() { }

        private void ApplyJumpForce() => _rigidbody.AddForce(Vector2.up * _hero.Data.JumpForce, ForceMode2D.Impulse);

        private void HandleMovement(float direction)
        {
            if (_hero.IsTouchingWall() && Mathf.Sign(direction) == Mathf.Sign(_hero.FacingDirection))
            {
                StopHorizontalMovement();
            }
            else
            {
                MoveHorizontally(direction);

                _hero.SetFacing(direction);
            }
        }

        private void StopHorizontalMovement() => _rigidbody.velocity = new Vector2(0.0f, _rigidbody.velocity.y);
        private void MoveHorizontally(float direction) => _rigidbody.velocity = new Vector2(direction * _hero.Data.MovementSpeed, _rigidbody.velocity.y);
    }
}