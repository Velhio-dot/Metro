using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Управление прогрессом игры: диалоги, квесты, события
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    // Прогресс диалогов
    private HashSet<string> completedDialoguePoints = new HashSet<string>();
    private HashSet<string> permanentlyCollectedItems = new HashSet<string>();

    // Публичные свойства
    public HashSet<string> CompletedDialoguePoints => completedDialoguePoints;
    public HashSet<string> PermanentlyCollectedItems => permanentlyCollectedItems;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("ProgressManager: инициализирован");
    }

    // ===== ДИАЛОГИ =====

    /// <summary>
    /// Отметить диалоговую точку как пройденную
    /// </summary>
    public void MarkDialoguePointCompleted(string pointId)
    {
        if (string.IsNullOrEmpty(pointId)) return;

        if (!completedDialoguePoints.Contains(pointId))
        {
            completedDialoguePoints.Add(pointId);
            Debug.Log($"ProgressManager: диалоговая точка '{pointId}' отмечена как пройденная");
        }
    }

    /// <summary>
    /// Проверить пройдена ли диалоговая точка
    /// </summary>
    public bool IsDialoguePointCompleted(string pointId)
    {
        return completedDialoguePoints.Contains(pointId);
    }

    /// <summary>
    /// Сбросить прогресс диалогов
    /// </summary>
    public void ResetDialogueProgress()
    {
        completedDialoguePoints.Clear();
        Debug.Log("ProgressManager: прогресс диалогов сброшен");
    }

    // ===== ПРЕДМЕТЫ =====

    /// <summary>
    /// Отметить предмет как навсегда собранный
    /// </summary>
    public void MarkItemAsPermanentlyCollected(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        if (!permanentlyCollectedItems.Contains(itemId))
        {
            permanentlyCollectedItems.Add(itemId);
            Debug.Log($"ProgressManager: предмет '{itemId}' отмечен как навсегда собранный");
        }
    }

    /// <summary>
    /// Проверить собран ли предмет навсегда
    /// </summary>
    public bool IsItemPermanentlyCollected(string itemId)
    {
        return permanentlyCollectedItems.Contains(itemId);
    }

    /// <summary>
    /// Сбросить собранные предметы
    /// </summary>
    public void ResetCollectedItems()
    {
        permanentlyCollectedItems.Clear();
        Debug.Log("ProgressManager: собранные предметы сброшены");
    }

    // ===== СОХРАНЕНИЕ/ЗАГРУЗКА =====

    /// <summary>
    /// Загрузить прогресс из данных
    /// </summary>
    public void LoadFromGameData(GameData data)
    {
        if (data == null) return;

        // Диалоги
        completedDialoguePoints.Clear();
        foreach (var point in data.completedDialoguePoints)
        {
            completedDialoguePoints.Add(point);
        }

        // Предметы
        permanentlyCollectedItems.Clear();
        foreach (var itemId in data.permanentlyCollectedItemIds)
        {
            permanentlyCollectedItems.Add(itemId);
        }

        Debug.Log($"ProgressManager: загружено {completedDialoguePoints.Count} диалоговых точек, " +
                 $"{permanentlyCollectedItems.Count} собранных предметов");
    }

    /// <summary>
    /// Сохранить прогресс в данные
    /// </summary>
    public void SaveToGameData(GameData data)
    {
        if (data == null) return;

        // Диалоги
        data.completedDialoguePoints.Clear();
        foreach (var point in completedDialoguePoints)
        {
            data.completedDialoguePoints.Add(point);
        }

        // Предметы
        data.permanentlyCollectedItemIds.Clear();
        foreach (var itemId in permanentlyCollectedItems)
        {
            data.permanentlyCollectedItemIds.Add(itemId);
        }

        Debug.Log("ProgressManager: прогресс сохранен в данные");
    }

    // ===== УТИЛИТЫ =====

    /// <summary>
    /// Получить статистику прогресса
    /// </summary>
    public string GetProgressInfo()
    {
        return $"Прогресс: {completedDialoguePoints.Count} диалогов, " +
               $"{permanentlyCollectedItems.Count} предметов";
    }

    /// <summary>
    /// Полный сброс прогресса
    /// </summary>
    public void ResetAllProgress()
    {
        ResetDialogueProgress();
        ResetCollectedItems();
        Debug.Log("ProgressManager: весь прогресс сброшен");
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Добавить тестовую точку")]
    void TestAddDialoguePoint()
    {
        MarkDialoguePointCompleted("test_point_" + Random.Range(1, 100));
    }

    [ContextMenu("Тест: Добавить тестовый предмет")]
    void TestAddItem()
    {
        MarkItemAsPermanentlyCollected("test_item_" + Random.Range(1, 100));
    }

    [ContextMenu("Тест: Показать информацию")]
    void TestInfo()
    {
        Debug.Log(GetProgressInfo());
    }

    [ContextMenu("Тест: Сбросить всё")]
    void TestReset() => ResetAllProgress();
#endif
}