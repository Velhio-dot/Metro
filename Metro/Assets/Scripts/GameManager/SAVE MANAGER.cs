using UnityEngine;
using System.IO;

/// <summary>
/// Только сохранение/загрузка файлов. Никакой игровой логики.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Настройки сохранения")]
    [SerializeField] private string saveFileName = "savegame.json";
    [SerializeField] private bool useEncryption = false;

    private string saveFilePath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
        Debug.Log($"SaveManager: путь сохранения - {saveFilePath}");
    }

    // ===== ОСНОВНЫЕ МЕТОДЫ =====

    /// <summary>
    /// Сохранить данные в файл
    /// </summary>
    public void SaveToFile(GameData data)
    {
        if (data == null)
        {
            Debug.LogError("SaveManager: попытка сохранить null данные");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                json = SimpleEncrypt(json);
            }

            File.WriteAllText(saveFilePath, json);
            Debug.Log($"SaveManager: игра сохранена в {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: ошибка сохранения - {e.Message}");
        }
    }

    /// <summary>
    /// Загрузить данные из файла
    /// </summary>
    public GameData LoadFromFile()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.Log("SaveManager: файл сохранения не найден");
            return null;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);

            if (useEncryption)
            {
                json = SimpleDecrypt(json);
            }

            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("SaveManager: игра загружена");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: ошибка загрузки - {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Удалить файл сохранения
    /// </summary>
    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("SaveManager: файл сохранения удален");
        }
    }

    /// <summary>
    /// Проверить наличие сохранения
    /// </summary>
    public bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
    }

    /// <summary>
    /// Получить информацию о сохранении
    /// </summary>
    public string GetSaveInfo()
    {
        if (!SaveFileExists()) return "Нет сохранения";

        var fileInfo = new FileInfo(saveFilePath);
        return $"Сохранение: {fileInfo.LastWriteTime:g}, {fileInfo.Length / 1024}KB";
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

    private string SimpleEncrypt(string data)
    {
        // Базовая "защита" от случайного редактирования файла
        // В реальном проекте используй нормальное шифрование
        char[] array = data.ToCharArray();
        System.Array.Reverse(array);
        return new string(array);
    }

    private string SimpleDecrypt(string data)
    {
        char[] array = data.ToCharArray();
        System.Array.Reverse(array);
        return new string(array);
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Создать тестовое сохранение")]
    void TestCreateSave()
    {
        var testData = new GameData();
        testData.currentHealth = 75f;
        testData.lastCheckpointScene = "TestScene";
        SaveToFile(testData);
    }

    [ContextMenu("Тест: Загрузить и показать")]
    void TestLoadAndShow()
    {
        var data = LoadFromFile();
        if (data != null)
        {
            Debug.Log($"Загружено: здоровье={data.currentHealth}, сцена={data.lastCheckpointScene}");
        }
    }

    [ContextMenu("Тест: Удалить сохранение")]
    void TestDeleteSave() => DeleteSaveFile();
#endif
}