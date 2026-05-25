using Player.Abilities;
using System.Collections;
using UnityEngine;

namespace Player.StateMachine
{
    public sealed class SlideState : IState
    {
        private const string SlideAnimationName = "Slide";

        private readonly Hero _hero;
        private readonly Slide _slideAbility;
        private Coroutine _slideCoroutine;

        public SlideState(Hero hero)
        {
            _hero = hero;

            _slideAbility = new Slide(
                _hero.Rigidbody,
                _hero.GetComponent<BoxCollider2D>(),
                _hero.Data.SlideSpeed,
                _hero.Data.SlideDuration,
                _hero.Data.SlideColliderSize,
                _hero.Data.SlideColliderOffset,
                _hero.Data.StandColliderSize,
                _hero.Data.StandColliderOffset);
        }

        public void Enter()
        {
            _hero.AnimationService.SetState(States.Slide);

            StartSlideCoroutine();
        }

        public void Tick() { }
        public void FixedTick() { }

        public void Exit()
        {
            StopSlideCoroutine();
        }

        private void StartSlideCoroutine()
        {
            _slideCoroutine = _hero.StartCoroutine(PerformSlide());
        }

        private void StopSlideCoroutine()
        {
            if (_slideCoroutine != null)
            {
                _hero.StopCoroutine(_slideCoroutine);

                _slideCoroutine = null;
            }
        }

        private IEnumerator PerformSlide()
        {
            yield return _slideAbility.Execute(_hero.FacingDirection);

            _hero.StateMachine.Change<IdleState>();
        }
    }
}