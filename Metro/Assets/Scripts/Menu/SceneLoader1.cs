using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    private bool isTransitioning;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        InitializeFadeCanvas();
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        return true;
    }

    private void InitializeFadeCanvas()
    {
        if (fadeCanvas == null)
        {
            return;
        }

        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.alpha = 0f;
    }

    public void LoadScene(string sceneName, bool useFade = true)
    {
        if (isTransitioning)
        {
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneName, useFade));
    }

    public void LoadSceneWithSave(string sceneName, bool isNewGame)
    {
        if (isTransitioning)
        {
            return;
        }

        if (DataCoordinator.Instance != null)
        {
            if (isNewGame)
            {
                DataCoordinator.Instance.CreateNewGame();
            }
            else
            {
                DataCoordinator.Instance.SaveGame();
            }
        }
        else
        {
            Debug.LogWarning("DataCoordinator не найден при загрузке сцены");
        }

        LoadScene(sceneName);
    }

    private IEnumerator LoadSceneRoutine(string sceneName, bool useFade)
    {
        isTransitioning = true;

        if (useFade)
        {
            yield return Fade(0f, 1f);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        if (useFade)
        {
            yield return Fade(1f, 0f);
        }

        isTransitioning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeCanvas == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = to;
    }

    public void ReturnToMainMenu()
    {
        LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
