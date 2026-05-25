using GeneralLogicEnemies;
using UnityEngine;

namespace Player.Abilities
{
    public sealed class MeleeAttack
    {
        private const float AngleMultiplier = 0.5f;

        private readonly Transform _attackOrigin;
        private readonly LayerMask _enemyLayerMask;
        private readonly float _attackRange;
        private readonly float _attackAngleHalf;
        private readonly Hero _hero;
        private OnePunchManSystem _onePunchSystem;
        private bool _onePunchSystemChecked = false;
        private DamageBonusService _damageBonusService;
        private bool _serviceInitialized = false;

        public MeleeAttack(Transform attackOrigin, float attackRange, float attackAngle, LayerMask enemyLayerMask, Hero hero)
        {
            _attackOrigin = attackOrigin;
            _attackRange = attackRange;
            _enemyLayerMask = enemyLayerMask;
            _attackAngleHalf = attackAngle * AngleMultiplier;
            _hero = hero;
        }

        private void InitializeDamageBonusService()
        {
            if (_serviceInitialized)
            {
                return;
            }


            if (_hero == null)
            {
                return;
            }

            if (_hero.AbilityManager == null)
            {

                var abilityManager = _hero.GetComponent<AbilityManager>() ?? _hero.AbilityManager;

                if (abilityManager == null)
                {
                    return;
                }
            }

            if (_hero.AbilityManager != null)
            {
                _damageBonusService = new DamageBonusService(_hero.AbilityManager);

                _serviceInitialized = true;
            }
            else
            {
            }
        }

        private OnePunchManSystem GetOnePunchSystem()
        {
            if (!_onePunchSystemChecked && _hero != null)
            {
                _onePunchSystem = _hero.GetComponent<OnePunchManSystem>();

                _onePunchSystemChecked = true;
            }

            return _onePunchSystem;
        }

        public bool Perform(int baseDamage, Vector2 attackDirection)
        {
            InitializeDamageBonusService();

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(_attackOrigin.position, _attackRange, _enemyLayerMask);

            bool hitConnected = false;

            OnePunchManSystem onePunchSystem = GetOnePunchSystem();

            foreach (Collider2D collider in hitColliders)
            {
                if (IsWithinAttackAngle(collider.transform.position, attackDirection))
                {
                    GameObject enemyObject = collider.gameObject;

                    Entity enemyEntity = enemyObject.GetComponent<Entity>();

                    if (enemyEntity == null)
                    {
                        continue;
                    }

                    bool wasInstakill = false;

                    if (onePunchSystem != null && onePunchSystem.IsActive)
                    {
                        wasInstakill = onePunchSystem.CheckForInstakill(enemyEntity);
                    }

                    if (wasInstakill)
                    {
                        enemyEntity.TakeDamage(9999);
                    }
                    else
                    {
                        int finalDamage = CalculateFinalDamage(baseDamage, enemyObject);

                        enemyEntity.TakeDamage(finalDamage);
                    }

                    hitConnected = true;
                }
            }

            return hitConnected;
        }

        private int CalculateFinalDamage(int baseDamage, GameObject enemyObject)
        {
            if (!_serviceInitialized || _damageBonusService == null)
            {
                return CalculateDamageDirectly(baseDamage, enemyObject);
            }

            return _damageBonusService.CalculateDamageWithBonuses(baseDamage, enemyObject);
        }

        private int CalculateDamageDirectly(int baseDamage, GameObject enemyObject)
        {
            float defaultMultiplier = 1f;
            int minimumBonusDamage = 1;

            if (_hero == null || _hero.AbilityManager == null)
            {
                return baseDamage;
            }

            var enemyTypeComponent = enemyObject.GetComponent<EnemyTypeComponent>() ??
                                   enemyObject.GetComponentInParent<EnemyTypeComponent>();

            if (enemyTypeComponent == null)
            {
                return baseDamage;
            }

            float multiplier = _hero.AbilityManager.GetDamageMultiplierForEnemy(enemyObject);

            if (multiplier > defaultMultiplier)
            {
                int finalDamage = Mathf.CeilToInt(baseDamage * multiplier);

                if (finalDamage == baseDamage)
                {
                    finalDamage = baseDamage + minimumBonusDamage;
                }

                CreateBonusEffect(enemyObject, multiplier);

                return finalDamage;
            }

            return baseDamage;
        }

        private void CreateBonusEffect(GameObject target, float multiplier)
        {
            float verticalOffset = 1.5f;
            string effectName = "DirectBonusEffect";
            float destroyDelay = 1f;
            int fontSize = 24;

            GameObject textObj = new GameObject(effectName);

            textObj.transform.position = target.transform.position + Vector3.up * verticalOffset;

            TextMesh textMesh = textObj.AddComponent<TextMesh>();

            textMesh.text = $"x{multiplier}!";

            textMesh.color = Color.magenta;
            textMesh.fontSize = fontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;

            Object.Destroy(textObj, destroyDelay);
        }

        private bool IsWithinAttackAngle(Vector3 enemyPosition, Vector2 attackDirection)
        {
            Vector2 directionToEnemy = (enemyPosition - _attackOrigin.position).normalized;

            float angle = Vector2.Angle(attackDirection, directionToEnemy);

            return angle <= _attackAngleHalf;
        }

        public void DrawGizmos()
        {
            if (_attackOrigin != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_attackOrigin.position, _attackRange);
            }
        }
    }
}