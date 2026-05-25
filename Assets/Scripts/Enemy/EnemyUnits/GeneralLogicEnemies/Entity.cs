using UnityEngine;

namespace GeneralLogicEnemies
{
    public abstract class Entity : MonoBehaviour
    {
        private const int DefaultMaxLives = 6;
        private const float DefaultDeathAnimationDelay = 1f;
        private const float GrayColorValue = 0.5f;
        private const float GrayAlphaValue = 0.7f;

        [SerializeField] protected int lives = DefaultMaxLives;
        [SerializeField] protected float deathAnimationDelay = DefaultDeathAnimationDelay;

        public int MaxLives { get; protected set; } = DefaultMaxLives;
        public bool IsDead { get; protected set; }
        public bool IsAlive => lives > 0 && !IsDead;

        public event System.Action<Entity> OnEntityDeath;

        protected virtual void Awake()
        {
            IsDead = false;
        }

        public virtual void TakeDamage(int amount)
        {
            int deathThreshold = 1;

            if (IsDead)
            {
                return;
            }

            lives -= amount;

            if (lives < deathThreshold)
            {
                Die();
            }
        }

        public virtual void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;

            OnEntityDeath?.Invoke(this);

            DisablePhysics();
            PlayDeathAnimation();
            MarkEnemyAsKilled();

            Invoke(nameof(DestroyAfterAnimation), deathAnimationDelay);
        }

        protected virtual void PlayDeathAnimation()
        {
            Animator animator = GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(GrayColorValue, GrayColorValue, GrayColorValue, GrayAlphaValue);
            }
        }

        protected virtual void DisablePhysics()
        {
            Collider2D collider = GetComponent<Collider2D>();

            if (collider != null)
            {
                collider.enabled = false;
            }

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();

            if (rigidbody != null)
            {
                rigidbody.simulated = false;
            }
        }

        protected virtual void MarkEnemyAsKilled()
        {
            EnemyRegistration enemyRegistration = GetComponent<EnemyRegistration>();

            if (enemyRegistration != null)
            {
                string enemyId = enemyRegistration.GetEnemyId();

                if (!string.IsNullOrEmpty(enemyId))
                {
                    if (EnemyManager.Instance != null)
                    {
                        EnemyManager.Instance.MarkEnemyKilled(enemyId);
                    }
                }
            }
        }

        protected virtual void DestroyAfterAnimation()
        {
            Destroy(gameObject);
        }
    }
}