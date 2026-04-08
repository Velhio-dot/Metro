using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;

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

    private bool isTransitionInProgress;

    private void Start()
    {
        // Пытаемся найти фейдер через CoreManager или напрямую
        if (CoreManager.Instance != null && CoreManager.Instance.Fader != null)
        {
            sceneFader = CoreManager.Instance.Fader;
        }
        else if (sceneFader == null)
        {
            sceneFader = FindFirstObjectByType<SceneFader>();
        }

        UpdateLoadButtonState();
        Debug.Log("Главное меню готово");
    }

    private void UpdateLoadButtonState()
    {
        if (loadGameButton == null || loadButtonText == null)
        {
            return;
        }

        bool hasSave = CheckForSaveFile();
        loadGameButton.interactable = hasSave;
        loadButtonText.text = hasSave ? "ЗАГРУЗИТЬ ИГРУ" : "СОХРАНЕНИЙ НЕТ";
    }

    private bool CheckForSaveFile()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        return File.Exists(savePath);
    }

    public void OnNewGameClicked()
    {
        if (!TryBeginTransition())
        {
            return;
        }

        DeleteExistingSave();
        EnsureSaveSystemExists(true);

        LoadTargetScene(firstSceneName);
    }

    public void OnLoadGameClicked()
    {
        if (!CheckForSaveFile())
        {
            Debug.LogError("Нет сохранения!");
            return;
        }

        if (!TryBeginTransition())
        {
            return;
        }

        string sceneToLoad = GetSavedSceneName();
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = firstSceneName;
        }

        EnsureSaveSystemExists(false);
        LoadTargetScene(sceneToLoad);
    }

    public void OnQuitClicked()
    {
        if (!TryBeginTransition())
        {
            return;
        }

        StartCoroutine(DelayedQuit());
    }

    private bool TryBeginTransition()
    {
        if (isTransitionInProgress)
        {
            return false;
        }

        isTransitionInProgress = true;
        SetButtonsInteractable(false);
        return true;
    }

    private void LoadTargetScene(string sceneName)
    {
        if (sceneFader != null)
        {
            sceneFader.LoadSceneWithFade(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (newGameButton != null)
        {
            newGameButton.interactable = interactable;
        }

        if (loadGameButton != null)
        {
            loadGameButton.interactable = interactable;
        }

        if (quitButton != null)
        {
            quitButton.interactable = interactable;
        }
    }

    private IEnumerator DelayedQuit()
    {
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

    private void DeleteExistingSave()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Сохранение удалено");
        }
    }

    private string GetSavedSceneName()
    {
        string savePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        if (!File.Exists(savePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            int startIndex = json.IndexOf("\"sceneName\":\"");
            if (startIndex != -1)
            {
                startIndex += 13;
                int endIndex = json.IndexOf("\"", startIndex);
                return json.Substring(startIndex, endIndex - startIndex);
            }
        }
        catch
        {
        }

        return null;
    }

    private void EnsureSaveSystemExists(bool isNewGame)
    {
        if (DataCoordinator.Instance == null)
        {
            Debug.LogWarning("MainMenuManager: DataCoordinator не найден в сцене!");
            GameObject systems = new GameObject("Systems");
            systems.AddComponent<DataCoordinator>();
            DontDestroyOnLoad(systems);
        }

        if (isNewGame && DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.CreateNewGame();
        }
    }
}
