using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Управление прогрессом игры: диалоги, квесты, события
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private readonly HashSet<string> completedDialoguePoints = new HashSet<string>();
    private readonly HashSet<string> permanentlyCollectedItems = new HashSet<string>();
    private readonly HashSet<string> playedCutsceneIds = new HashSet<string>();

    public bool HasFlashlight { get; private set; }

    public HashSet<string> CompletedDialoguePoints => completedDialoguePoints;
    public HashSet<string> PermanentlyCollectedItems => permanentlyCollectedItems;
    public HashSet<string> PlayedCutsceneIds => playedCutsceneIds;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // : изненный цикл теперь управляется CoreManager!
        return true;
    }

    public void MarkDialoguePointCompleted(string pointId)
    {
        if (string.IsNullOrEmpty(pointId))
        {
            return;
        }

        completedDialoguePoints.Add(pointId);
    }

    public bool IsDialoguePointCompleted(string pointId)
    {
        return completedDialoguePoints.Contains(pointId);
    }

    public void ResetDialogueProgress()
    {
        completedDialoguePoints.Clear();
    }

    public void MarkItemAsPermanentlyCollected(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return;
        }

        permanentlyCollectedItems.Add(itemId);
    }

    public bool IsItemPermanentlyCollected(string itemId)
    {
        return permanentlyCollectedItems.Contains(itemId);
    }

    public void ResetCollectedItems()
    {
        permanentlyCollectedItems.Clear();
    }

    // --- Катсцены ---

    public void MarkCutsceneAsPlayed(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId)) return;
        playedCutsceneIds.Add(cutsceneId);
        Debug.Log($"[ProgressManager] Катсцена {cutsceneId} помечена как просмотренная.");
    }

    public bool IsCutscenePlayed(string cutsceneId)
    {
        if (string.IsNullOrEmpty(cutsceneId)) return false;
        return playedCutsceneIds.Contains(cutsceneId);
    }

    // --- Фонарик ---

    public void SetFlashlightUnlocked(bool unlocked)
    {
        HasFlashlight = unlocked;
        Debug.Log($"[ProgressManager] Состояние фонарика изменено на: {unlocked}");
    }

    public void LoadFromGameData(GameData data)
    {
        if (data == null) return;

        CopyToSet(completedDialoguePoints, data.completedDialoguePoints);
        CopyToSet(permanentlyCollectedItems, data.permanentlyCollectedItemIds);
        CopyToSet(playedCutsceneIds, data.playedCutsceneIds);
        HasFlashlight = data.hasFlashlight;
    }

    public void SaveToGameData(GameData data)
    {
        if (data == null) return;

        CopyToList(data.completedDialoguePoints, completedDialoguePoints);
        CopyToList(data.permanentlyCollectedItemIds, permanentlyCollectedItems);
        CopyToList(data.playedCutsceneIds, playedCutsceneIds);
        data.hasFlashlight = HasFlashlight;
    }

    public string GetProgressInfo()
    {
        return $"Прогресс: {completedDialoguePoints.Count} диалогов, {permanentlyCollectedItems.Count} предметов, {playedCutsceneIds.Count} катсцен. Фонарик: {HasFlashlight}";
    }

    public void ResetAllProgress()
    {
        ResetDialogueProgress();
        ResetCollectedItems();
        playedCutsceneIds.Clear();
        HasFlashlight = false;
    }

    private static void CopyToSet(HashSet<string> target, List<string> source)
    {
        target.Clear();
        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Count; i++)
        {
            string value = source[i];
            if (!string.IsNullOrEmpty(value))
            {
                target.Add(value);
            }
        }
    }

    private static void CopyToList(List<string> target, HashSet<string> source)
    {
        target.Clear();
        foreach (string value in source)
        {
            target.Add(value);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Добавить тестовую точку")]
    private void TestAddDialoguePoint()
    {
        MarkDialoguePointCompleted("test_point_" + Random.Range(1, 100));
    }

    [ContextMenu("Тест: Добавить тестовый предмет")]
    private void TestAddItem()
    {
        MarkItemAsPermanentlyCollected("test_item_" + Random.Range(1, 100));
    }

    [ContextMenu("Тест: Показать информацию")]
    private void TestInfo()
    {
        Debug.Log(GetProgressInfo());
    }

    [ContextMenu("Тест: Сбросить всё")]
    private void TestReset()
    {
        ResetAllProgress();
    }
#endif

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
