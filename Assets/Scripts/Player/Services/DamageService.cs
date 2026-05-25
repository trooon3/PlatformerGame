using Player.StateMachine;
using UnityEngine;

namespace Player
{
    public sealed class DamageService
    {
        private readonly Hero _hero;
        private readonly HealthManager _healthManager;

        public DamageService(Hero hero, HealthManager healthManager)
        {
            _hero = hero;
            _healthManager = healthManager;
        }

        public void ApplyDamage(int damageAmount)
        {
            int lastChanceHealth = 1;

            if (_healthManager == null)
            {
                return;
            }

            int currentHealth = _healthManager.CurrentHealth;

            if (_hero.AbilityManager != null && _hero.AbilityManager.IsLastChanceActive & damageAmount >= currentHealth)
            {
                _hero.AbilityManager.UseLastChance();

                _hero.SetHealth(lastChanceHealth); 

                return;
            }

            _healthManager.TakeDamage(damageAmount);

            HandleDamageResponse(currentHealth, damageAmount);
        }

        private void HandleDamageResponse(int currentHealth, int damageAmount)
        {
            int fatalHealthThreshold = 0;

            if (currentHealth - damageAmount <= fatalHealthThreshold)
            {
                _hero.StateMachine.Change<DieState>();
            }
            else
            {
                _hero.StateMachine.Change<HurtState>();
            }
        }
    }
}