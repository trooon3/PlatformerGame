using Player.Abilities;
using UnityEngine;

public sealed class DamageBonusService
{
    private readonly AbilityManager _abilityManager;

    public DamageBonusService(AbilityManager abilityManager)
    {
        _abilityManager = abilityManager;
    }

    public int CalculateDamageWithBonuses(int baseDamage, GameObject target)
    {
        float neutralMultiplier = 1f;
        int minimumBonusDamage = 1;

        if (_abilityManager == null || target == null)
        {
            return baseDamage;
        }

        float multiplier = _abilityManager.GetDamageMultiplierForEnemy(target);

        if (multiplier > neutralMultiplier)
        {
            int finalDamage = Mathf.CeilToInt(baseDamage * multiplier);

            if (finalDamage <= baseDamage)
            {
                finalDamage = baseDamage + minimumBonusDamage;
            }

            ShowEffect(target, multiplier);

            return finalDamage;
        }

        return baseDamage;
    }

    private void ShowEffect(GameObject target, float multiplier)
    {
        float verticalOffset = 1.5f;
        string effectObjectName = "BonusEffect";
        int fontSize = 20;
        float destroyDelay = 1f;

        GameObject textObj = new GameObject(effectObjectName);
        textObj.transform.position = target.transform.position + Vector3.up * verticalOffset;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = $"x{multiplier}!";
        textMesh.color = Color.red;
        textMesh.fontSize = fontSize;

        Object.Destroy(textObj, destroyDelay);
    }
}