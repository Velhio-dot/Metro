using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Цвета")]
    [SerializeField] private Color fadeColor = Color.black;

    private bool isFading = false;
    private bool isSceneLoadInProgress = false;

    private static SceneFader instance;
    public static SceneFader Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Если фейдер в CoreManager, он уже будет DontDestroyOnLoad
        }

        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false;
            // Устанавливаем начальный цвет — чисто черный, чтобы не было вспышки при загрузке
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // При загрузке НОВОЙ сцены — плавно проявляем её из черного
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    // Start() больше не нужен для FadeIn, так как есть OnSceneLoaded

    public IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        if (fadeImage == null || isFading)
        {
            yield break;
        }

        isFading = true;
        SetInputBlocker(true);
        fadeImage.color = fadeColor;
        fadeImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.gameObject.SetActive(false);
        SetInputBlocker(false);
        isFading = false;

        // Возвращаем управление игроку после проявления сцены
        if (Player1.Instance != null && !isSceneLoadInProgress)
        {
            Player1.Instance.SetControl(true);
        }
    }

    public IEnumerator FadeOut()
    {
        gameObject.SetActive(true);
        if (fadeImage == null || isFading)
        {
            yield break;
        }

        isFading = true;
        SetInputBlocker(true);
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        isFading = false;
    }

    public void LoadSceneWithFade(string sceneName, bool saveGame = true, string targetSpawn = "", Vector2 forceDirection = default)
    {
        gameObject.SetActive(true);
        if (isSceneLoadInProgress) return;
        StartCoroutine(FadeAndLoadScene(sceneName, saveGame, targetSpawn, forceDirection));
    }

    private IEnumerator FadeAndLoadScene(string sceneName, bool saveGame, string targetSpawn, Vector2 forceDirection)
    {
        if (string.IsNullOrEmpty(sceneName)) yield break;

        isSceneLoadInProgress = true;

        // 0. Захват управления и автоматический шаг
        if (Player1.Instance != null)
        {
            Player1.Instance.SetControl(false);
            if (forceDirection != Vector2.zero)
            {
                Player1.Instance.SetForceMove(forceDirection);
                yield return new WaitForSeconds(0.3f);
                Player1.Instance.SetForceMove(Vector2.zero);
            }
        }

        // 1. Сохранение данных перед переходом
        if (saveGame && DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.SaveGame();
            if (!string.IsNullOrEmpty(targetSpawn))
            {
                DataCoordinator.Instance.SetTargetSpawn(targetSpawn);
            }
        }

        // 2. Затемнение
        yield return StartCoroutine(FadeOut());

        // 3. Асинхронная загрузка
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // FadeIn сработает автоматически через OnSceneLoaded
        isSceneLoadInProgress = false;
    }

    public static void FadeToScene(string sceneName)
    {
        if (CoreManager.Instance != null && CoreManager.Instance.Fader != null)
        {
            CoreManager.Instance.Fader.LoadSceneWithFade(sceneName);
        }
        else
        {
            var fader = FindFirstObjectByType<SceneFader>();
            if (fader != null) fader.LoadSceneWithFade(sceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    private void SetInputBlocker(bool shouldBlock)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = shouldBlock;
        }
    }
}
