using Player.Input;
using Shared.Sensors;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class RunState : IState
    {
        private const float HorizontalDeadZone = 0.1f;

        private readonly Hero _hero;
        private readonly IInputProvider _inputProvider;
        private readonly GroundCheck _groundCheck;
        private readonly Rigidbody2D _rigidbody;

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
            if (_inputProvider.IsJumpPressed && _groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<JumpState>();

                return;
            }

            if (_inputProvider.IsSlidePressed && _groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<SlideState>();

                return;
            }

            if (Mathf.Abs(_inputProvider.HorizontalAxis) < HorizontalDeadZone)
            {
                _hero.StateMachine.Change<IdleState>();

                return;
            }

            if (!_groundCheck.IsGrounded)
            {
                _hero.StateMachine.Change<JumpState>();
            }
        }

        public void FixedTick()
        {
            float horizontalInput = _inputProvider.HorizontalAxis;

            if (horizontalInput == 0.0f)
            {
                StopHorizontalMovement();
            }
            else
            {
                MoveHorizontally(horizontalInput);

                _hero.SetFacing(horizontalInput);
            }
        }

        public void Exit() { }

        private void StopHorizontalMovement()
        {
            _rigidbody.velocity = new Vector2(0.0f, _rigidbody.velocity.y);
        }

        private void MoveHorizontally(float direction)
        {
            _rigidbody.velocity = new Vector2(direction * _hero.Data.MovementSpeed, _rigidbody.velocity.y);
        }
    }
}