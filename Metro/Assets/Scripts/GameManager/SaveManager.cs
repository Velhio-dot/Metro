using System.IO;
using UnityEngine;

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

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
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
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: ошибка сохранения - {e.Message}");
        }
    }

    public GameData LoadFromFile()
    {
        if (!File.Exists(saveFilePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(saveFilePath);
            if (useEncryption)
            {
                json = SimpleDecrypt(json);
            }

            return JsonUtility.FromJson<GameData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveManager: ошибка загрузки - {e.Message}");
            return null;
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }
    }

    public bool SaveFileExists()
    {
        return File.Exists(saveFilePath);
    }

    public string GetSaveInfo()
    {
        if (!SaveFileExists())
        {
            return "Нет сохранения";
        }

        var fileInfo = new FileInfo(saveFilePath);
        return $"Сохранение: {fileInfo.LastWriteTime:g}, {fileInfo.Length / 1024}KB";
    }

    private static string SimpleEncrypt(string data)
    {
        char[] array = data.ToCharArray();
        System.Array.Reverse(array);
        return new string(array);
    }

    private static string SimpleDecrypt(string data)
    {
        char[] array = data.ToCharArray();
        System.Array.Reverse(array);
        return new string(array);
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Создать тестовое сохранение")]
    private void TestCreateSave()
    {
        var testData = new GameData();
        testData.currentHealth = 75f;
        testData.lastCheckpointScene = "TestScene";
        SaveToFile(testData);
    }

    [ContextMenu("Тест: Загрузить и показать")]
    private void TestLoadAndShow()
    {
        var data = LoadFromFile();
        if (data != null)
        {
            Debug.Log($"Загружено: здоровье={data.currentHealth}, сцена={data.lastCheckpointScene}");
        }
    }

    [ContextMenu("Тест: Удалить сохранение")]
    private void TestDeleteSave()
    {
        DeleteSaveFile();
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
