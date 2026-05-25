using UnityEngine;

public sealed class EnemyTypeComponent : MonoBehaviour, IEnemyTypeComponent
{
    [SerializeField] private EnemyType _enemyType = EnemyType.Default;

    public EnemyType EnemyType => _enemyType;

    public void SetEnemyType(EnemyType type)
    {
        _enemyType = type;
    }
}