using GeneralLogicEnemies;
using UnityEngine;

namespace Player.Abilities
{
    public sealed class MeleeAttack
    {
        private const float AngleMultiplier = 0.5f;
        private const int InstakillDamage = 9999;
        private const float DefaultMultiplier = 1f;
        private const int MinimumBonusDamage = 1;

        private readonly Transform _attackOrigin;
        private readonly LayerMask _enemyLayerMask;
        private readonly float _attackRange;
        private readonly float _attackAngleHalf;
        private readonly Hero _hero;

        private OnePunchManSystem _onePunchSystem;
        private DamageBonusService _damageBonusService;
        private bool _onePunchSystemChecked;
        private bool _damageBonusServiceInitialized;

        public MeleeAttack(
            Transform attackOrigin,
            float attackRange,
            float attackAngle,
            LayerMask enemyLayerMask,
            Hero hero)
        {
            _attackOrigin = attackOrigin;
            _attackRange = attackRange;
            _enemyLayerMask = enemyLayerMask;
            _attackAngleHalf = attackAngle * AngleMultiplier;
            _hero = hero;
        }

        public bool Perform(int baseDamage, Vector2 attackDirection)
        {
            if (_attackOrigin == null)
            {
                return false;
            }

            InitializeDamageBonusService();

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
                _attackOrigin.position,
                _attackRange,
                _enemyLayerMask);

            bool hitConnected = false;
            OnePunchManSystem onePunchSystem = GetOnePunchSystem();

            foreach (Collider2D hitCollider in hitColliders)
            {
                if (hitCollider == null)
                {
                    continue;
                }

                Vector2 closestPointOnEnemy = hitCollider.ClosestPoint(_attackOrigin.position);

                if (IsWithinAttackAngle(closestPointOnEnemy, attackDirection) == false)
                {
                    continue;
                }

                Entity enemyEntity = hitCollider.GetComponent<Entity>();

                if (enemyEntity == null)
                {
                    enemyEntity = hitCollider.GetComponentInParent<Entity>();
                }

                if (enemyEntity == null)
                {
                    continue;
                }

                ApplyDamageToEnemy(enemyEntity, hitCollider.gameObject, baseDamage, onePunchSystem);

                hitConnected = true;
            }

            return hitConnected;
        }

        public void DrawGizmos()
        {
            if (_attackOrigin == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackOrigin.position, _attackRange);
        }

        private void ApplyDamageToEnemy(
            Entity enemyEntity,
            GameObject enemyObject,
            int baseDamage,
            OnePunchManSystem onePunchSystem)
        {
            bool wasInstakill = onePunchSystem != null &&
                                onePunchSystem.IsActive &&
                                onePunchSystem.CheckForInstakill(enemyEntity);

            if (wasInstakill)
            {
                enemyEntity.TakeDamage(InstakillDamage);

                return;
            }

            int finalDamage = CalculateFinalDamage(baseDamage, enemyObject);

            enemyEntity.TakeDamage(finalDamage);
        }

        private void InitializeDamageBonusService()
        {
            if (_damageBonusServiceInitialized || _hero == null || _hero.AbilityManager == null)
            {
                return;
            }

            _damageBonusService = new DamageBonusService(_hero.AbilityManager);
            _damageBonusServiceInitialized = true;
        }

        private OnePunchManSystem GetOnePunchSystem()
        {
            if (_onePunchSystemChecked)
            {
                return _onePunchSystem;
            }

            if (_hero != null)
            {
                _onePunchSystem = _hero.GetComponent<OnePunchManSystem>();
            }

            _onePunchSystemChecked = true;

            return _onePunchSystem;
        }

        private int CalculateFinalDamage(int baseDamage, GameObject enemyObject)
        {
            if (_damageBonusServiceInitialized && _damageBonusService != null)
            {
                return _damageBonusService.CalculateDamageWithBonuses(baseDamage, enemyObject);
            }

            return CalculateDamageDirectly(baseDamage, enemyObject);
        }

        private int CalculateDamageDirectly(int baseDamage, GameObject enemyObject)
        {
            if (_hero == null || _hero.AbilityManager == null || enemyObject == null)
            {
                return baseDamage;
            }

            float multiplier = _hero.AbilityManager.GetDamageMultiplierForEnemy(enemyObject);

            if (multiplier <= DefaultMultiplier)
            {
                return baseDamage;
            }

            int finalDamage = Mathf.CeilToInt(baseDamage * multiplier);

            if (finalDamage <= baseDamage)
            {
                finalDamage = baseDamage + MinimumBonusDamage;
            }

            CreateBonusEffect(enemyObject, multiplier);

            return finalDamage;
        }

        private void CreateBonusEffect(GameObject target, float multiplier)
        {
            const float verticalOffset = 1.5f;
            const string effectName = "DirectBonusEffect";
            const float destroyDelay = 1f;
            const int fontSize = 24;

            GameObject textObject = new GameObject(effectName);

            textObject.transform.position = target.transform.position + Vector3.up * verticalOffset;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();

            textMesh.text = $"x{multiplier}!";
            textMesh.color = Color.magenta;
            textMesh.fontSize = fontSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.fontStyle = FontStyle.Bold;

            Object.Destroy(textObject, destroyDelay);
        }

        private bool IsWithinAttackAngle(Vector3 enemyPosition, Vector2 attackDirection)
        {
            Vector2 directionToEnemy = (enemyPosition - _attackOrigin.position).normalized;
            float angle = Vector2.Angle(attackDirection, directionToEnemy);

            return angle <= _attackAngleHalf;
        }
    }
}