using UnityEngine;

namespace Player.Abilities
{
    public sealed class AbilityManager
    {
        private const float BonusMultiplier = 2f;

        public bool HasDash { get; private set; }
        public bool HasDoubleJump { get; private set; }
        public bool HasMap { get; private set; }
        public bool HasAnatomy { get; private set; }
        public bool HasArmor { get; private set; }
        public bool HasSwampDamageBonus { get; private set; }
        public bool HasSkeletonDamageBonus { get; private set; }
        public bool HasDemonDamageBonus { get; private set; }
        public bool HasSpiderDamageBonus { get; private set; }
        public bool HasZombieDamageBonus { get; private set; }
        public bool HasBossDamageBonus { get; private set; }
        public bool IsLastChanceActive { get; private set; }
        public bool HasPassiveHealthRegeneration { get; private set; }
        public bool HasRobocopRegeneration { get; private set; }

        public bool HasVampireAbility;
        public bool HasOnePunchManAbility;

        private readonly ArmorManager _armorManager;
        private readonly Hero _hero;

        public AbilityManager(Hero hero)
        {
            _hero = hero;

            _armorManager = _hero.GetComponent<ArmorManager>() ??
                           UnityEngine.Object.FindObjectOfType<ArmorManager>();
            LoadAbilities();
        }

