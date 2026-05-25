using Player;
using Player.StateMachine;
using Shared.Sensors;
using UnityEngine;

public sealed class AirAttackState : BaseAttackState
{
    private readonly GroundCheck _groundCheck;

    public AirAttackState(Hero hero, Transform attackPoint)
        : base(hero, attackPoint, hero.Data.AirAttackDamage, States.AirAttack)
    {
        _groundCheck = hero.GetComponentInChildren<GroundCheck>();
    }

    public override void Tick()
    {
        if (_hero.AnimationService.IsAnimationFinished(States.AirAttack.ToString()))
        {
            _hero.StateMachine.Change<JumpState>();
        }
    }

    public override void OnAnimationEnd()
    {
        if (_groundCheck.IsGrounded)
        {
            _hero.StateMachine.Change<IdleState>();
        }
        else
        {
            _hero.StateMachine.Change<JumpState>();
        }
    }

    protected override void PlayAttackSound(bool hitConnected)
    {
        if (_hero.AudioController == null)
        {
            return;
        }

        if (hitConnected)
        {
            _hero.AudioController.PlayAirAttackHitSound();
        }
        else
        {
            _hero.AudioController.PlayAirAttackMissSound();
        }
    }
}