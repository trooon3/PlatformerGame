using System.Collections.Generic;
using UnityEngine;
using DoorControl;
using NPC;
using EasyBossLogic;
using MiddleBossLogic;
using HardBossLogic;
using ChestControl;

public class DebugTeleporter : MonoBehaviour
{
    private int _keyIndex;
    private int _checkpointIndex;
    private int _bossDoorIndex;
    private int _merchantIndex;
    private int _bossIndex;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TeleportToNextKey();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TeleportToNextCheckpoint();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TeleportToNextBossDoor();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TeleportToNextMerchant();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TeleportToNextBoss();
        }
    }

    private void TeleportToNextKey()
    {
        List<Chest> closedChests = new List<Chest>();
        Chest[] allChests = FindObjectsByType<Chest>(FindObjectsSortMode.None);

        foreach (Chest chest in allChests)
        {
            if (chest.IsOpened == false)
            {
                closedChests.Add(chest);
            }
        }

        if (closedChests.Count == 0)
        {
            Debug.Log("Дебаг: Закрытых сундуков на сцене больше нет.");
            return;
        }

        if (_keyIndex >= closedChests.Count)
        {
            _keyIndex = 0;
        }
        else
        {
            _keyIndex = (_keyIndex + 1) % closedChests.Count;
        }

        ExecuteTeleport(closedChests[_keyIndex].transform.position, "Закрытый сундук");
    }

    private void TeleportToNextCheckpoint()
    {
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        if (checkpoints.Length == 0)
        {
            Debug.Log("Дебаг: Чекпоинты не найдены.");
            return;
        }

        _checkpointIndex = (_checkpointIndex + 1) % checkpoints.Length;
        ExecuteTeleport(checkpoints[_checkpointIndex].transform.position, $"Чекпоинт ({checkpoints[_checkpointIndex].GetCheckpointId()})");
    }

    private void TeleportToNextBossDoor()
    {
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        List<Door> bossDoors = new List<Door>();

        foreach (Door door in allDoors)
        {
            if (door.requiresKey)
            {
                bossDoors.Add(door);
            }
        }

        if (bossDoors.Count == 0)
        {
            Debug.Log("Дебаг: Дверей боссов (с ключом) не найдено.");
            return;
        }

        _bossDoorIndex = (_bossDoorIndex + 1) % bossDoors.Count;
        ExecuteTeleport(bossDoors[_bossDoorIndex].transform.position, "Дверь босса");
    }

    private void TeleportToNextMerchant()
    {
        Merchant[] merchants = FindObjectsByType<Merchant>(FindObjectsSortMode.None);
        if (merchants.Length == 0)
        {
            Debug.Log("Дебаг: Торговцы не найдены.");
            return;
        }

        _merchantIndex = (_merchantIndex + 1) % merchants.Length;
        ExecuteTeleport(merchants[_merchantIndex].transform.position, "Торговец");
    }

    private void TeleportToNextBoss()
    {
        List<Transform> bossTransforms = new List<Transform>();

        foreach (var boss in FindObjectsByType<BossReaper>(FindObjectsSortMode.None))
        {
            if (boss.IsAlive) bossTransforms.Add(boss.transform);
        }
        foreach (var boss in FindObjectsByType<BossGolem>(FindObjectsSortMode.None))
        {
            if (boss.IsAlive) bossTransforms.Add(boss.transform);
        }
        foreach (var boss in FindObjectsByType<BossStoneGolem>(FindObjectsSortMode.None))
        {
            if (boss.IsAlive) bossTransforms.Add(boss.transform);
        }
        foreach (var boss in FindObjectsByType<BossKuznets>(FindObjectsSortMode.None))
        {
            if (boss.IsAlive) bossTransforms.Add(boss.transform);
        }

        if (bossTransforms.Count == 0)
        {
            Debug.Log("Дебаг: Живых боссов на сцене не найдено.");
            return;
        }

        _bossIndex = (_bossIndex + 1) % bossTransforms.Count;
        ExecuteTeleport(bossTransforms[_bossIndex].position, "Босс");
    }

    private void ExecuteTeleport(Vector3 targetPosition, string targetName)
    {
        Hero player = Hero.Instance;

        if (player == null)
        {
            Debug.LogWarning("Дебаг: Не найден синглтон Hero.Instance!");
            return;
        }

        player.transform.position = targetPosition;

        if (player.Rigidbody != null)
        {
            player.Rigidbody.velocity = Vector2.zero;
        }

        Debug.Log($"[DebugTeleporter] Телепортация к: {targetName}");
    }
}