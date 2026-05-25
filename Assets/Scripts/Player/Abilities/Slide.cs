using System.Collections;
using UnityEngine;

namespace Player.Abilities
{
    public sealed class Slide
    {
        private readonly Rigidbody2D _rigidbody;
        private readonly BoxCollider2D _boxCollider;
        private readonly Vector2 _slideColliderSize;
        private readonly Vector2 _slideColliderOffset;
        private readonly Vector2 _standColliderSize;
        private readonly Vector2 _standColliderOffset;
        private readonly float _slideSpeed;
        private readonly float _slideDuration;

        public Slide(Rigidbody2D rigidbody,
                     BoxCollider2D boxCollider,
                     float slideSpeed,
                     float slideDuration,
                     Vector2 slideColliderSize,
                     Vector2 slideColliderOffset,
                     Vector2 standColliderSize,
                     Vector2 standColliderOffset)
        {
            _rigidbody = rigidbody;
            _boxCollider = boxCollider;
            _slideSpeed = slideSpeed;
            _slideDuration = slideDuration;
            _slideColliderSize = slideColliderSize;
            _slideColliderOffset = slideColliderOffset;
            _standColliderSize = standColliderSize;
            _standColliderOffset = standColliderOffset;
        }

        public IEnumerator Execute(float rawDirection)
        {
            int direction = CalculateSlideDirection(rawDirection);

            ApplySlideColliderSettings(direction);

            yield return PerformSlideMovement(direction);

            ResetColliderToStanding();
            StopHorizontalMovement();
        }

        private int CalculateSlideDirection(float rawDirection)
        {
            return Mathf.RoundToInt(Mathf.Sign(rawDirection));
        }

        private void ApplySlideColliderSettings(int direction)
        {
            _boxCollider.size = _slideColliderSize;

            _boxCollider.offset = new Vector2(
                _slideColliderOffset.x * direction,
                _slideColliderOffset.y
            );
        }

        private IEnumerator PerformSlideMovement(int direction)
        {
            float elapsedTime = 0f;

            while (elapsedTime < _slideDuration)
            {
                ApplySlideVelocity(direction);

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }

        private void ApplySlideVelocity(int direction)
        {
            _rigidbody.velocity = new Vector2(direction * _slideSpeed, _rigidbody.velocity.y);
        }

        private void ResetColliderToStanding()
        {
            _boxCollider.size = _standColliderSize;
            _boxCollider.offset = _standColliderOffset;
        }

        private void StopHorizontalMovement()
        {
            _rigidbody.velocity = new Vector2(0f, _rigidbody.velocity.y);
        }
    }
}