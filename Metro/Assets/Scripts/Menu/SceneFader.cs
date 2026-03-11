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

    void Awake()
    {
        // Автоматически находим Image если не назначен
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        // Убедимся что объект активен и готов
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false; // Чтобы клики проходили сквозь
        }
    }

    void Start()
    {
        // При старте - плавное появление
        StartCoroutine(FadeIn());
    }

    // Появление (из черного в прозрачный)
    public IEnumerator FadeIn()
    {
        if (fadeImage == null || isFading) yield break;

        isFading = true;
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
        isFading = false;
    }

    // Исчезновение (из прозрачного в черный)
    public IEnumerator FadeOut()
    {
        if (fadeImage == null || isFading) yield break;

        isFading = true;
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

    // Загрузка сцены с фейдом
    public void LoadSceneWithFade(string sceneName)
    {
        if (isFading) return;

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 1. Затемнение
        yield return StartCoroutine(FadeOut());

        // 2. Загрузка сцены
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);

        // 3. Осветление произойдет автоматически в Start() новой сцены
    }

    // Статический метод для легкого доступа
    public static void FadeToScene(string sceneName)
    {
        var fader = FindObjectOfType<SceneFader>();
        if (fader != null)
        {
            fader.LoadSceneWithFade(sceneName);
        }
        else
        {
            // Если фейдера нет - просто грузим сцену
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}