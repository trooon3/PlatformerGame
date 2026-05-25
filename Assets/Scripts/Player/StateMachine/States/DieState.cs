using Player;
using Player.StateMachine;
using System.Collections;
using UnityEngine;
using YG;

public sealed class DieState : IState
{
    private const string DieAnimationName = "Die";
    private const float RestartAnimationThreshold = 0.55f;
    private const float RestartDelay = 1f;

    private readonly Hero _hero;
    private readonly Animator _animator;
    private bool _isRestarting;

    public DieState(Hero hero)
    {
        _hero = hero;
        _animator = hero.GetComponent<Animator>();
    }

    public void Enter()
    {
        _animator.SetInteger("state", (int)States.Hurt);

        _isRestarting = false;
    }

    public void Tick()
    {
        if (!_isRestarting && IsDieAnimationFinished())
        {
            _isRestarting = true;

            _hero.StartCoroutine(RestartGame());
        }
    }

    public void FixedTick() { }

    public void Exit() { }

    private bool IsDieAnimationFinished()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        YG2.InterstitialAdvShow();

        return stateInfo.IsName(DieAnimationName) && stateInfo.normalizedTime >= RestartAnimationThreshold;
    }

    private IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(RestartDelay);

        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}