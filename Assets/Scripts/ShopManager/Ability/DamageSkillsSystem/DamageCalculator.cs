using Player.Abilities;
using UnityEngine;

public sealed class DamageCalculator
{
    private readonly AbilityManager _abilityManager;

    public DamageCalculator(AbilityManager abilityManager)
    {
        _abilityManager = abilityManager;
    }

    public int CalculateDamageWithBonuses(int baseDamage, GameObject target = null)
    {
        int finalDamage = baseDamage;
        float neutralMultiplier = 1f;

        if (_abilityManager != null && target != null)
        {
            float multiplier = _abilityManager.GetDamageMultiplierForEnemy(target);

            if (multiplier > neutralMultiplier)
            {
                finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

                EnemyTypeComponent enemyTypeComponent = target.GetComponent<EnemyTypeComponent>() ?? target.GetComponentInParent<EnemyTypeComponent>();

                if (enemyTypeComponent != null)
                {
                    bool hasBonus = _abilityManager.HasBonusForEnemy(target);

                    string bonusType = GetSpecificBonusType(enemyTypeComponent.EnemyType);
                    bool isBonusActive = CheckSpecificBonus(enemyTypeComponent.EnemyType);
                }

                ShowBonusDamageEffect(target, multiplier, enemyTypeComponent);
            }
        }

        return finalDamage;
    }

    private string GetSpecificBonusType(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Swamp => "Swamp Damage Bonus",
            EnemyType.Skeleton => "Skeleton Damage Bonus",
            EnemyType.Demon => "Demon Damage Bonus",
            EnemyType.Spider => "Spider Damage Bonus",
            EnemyType.Zombie => "Zombie Damage Bonus",
            EnemyType.Boss => "Boss Damage Bonus",
            _ => "No Bonus"
        };
    }

    private bool CheckSpecificBonus(EnemyType enemyType)
    {
        if (_abilityManager == null)
        {
            return false;
        }

        return enemyType switch
        {
            EnemyType.Swamp => _abilityManager.HasSwampDamageBonus,
            EnemyType.Skeleton => _abilityManager.HasSkeletonDamageBonus,
            EnemyType.Demon => _abilityManager.HasDemonDamageBonus,
            EnemyType.Spider => _abilityManager.HasSpiderDamageBonus,
            EnemyType.Zombie => _abilityManager.HasZombieDamageBonus,
            EnemyType.Boss => _abilityManager.HasBossDamageBonus,
            _ => false
        };
    }

    private void ShowBonusDamageEffect(GameObject target, float multiplier, EnemyTypeComponent enemyTypeComponent = null)
    {
        string enemyTypeName = "врага";

        if (enemyTypeComponent != null)
        {
            enemyTypeName = GetEnemyTypeName(enemyTypeComponent.EnemyType);
        }

        CreateBonusDamageText(target, $"БОЛОТО x{multiplier}!");
    }

    private string GetEnemyTypeName(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Swamp => "болотного врага",
            EnemyType.Skeleton => "скелета",
            EnemyType.Demon => "демона",
            EnemyType.Spider => "паука",
            EnemyType.Zombie => "зомби",
            EnemyType.Boss => "босса",
            _ => "врага"
        };
    }

    private void CreateBonusDamageText(GameObject target, string text)
    {
        float verticalOffset = 2f;
        string textObjectName = "BonusDamageText";
        int fontSize = 36;
        float destroyDelay = 2f;

        GameObject textObj = new GameObject(textObjectName);
        textObj.transform.position = target.transform.position + Vector3.up * verticalOffset;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = text;

        textMesh.color = Color.red;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontStyle = FontStyle.Bold;

        Object.Destroy(textObj, destroyDelay);
    }
}