using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [Header("Кнопки")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Текст кнопки загрузки")]
    [SerializeField] private TextMeshProUGUI loadButtonText;

    [Header("Настройки")]
    [SerializeField] private string firstSceneName = "Level1";

    [Header("Фейд (опционально)")]
    [SerializeField] private SceneFader sceneFader;

    void Start()
    {
        // Находим фейдер если не назначен
        if (sceneFader == null)
        {
            sceneFader = FindObjectOfType<SceneFader>();
        }

        UpdateLoadButtonState();
        Debug.Log("Главное меню готово");
    }

    void UpdateLoadButtonState()
    {
        if (loadGameButton == null || loadButtonText == null) return;

        bool hasSave = CheckForSaveFile();
        loadGameButton.interactable = hasSave;
        loadButtonText.text = hasSave ? "ЗАГРУЗИТЬ ИГРУ" : "СОХРАНЕНИЙ НЕТ";
    }

    bool CheckForSaveFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        return File.Exists(savePath);
    }

    // === МЕТОДЫ ДЛЯ КНОПОК ===

    public void OnNewGameClicked()
    {
        Debug.Log("🆕 Новая игра");

        // Блокируем кнопки на время перехода
        SetButtonsInteractable(false);

        // Удаляем сохранение
        DeleteExistingSave();

        // Создаем/сбрасываем GameSaveSystem
        EnsureSaveSystemExists(true);

        // Загружаем сцену с фейдом или без
        if (sceneFader != null)
        {
            sceneFader.LoadSceneWithFade(firstSceneName);
        }
        else
        {
            SceneManager.LoadScene(firstSceneName);
        }
    }

    public void OnLoadGameClicked()
    {
        Debug.Log("🔄 Загрузка игры");

        if (!CheckForSaveFile())
        {
            Debug.LogError("Нет сохранения!");
            return;
        }

        SetButtonsInteractable(false);

        // Определяем сцену
        string sceneToLoad = GetSavedSceneName();
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = firstSceneName;
        }

        // Создаем GameSaveSystem
        EnsureSaveSystemExists(false);

        // Загружаем
        if (sceneFader != null)
        {
            sceneFader.LoadSceneWithFade(sceneToLoad);
        }
        else
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void OnQuitClicked()
    {
        Debug.Log("🚪 Выход");

        // Небольшая задержка перед выходом
        StartCoroutine(DelayedQuit());
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

    void SetButtonsInteractable(bool interactable)
    {
        if (newGameButton != null) newGameButton.interactable = interactable;
        if (loadGameButton != null) loadGameButton.interactable = interactable;
        if (quitButton != null) quitButton.interactable = interactable;
    }

    System.Collections.IEnumerator DelayedQuit()
    {
        SetButtonsInteractable(false);

        // Если есть фейдер - затемняем перед выходом
        if (sceneFader != null)
        {
            yield return sceneFader.StartCoroutine(sceneFader.FadeOut());
        }

        yield return new WaitForSeconds(0.5f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void DeleteExistingSave()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("🗑️ Сохранение удалено");
        }
    }

    string GetSavedSceneName()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (!File.Exists(savePath)) return null;

        try
        {
            string json = File.ReadAllText(savePath);

            // Простой парсинг JSON для имени сцены
            int startIndex = json.IndexOf("\"sceneName\":\"");
            if (startIndex != -1)
            {
                startIndex += 13; // Длина "\"sceneName\":\""
                int endIndex = json.IndexOf("\"", startIndex);
                return json.Substring(startIndex, endIndex - startIndex);
            }
        }
        catch
        {
            // Игнорируем ошибки
        }

        return null;
    }

    void EnsureSaveSystemExists(bool isNewGame)
    {
        // ★★★ ИСПОЛЬЗУЕМ DataCoordinator вместо GameSaveSystem ★★★
        if (DataCoordinator.Instance == null)
        {
            Debug.LogWarning("MainMenuManager: DataCoordinator не найден в сцене!");
            // Создаем минимальную систему если её нет
            GameObject systems = new GameObject("Systems");
            systems.AddComponent<DataCoordinator>();
            DontDestroyOnLoad(systems);
        }

        if (isNewGame && DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.CreateNewGame();
        }

        Debug.Log("MainMenu: используется DataCoordinator");
    }
}