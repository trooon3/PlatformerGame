using UnityEngine;
using YG; 

public sealed class LevelStatsTracker : MonoBehaviour
{
    public static LevelStatsTracker Instance { get; private set; }

    private static float _persistentTime = 0f;
    private static int _persistentDamage = 0;
    private static int _persistentDeaths = 0;
    private static int _persistentScore = 0;

    public float TimeInSeconds => _persistentTime;
    public int TotalDamageTaken => _persistentDamage;
    public int TotalDeaths => _persistentDeaths;
    public int TotalScore => _persistentScore;

    private bool _isTracking = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        HealthManager.OnDamageTakenGlobal += AddDamage;
        Hero.OnHeroDiedGlobal += AddDeath;
        EnemyManager.OnEnemyKilledGlobal += AddScore;
        BossDoor.OnBossDoorOpenedGlobal += CompleteLevel;
    }

    private void OnDisable()
    {
        HealthManager.OnDamageTakenGlobal -= AddDamage;
        Hero.OnHeroDiedGlobal -= AddDeath;
        EnemyManager.OnEnemyKilledGlobal -= AddScore;
        BossDoor.OnBossDoorOpenedGlobal -= CompleteLevel;
    }

    private void Update()
    {
        if (_isTracking)
        {
            _persistentTime += Time.deltaTime;
        }
    }

    private void AddDamage(int amount)
    {
        if (_isTracking) _persistentDamage += amount;
    }

    private void AddDeath()
    {
        if (_isTracking) _persistentDeaths++;
    }

    private void AddScore()
    {
        if (_isTracking) _persistentScore += 100;
    }

    private void CompleteLevel()
    {
        if (_isTracking == false) return;

        _isTracking = false;
        Debug.Log("Уровень завершен. Запускаем поочередную отправку рекордов...");

        StartCoroutine(SendLeaderboardsRoutine());
    }

    private System.Collections.IEnumerator SendLeaderboardsRoutine()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        YG2.SetLeaderboard("ScoreBoard", _persistentScore);
#endif
        yield return null;

        ResetAllStats();
    }

    public static void ResetAllStats()
    {
        _persistentTime = 0f;
        _persistentDamage = 0;
        _persistentDeaths = 0;
        _persistentScore = 0;
    }
}