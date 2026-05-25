using GeneralLogicEnemies;
using UnityEngine;

public sealed class EnemyRegistration : MonoBehaviour
{
    private const string DefaultIdFormat = "{0}_{1:F3}_{2:F3}";
    private const float DeathSaveDelay = 0.1f;
    private static int _counter = 0;

    [SerializeField] private string _enemyId;

    private Entity _enemyEntity;
    private bool _isRegistered = false;
    private bool _isDead = false;
    private bool _isQuitting = false;

    private void Start()
    {
        InitializeEnemyRegistration();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void InitializeEnemyRegistration()
    {
        _enemyEntity = GetComponent<Entity>();

        if (_enemyEntity == null)
        {
            return;
        }

        if (EnemyManager.Instance == null)
        {
            return;
        }

        GenerateEnemyIdIfEmpty();

        _enemyEntity.OnEntityDeath += OnEnemyDeath;

        EnemyManager.Instance.RegisterEnemy(_enemyEntity, _enemyId);

        _isRegistered = true;
    }

    private void GenerateEnemyIdIfEmpty()
    {
        if (string.IsNullOrEmpty(_enemyId))
        {
            _counter++;

            _enemyId = string.Format(
                DefaultIdFormat,
                gameObject.name.Replace("(Clone)", "").Trim(),
                transform.position.x,
                transform.position.y
            ) + $"_{_counter}";
        }
    }

    private void OnEnemyDeath(Entity enemy)
    {
        if (_isDead || _isQuitting)
        {
            return;
        }

        _isDead = true;

        Invoke(nameof(SaveEnemyDeath), DeathSaveDelay);
    }

    private void SaveEnemyDeath()
    {
        if (string.IsNullOrEmpty(_enemyId))
        {
            return;
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.MarkEnemyKilled(_enemyId);
        }

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkEnemyKilled(_enemyId);
        }
    }

    private void OnDestroy()
    {
        if (_enemyEntity != null)
        {
            _enemyEntity.OnEntityDeath -= OnEnemyDeath;
        }

        if (!_isDead && _isRegistered && !_isQuitting)
        {
            if (EnemyManager.Instance != null && !string.IsNullOrEmpty(_enemyId))
            {
                EnemyManager.Instance.RemoveEnemy(_enemyId);
            }
        }
    }

    public void SetEnemyId(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            _enemyId = id;
        }
    }

    public string GetEnemyId()
    {
        return _enemyId;
    }
}