        public void UnlockDash()
        {
            HasDash = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockAnatomy()
        {
            HasAnatomy = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockMap()
        {
            HasMap = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockArmor()
        {
            HasArmor = true;

            SaveAbilities();
            SaveToSaveSystem();

            _armorManager?.UnlockArmorAbility();
        }

        public void UnlockSwampDamageBonus()
        {
            HasSwampDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockSkeletonDamageBonus()
        {
            HasSkeletonDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockDemonDamageBonus()
        {
            HasDemonDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockSpiderDamageBonus()
        {
            HasSpiderDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockZombieDamageBonus()
        {
            HasZombieDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockBossDamageBonus()
        {
            HasBossDamageBonus = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void PurchaseLastChance()
        {
            IsLastChanceActive = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UseLastChance()
        {
            IsLastChanceActive = false;

            SaveToPlayerPrefs();
            SaveToSaveSystem();
        }

        public void UnlockPassiveHealthRegeneration()
        {
            HasPassiveHealthRegeneration = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();

            var passiveRegen = _hero?.GetComponent<PassiveHealthRegeneration>();
            passiveRegen?.EnableRegeneration();
        }

        public void UnlockRobocopRegeneration()
        {
            HasRobocopRegeneration = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();

            var passiveRegen = _hero?.GetComponent<PassiveArmorRegeneration>();
            passiveRegen?.Activate();
        }

        public void UnlockVampireAbility()
        {
            HasVampireAbility = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();

            var vampireSystem = _hero?.GetComponent<VampireHealthSystem>();

            if (vampireSystem == null)
            {
                vampireSystem = _hero?.gameObject.AddComponent<VampireHealthSystem>();
            }

            if (vampireSystem != null)
            {
                vampireSystem.Activate();
            }
        }

        public void UnlockOnePunchManAbility()
        {
            HasOnePunchManAbility = true;

            SaveToPlayerPrefs();
            SaveToSaveSystem();

            var onePunchSystem = _hero?.GetComponent<OnePunchManSystem>();

            if (onePunchSystem == null)
            {
                onePunchSystem = _hero?.gameObject.AddComponent<OnePunchManSystem>();
            }

            if (onePunchSystem != null)
            {
                onePunchSystem.Activate();
            }
        }

        public float GetDamageMultiplierForEnemy(GameObject enemy)
        {
            float defaultMultiplier = 1f;

            if (enemy == null)
            {
                return defaultMultiplier;
            }

            var enemyTypeComponent = enemy.GetComponent<EnemyTypeComponent>() ??
                                   enemy.GetComponentInParent<EnemyTypeComponent>();

            if (enemyTypeComponent == null)
            {
                return defaultMultiplier;
            }

            float multiplier = defaultMultiplier;

            switch (enemyTypeComponent.EnemyType)
            {
                case EnemyType.Swamp:
                    multiplier = HasSwampDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                case EnemyType.Skeleton:
                    multiplier = HasSkeletonDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                case EnemyType.Demon:
                    multiplier = HasDemonDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                case EnemyType.Spider:
                    multiplier = HasSpiderDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                case EnemyType.Zombie:
                    multiplier = HasZombieDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                case EnemyType.Boss:
                    multiplier = HasBossDamageBonus ? BonusMultiplier : defaultMultiplier;

                    break;

                default:
                    multiplier = defaultMultiplier;

                    break;
            }

            return multiplier;
        }

        public bool HasBonusForEnemy(GameObject enemy)
        {
            if (enemy == null)
            {
                return false;
            }

            var enemyTypeComponent = enemy.GetComponent<EnemyTypeComponent>() ?? enemy.GetComponentInParent<EnemyTypeComponent>();

            if (enemyTypeComponent == null)
            {
                return false;
            }

            bool hasBonus = false;

            switch (enemyTypeComponent.EnemyType)
            {
                case EnemyType.Swamp:
                    hasBonus = HasSwampDamageBonus;

                    break;

                case EnemyType.Skeleton:
                    hasBonus = HasSkeletonDamageBonus;

                    break;

                case EnemyType.Demon:
                    hasBonus = HasDemonDamageBonus;

                    break;

                case EnemyType.Spider:
                    hasBonus = HasSpiderDamageBonus;

                    break;

                case EnemyType.Zombie:
                    hasBonus = HasZombieDamageBonus;

                    break;

                case EnemyType.Boss:
                    hasBonus = HasBossDamageBonus;

                    break;

                default:
                    hasBonus = false;

                    break;
            }

            return hasBonus;
        }

        public bool NeedArmorPlates()
        {
            return HasArmor && _armorManager != null && _armorManager.NeedArmorPlates();
        }

        private void LoadAbilities()
        {
            if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave() && SaveSystem.Instance.CurrentSave?.abilityData != null)
            {
                LoadFromSaveData(SaveSystem.Instance.CurrentSave.abilityData);
            }
            else
            {
                LoadFromPlayerPrefs();
            }
        }

        private void LoadFromSaveData(AbilitySaveData saveData)
        {
            HasDash = saveData.hasDash;
            HasDoubleJump = saveData.hasDoubleJump;
            HasMap = saveData.hasMap;
            HasAnatomy = saveData.hasAnatomy;
            HasArmor = saveData.hasArmor;
            HasSwampDamageBonus = saveData.hasSwampDamageBonus;
            HasSkeletonDamageBonus = saveData.hasSkeletonDamageBonus;
            HasDemonDamageBonus = saveData.hasDemonDamageBonus;
            HasSpiderDamageBonus = saveData.hasSpiderDamageBonus;
            HasZombieDamageBonus = saveData.hasZombieDamageBonus;
            HasBossDamageBonus = saveData.hasBossDamageBonus;
            IsLastChanceActive = saveData.isLastChanceActive;
            HasPassiveHealthRegeneration = saveData.hasPassiveHealthRegeneration;
            HasRobocopRegeneration = saveData.hasRobocopRegeneration;
            HasVampireAbility = saveData.hasVampireAbility;
            HasOnePunchManAbility = saveData.hasOnePunchManAbility;

            ApplyLoadedEffects();
        }

        private void ApplyLoadedEffects()
        {
            if (_hero == null)
            {
                return;
            }

            if (HasPassiveHealthRegeneration)
            {
                var passiveRegen = _hero.GetComponent<PassiveHealthRegeneration>();

                if (passiveRegen != null)
                {
                    passiveRegen.EnableRegeneration();
                }
            }

            if (HasRobocopRegeneration)
            {
                var armorRegen = _hero.GetComponent<PassiveArmorRegeneration>();

                if (armorRegen != null)
                {
                    armorRegen.Activate();
                }
            }

            if (HasVampireAbility)
            {
                var vampireSystem = _hero.GetComponent<VampireHealthSystem>();

                if (vampireSystem == null)
                {
                    vampireSystem = _hero.gameObject.AddComponent<VampireHealthSystem>();
                }

                vampireSystem.Activate();
            }

            if (HasOnePunchManAbility)
            {
                var onePunchSystem = _hero.GetComponent<OnePunchManSystem>();

                if (onePunchSystem == null)
                {
                    onePunchSystem = _hero.gameObject.AddComponent<OnePunchManSystem>();
                }

                onePunchSystem.Activate();
            }

            if (HasArmor && _armorManager != null)
            {
                _armorManager.UnlockArmorAbility();
            }
        }

        private void SaveToSaveSystem()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.UpdateAbilityData(this);
            }
        }

        private void LoadFromPlayerPrefs()
        {
            int activeStateValue = 1;
            int inactiveStateValue = 0;

            HasDash = PlayerPrefs.GetInt("Ability_Dash", inactiveStateValue) == activeStateValue;
            HasDoubleJump = PlayerPrefs.GetInt("Ability_DoubleJump", inactiveStateValue) == activeStateValue;
            HasMap = PlayerPrefs.GetInt("Ability_Map", inactiveStateValue) == activeStateValue;
            HasAnatomy = PlayerPrefs.GetInt("Ability_Anatomy", inactiveStateValue) == activeStateValue;
            HasArmor = PlayerPrefs.GetInt("Ability_Armor", inactiveStateValue) == activeStateValue;
            HasSwampDamageBonus = PlayerPrefs.GetInt("Ability_SwampDamage", inactiveStateValue) == activeStateValue;
            HasSkeletonDamageBonus = PlayerPrefs.GetInt("Ability_SkeletonDamage", inactiveStateValue) == activeStateValue;
            HasDemonDamageBonus = PlayerPrefs.GetInt("Ability_DemonDamage", inactiveStateValue) == activeStateValue;
            HasSpiderDamageBonus = PlayerPrefs.GetInt("Ability_SpiderDamage", inactiveStateValue) == activeStateValue;
            HasZombieDamageBonus = PlayerPrefs.GetInt("Ability_ZombieDamage", inactiveStateValue) == activeStateValue;
            HasBossDamageBonus = PlayerPrefs.GetInt("Ability_BossDamage", inactiveStateValue) == activeStateValue;
            IsLastChanceActive = PlayerPrefs.GetInt("LastChance_Active", inactiveStateValue) == activeStateValue;
            HasVampireAbility = PlayerPrefs.GetInt("Ability_Vampire", inactiveStateValue) == activeStateValue;
            HasPassiveHealthRegeneration = PlayerPrefs.GetInt("Ability_PassiveHealthRegen", inactiveStateValue) == activeStateValue;
            HasRobocopRegeneration = PlayerPrefs.GetInt("Ability_RobocopRegen", inactiveStateValue) == activeStateValue;
            HasOnePunchManAbility = PlayerPrefs.GetInt("Ability_OnePunchMan", inactiveStateValue) == activeStateValue;
        }

        private void SaveAbilities()
        {
            SaveToPlayerPrefs();
        }

        private void SaveToPlayerPrefs()
        {
            int activeStateValue = 1;
            int inactiveStateValue = 0;

            PlayerPrefs.SetInt("Ability_Dash", HasDash ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_DoubleJump", HasDoubleJump ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_Map", HasMap ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_Anatomy", HasAnatomy ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_Armor", HasArmor ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_SwampDamage", HasSwampDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_SkeletonDamage", HasSkeletonDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_DemonDamage", HasDemonDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_SpiderDamage", HasSpiderDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_ZombieDamage", HasZombieDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_BossDamage", HasBossDamageBonus ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_PassiveHealthRegen", HasPassiveHealthRegeneration ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_RobocopRegen", HasRobocopRegeneration ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("LastChance_Active", IsLastChanceActive ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_Vampire", HasVampireAbility ? activeStateValue : inactiveStateValue);
            PlayerPrefs.SetInt("Ability_OnePunchMan", HasOnePunchManAbility ? activeStateValue : inactiveStateValue);
            PlayerPrefs.Save();
        }
    }
}