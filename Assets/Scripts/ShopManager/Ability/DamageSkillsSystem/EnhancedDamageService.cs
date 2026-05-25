using Shared.Damage;
using UnityEngine;

namespace Player
{
    public sealed class EnhancedDamageService
    {
        private readonly Hero _hero;
        private readonly DamageCalculator _damageCalculator;

        public EnhancedDamageService(Hero hero)
        {
            _hero = hero;
            _damageCalculator = new DamageCalculator(_hero.AbilityManager);
        }

        public void ApplyDamageToTarget(int baseDamage, GameObject target)
        {
            if (target == null)
            {
                return;
            }

            IDamageable damageable = target.GetComponent<IDamageable>() ?? target.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                int finalDamage = _damageCalculator.CalculateDamageWithBonuses(baseDamage, target);
                damageable.TakeDamage(finalDamage);

                if (finalDamage > baseDamage)
                {
                    ShowBonusDamageEffect(target);
                }
            }
        }

        private void ShowBonusDamageEffect(GameObject target)
        {
            CreateDamageText(target, "Болото x2!");
        }

        private void CreateDamageText(GameObject target, string text)
        {
            float verticalOffset = 1.5f;
            string damageTextObjectName = "BonusDamageText";
            int textFontSize = 20;
            float textLifetime = 1f;

            GameObject textObj = new GameObject(damageTextObjectName);
            textObj.transform.position = target.transform.position + Vector3.up * verticalOffset;

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = Color.green;
            textMesh.fontSize = textFontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;

            Object.Destroy(textObj, textLifetime);
        }
    }
}