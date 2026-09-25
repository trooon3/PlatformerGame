using UnityEngine;

namespace Player.StateMachine
{
    public sealed class Attack3State : BaseAttackState
    {
        public Attack3State(Hero hero, Transform attackPoint)
            : base(hero, attackPoint, hero.Data.SuperAttackDamage, States.Attack3) { }

        protected override void PlayAttackSound(bool hitConnected)
        {
            if (_hero.SfxPlayer == null || _hero.SoundConfiguration == null)
            {
                return;
            }

            if (hitConnected)
            {
                _hero.SfxPlayer.Play(_hero.SoundConfiguration.Attack3HitSound);
            }
            else
            {
                _hero.SfxPlayer.Play(_hero.SoundConfiguration.Attack3MissSound);
            }
        }
    }
}