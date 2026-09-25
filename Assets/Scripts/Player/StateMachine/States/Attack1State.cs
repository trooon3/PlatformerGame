using UnityEngine;

namespace Player.StateMachine
{
    public sealed class Attack1State : BaseAttackState
    {
        public Attack1State(Hero hero, Transform attackPoint)
            : base(hero, attackPoint, hero.Data.Attack1Damage, States.Attack) { }

        protected override void PlayAttackSound(bool hitConnected)
        {
            if (_hero.SfxPlayer == null || _hero.SoundConfiguration == null)
            {
                return;
            }

            if (hitConnected)
            {
                _hero.SfxPlayer.Play(_hero.SoundConfiguration.Attack1HitSound);
            }
            else
            {
                _hero.SfxPlayer.Play(_hero.SoundConfiguration.Attack1MissSound);
            }
        }
    }
}