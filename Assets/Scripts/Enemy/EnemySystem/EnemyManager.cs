using GeneralLogicEnemies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class EnemyManager : MonoBehaviour
{
    private const float SaveSyncDelay = 0.5f;

    public static EnemyManager Instance { get; private set; }

    private Dictionary<string, Entity> _enemies = new Dictionary<string, Entity>();
    private HashSet<string> _killedEnemies = new HashSet<string>();

    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeSaveSystemConnection();
    }

    private void OnDestroy()
    {
        CleanupEventSubscriptions();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SubscribeToEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SubscribeToEvents()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameLoaded += OnGameLoaded;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void CleanupEventSubscriptions()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.OnGameLoaded -= OnGameLoaded;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeSaveSystemConnection()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            StartCoroutine(DelayedSaveSync());
        }
    }

    private IEnumerator DelayedSaveSync()
    {
        yield return new WaitForSeconds(SaveSyncDelay);

        SyncWithSaveData();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsRestartingSameScene(scene.name))
        {
            ResetForSceneRestart();
        }
    }

    private bool IsRestartingSameScene(string newSceneName)
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            string savedSceneName = SaveSystem.Instance.CurrentSave.sceneName;

            return newSceneName == savedSceneName;
        }

        return false;
    }

    public void ResetForSceneRestart()
    {
        _enemies.Clear();

        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSave())
        {
            var saveData = SaveSystem.Instance.CurrentSave; _killedEnemies = saveData.killedEnemies != null ? new HashSet<string>(saveData.killedEnemies) : new HashSet<string>();
        }
        else
        {
            _killedEnemies.Clear();
        }
    }

    public void RegisterEnemy(Entity enemy, string enemyId)
    {
        int minRandomValue = 1000;
        int maxRandomValue = 9999;
        string invalidIdentifier = "-1";

        if (string.IsNullOrEmpty(enemyId) || enemyId.Contains(invalidIdentifier))
        {
            enemyId = $"{enemy.gameObject.name}_{Random.Range(minRandomValue, maxRandomValue)}";
        }

        if (_enemies.ContainsKey(enemyId))
        {
            enemyId = $"{enemyId}_{Random.Range(minRandomValue, maxRandomValue)}";
        }

        _enemies.Add(enemyId, enemy);

        if (_killedEnemies.Contains(enemyId))
        {
            enemy.gameObject.SetActive(false);
        }
    }

    public void RemoveEnemy(string enemyId)
    {
        if (_enemies.ContainsKey(enemyId))
        {
            _enemies.Remove(enemyId);
        }
    }

    public void MarkEnemyKilled(string enemyId)
    {
        string invalidIdentifier = "-1";

        if (string.IsNullOrEmpty(enemyId) || enemyId.Contains(invalidIdentifier))
        {
            return;
        }

        if (_killedEnemies.Contains(enemyId))
        {
            return;
        }

        _killedEnemies.Add(enemyId);

        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.MarkEnemyKilled(enemyId);
        }

        if (_enemies.ContainsKey(enemyId))
        {
            _enemies.Remove(enemyId);
        }
    }

    public HashSet<string> GetKilledEnemies()
    {
        return new HashSet<string>(_killedEnemies);
    }

    private void OnGameLoaded(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        _killedEnemies = saveData.killedEnemies != null ? new HashSet<string>(saveData.killedEnemies) : new HashSet<string>();

        ApplyKilledEnemiesStates();
    }

    private void ApplyKilledEnemiesStates()
    {
        foreach (var enemyId in _killedEnemies)
        {
            if (_enemies.ContainsKey(enemyId) && IsEnemyValid(_enemies[enemyId]))
            {
                _enemies[enemyId].gameObject.SetActive(false);
            }
        }
    }

    public void SyncWithSaveData()
    {
        if (SaveSystem.Instance == null || !SaveSystem.Instance.HasSave())
        {
            return;
        }

        var saveData = SaveSystem.Instance.CurrentSave;

        if (saveData.killedEnemies != null)
        {
            _killedEnemies = new HashSet<string>(saveData.killedEnemies);

            ApplyKilledEnemiesStates();
        }
    }

    public void ResetAllEnemies()
    {
        _killedEnemies.Clear();
        _enemies.Clear();

        var allEnemies = FindObjectsByType<Entity>(FindObjectsSortMode.None);

        foreach (var enemy in allEnemies)
        {
            if (IsEnemyValid(enemy))
            {
                enemy.gameObject.SetActive(true);
            }
        }
    }

    public bool IsEnemyAlive(string enemyId)
    {
        if (_killedEnemies.Contains(enemyId))
        {
            return false;
        }

        return _enemies.ContainsKey(enemyId) && IsEnemyValid(_enemies[enemyId]) && _enemies[enemyId].gameObject.activeInHierarchy;
    }

    private bool IsEnemyValid(Entity enemy)
    {
        return enemy != null && enemy.gameObject != null;
    }
}