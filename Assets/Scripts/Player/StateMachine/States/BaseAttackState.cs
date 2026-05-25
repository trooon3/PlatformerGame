using Player.Abilities;
using UnityEngine;

namespace Player.StateMachine
{
    public abstract class BaseAttackState : IState, IAnimationEndHandler, IDamageDealer
    {
        private const string EnemyLayerName = "Enemy";
        private const float AttackRange = 1.4f;
        private const float AttackAngle = 180f;
        private const float FacingThreshold = 0.0f;

        protected readonly Hero _hero;
        protected readonly MeleeAttack _meleeAttack;
        protected readonly int _baseDamage;
        protected readonly States _animationState;
        protected bool _hasDealtDamage;

        protected BaseAttackState(Hero hero, Transform attackPoint, int baseDamage, States animationState)
        {
            _hero = hero;

            _meleeAttack = new MeleeAttack(
                attackOrigin: attackPoint,
                attackRange: AttackRange,
                attackAngle: AttackAngle,
                enemyLayerMask: LayerMask.GetMask(EnemyLayerName),
                hero: hero);

            _baseDamage = baseDamage;
            _animationState = animationState;
        }

        public virtual void Enter()
        {
            _hero.AnimationService.SetState(_animationState);
            _hasDealtDamage = false;
        }

        public virtual void Tick() { }

        public virtual void FixedTick() { }

        public virtual void Exit()
        {
            _hasDealtDamage = false;
        }

        public virtual void DealDamage()
        {
            if (_hasDealtDamage)
            {
                return;
            }

            _hasDealtDamage = true;

            Vector2 attackDirection = new Vector2(_hero.FacingDirection, FacingThreshold);

            bool hitConnected = _meleeAttack.Perform(_baseDamage, attackDirection);

            PlayAttackSound(hitConnected);
        }

        public virtual void OnAnimationEnd()
        {
            _hero.StateMachine.Change<IdleState>();
        }

        protected abstract void PlayAttackSound(bool hitConnected);
    }
}