#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EditorOnlyReset
{
    [MenuItem("Tools/⚡ Reset Game Data")]
    static void ResetGameData()
    {
        // 1. Найдём PlayerDataSO (автоматически)
        string[] guids = AssetDatabase.FindAssets("t:PlayerDataSO");
        if (guids.Length == 0)
        {
            Debug.LogError("PlayerDataSO не найден!");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PlayerDataSO playerData = AssetDatabase.LoadAssetAtPath<PlayerDataSO>(path);

            if (playerData != null)
            {
                playerData.ResetData();
                EditorUtility.SetDirty(playerData);
                Debug.Log($"Сброшен: {playerData.name}");
            }
        }

        // 2. Удалим сохранения
        string saveDir = Application.persistentDataPath;
        if (System.IO.Directory.Exists(saveDir))
        {
            var files = System.IO.Directory.GetFiles(saveDir, "savegame*.json");
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
                Debug.Log($"Удалён файл: {file}");
            }
        }

        Debug.Log("✅ Все данные сброшены к начальным значениям");
        AssetDatabase.SaveAssets();
    }

    // Автоматически срабатывает при входе в Play Mode
    [InitializeOnEnterPlayMode]
    static void OnEnterPlayMode(EnterPlayModeOptions options)
    {
        // Можно закомментировать, если не нужен автосброс
        ResetGameData();
    }
}
#endif