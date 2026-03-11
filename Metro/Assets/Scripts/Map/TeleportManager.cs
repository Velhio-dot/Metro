using UnityEngine;
using System.Collections.Generic;

public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance { get; private set; }

    [Header("Global Settings")]
    [SerializeField] private float globalCooldown = 0.5f; // Защита между телепортами

    private List<DoorTeleport> allDoors = new List<DoorTeleport>();
    private bool isGlobalTeleportActive = false;
    private float lastGlobalTeleportTime = 0f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("TeleportManager создан и сохранён между сценами");
        }
        else
        {
            Debug.LogWarning("Уничтожаем дубликат TeleportManager");
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Автоматический сброс глобальной блокировки если что-то пошло не так
        if (isGlobalTeleportActive && Time.time - lastGlobalTeleportTime > 10f)
        {
            Debug.LogWarning("Глобальная блокировка телепортов зависла! Принудительный сброс.");
            ResetGlobalLock();
        }
    }

    // Регистрация двери в системе
    public void RegisterDoor(DoorTeleport door)
    {
        if (!allDoors.Contains(door))
        {
            allDoors.Add(door);
            Debug.Log($"Дверь '{door.gameObject.name}' зарегистрирована в TeleportManager");
        }
    }

    // Удаление двери из системы
    public void UnregisterDoor(DoorTeleport door)
    {
        if (allDoors.Contains(door))
        {
            allDoors.Remove(door);
            Debug.Log($"Дверь '{door.gameObject.name}' удалена из TeleportManager");
        }
    }

    // Проверка можно ли начать телепортацию
    public bool CanStartTeleport()
    {
        // Проверка глобальной блокировки
        if (isGlobalTeleportActive)
        {
            Debug.Log($"Глобальная блокировка активна. Прошло: {Time.time - lastGlobalTeleportTime:F1}с");
            return false;
        }

        // Проверка глобального кулдауна
        if (Time.time - lastGlobalTeleportTime < globalCooldown)
        {
            float timeLeft = globalCooldown - (Time.time - lastGlobalTeleportTime);
            Debug.Log($"Глобальный кулдаун: {timeLeft:F1}с");
            return false;
        }

        return true;
    }

    // Начало глобальной телепортации
    public void StartGlobalTeleport(DoorTeleport initiatingDoor)
    {
        isGlobalTeleportActive = true;
        lastGlobalTeleportTime = Time.time;

        Debug.Log($"=== НАЧАТА ГЛОБАЛЬНАЯ ТЕЛЕПОРТАЦИЯ (инициатор: {initiatingDoor.gameObject.name}) ===");

        // Блокируем ВСЕ двери кроме инициатора
        foreach (DoorTeleport door in allDoors)
        {
            if (door != initiatingDoor)
            {
                door.SetExternalLock(true);
            }
        }
    }

    // Завершение глобальной телепортации
    public void EndGlobalTeleport(DoorTeleport initiatingDoor)
    {
        Debug.Log($"=== ЗАВЕРШЕНА ГЛОБАЛЬНАЯ ТЕЛЕПОРТАЦИЯ (инициатор: {initiatingDoor.gameObject.name}) ===");

        // Разблокируем ВСЕ двери
        foreach (DoorTeleport door in allDoors)
        {
            door.SetExternalLock(false);
        }

        isGlobalTeleportActive = false;
        lastGlobalTeleportTime = Time.time;
    }

    // Принудительный сброс блокировки
    public void ResetGlobalLock()
    {
        Debug.LogWarning("=== ПРИНУДИТЕЛЬНЫЙ СБРОС ГЛОБАЛЬНОЙ БЛОКИРОВКИ ===");

        foreach (DoorTeleport door in allDoors)
        {
            door.SetExternalLock(false);
        }

        isGlobalTeleportActive = false;
        lastGlobalTeleportTime = Time.time - globalCooldown; // Сбрасываем кулдаун
    }

    // Получить статистику
    public string GetStatus()
    {
        return $"TeleportManager: Дверей={allDoors.Count}, " +
               $"Блокировка={isGlobalTeleportActive}, " +
               $"Последняя тел.={Time.time - lastGlobalTeleportTime:F1}с назад";
    }

    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            GUI.Label(new Rect(10, 160, 400, 30), GetStatus());
        }
    }
}