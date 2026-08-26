using TMPro;
using Player.Abilities;
using UnityEngine;

public sealed class DamageBonusService
{
    private const float NeutralMultiplier = 1f;
    private const int MinimumBonusDamage = 1;
    private const float EffectVerticalOffset = 1.5f;
    private const float EffectDestroyDelay = 1f;


    private const int EffectFontSize = 10;

    private readonly AbilityManager _abilityManager;

    public DamageBonusService(AbilityManager abilityManager)
    {
        _abilityManager = abilityManager;
    }

    public int CalculateDamageWithBonuses(int baseDamage, GameObject target)
    {
        if (_abilityManager == null || target == null)
        {
            return baseDamage;
        }

        float multiplier = _abilityManager.GetDamageMultiplierForEnemy(target);

        if (multiplier <= NeutralMultiplier)
        {
            return baseDamage;
        }

        int finalDamage = Mathf.CeilToInt(baseDamage * multiplier);

        if (finalDamage <= baseDamage)
        {
            finalDamage = baseDamage + MinimumBonusDamage;
        }

        ShowEffect(target, multiplier);

        return finalDamage;
    }

    private void ShowEffect(GameObject target, float multiplier)
    {
        GameObject textObject = new GameObject("BonusEffect");
        textObject.transform.position = target.transform.position + Vector3.up * EffectVerticalOffset;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();

        textMesh.text = $"x{multiplier}";
        textMesh.color = Color.red;
        textMesh.fontSize = EffectFontSize;

        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontStyle = FontStyles.Bold;

        textMesh.sortingOrder = 1488;

        Object.Destroy(textObject, EffectDestroyDelay);
    }
}