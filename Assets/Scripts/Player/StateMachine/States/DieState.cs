using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;
using GameLogic; 

namespace Player.StateMachine
{
    public sealed class DieState : IState
    {
        private const float AnimationEndThreshold = 0.2f;
        private const float RestartDelay = 0.9f;

        private readonly Hero _hero;
        private readonly Rigidbody2D _rigidbody;
        private readonly Animator _animator;

        private bool _isRestarting;

        public DieState(Hero hero)
        {
            _hero = hero;
            _rigidbody = hero.GetComponent<Rigidbody2D>();
            _animator = hero.GetComponent<Animator>();
        }

        public void Enter()
        {
            if (_isRestarting) return;
            _isRestarting = true;

            FreezeHero();
            _hero.AnimationService.SetState(States.Die);

            _hero.StartCoroutine(RestartSequence());
        }

        private void FreezeHero()
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
            _rigidbody.gravityScale = 0f;
            _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        private IEnumerator RestartSequence()
        {
            float timer = 0f;
            while (timer < 2.5f)
            {
                AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName("Die") && info.normalizedTime >= AnimationEndThreshold)
                {
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            ShowInterstitialAdSafe();

            yield return new WaitForSecondsRealtime(RestartDelay);

            Time.timeScale = 1f;

            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
            {
                SaveSystem.Instance.LoadGameData();

                GameSaveData diskData = SaveSystem.Instance.CurrentSave;
                if (diskData != null)
                {
                    diskData.coins.isInitialized = true;

                    if (WalletManager.Instance != null)
                    {
                        WalletManager.Instance.LoadFromSaveData(diskData.coins);
                    }

                    if (PersistentWallet.Instance != null)
                    {
                        PersistentWallet.Instance.LoadFromSaveData(diskData.coins);
                    }
                }
            }

            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.ResetAllEnemies();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ShowInterstitialAdSafe()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            YG2.InterstitialAdvShow();
#endif
        }

        public void Tick() { }
        public void FixedTick() { }
        public void Exit() { }
    }
}