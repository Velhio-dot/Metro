using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvas != null)
            {
                fadeCanvas.gameObject.SetActive(true);
                fadeCanvas.alpha = 0f;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, bool useFade = true)
    {
        if (isTransitioning) return;

        StartCoroutine(LoadSceneRoutine(sceneName, useFade));
    }

    public void LoadSceneWithSave(string sceneName, bool isNewGame)
    {
        if (isTransitioning) return;

        // ★★★ ИСПОЛЬЗУЕМ DataCoordinator вместо GameSaveSystem ★★★
        if (DataCoordinator.Instance != null)
        {
            if (isNewGame)
            {
                DataCoordinator.Instance.CreateNewGame();
            }
            else
            {
                DataCoordinator.Instance.SaveGame(); // Сохраняем перед переходом
            }
        }
        else
        {
            Debug.LogWarning("DataCoordinator не найден при загрузке сцены");
        }

        LoadScene(sceneName);
    }

    System.Collections.IEnumerator LoadSceneRoutine(string sceneName, bool useFade)
    {
        isTransitioning = true;

        // Затемнение
        if (useFade && fadeCanvas != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
        }

        // Загрузка сцены
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Осветление
        if (useFade && fadeCanvas != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvas.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
        }

        isTransitioning = false;
    }

    public void ReturnToMainMenu()
    {
        LoadScene("MainMenu");
    }
}