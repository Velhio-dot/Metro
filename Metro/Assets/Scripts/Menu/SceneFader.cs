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

    public bool IsBusy => isFading || isSceneLoadInProgress;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.raycastTarget = false;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
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
    }

    public IEnumerator FadeOut()
    {
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

    public void LoadSceneWithFade(string sceneName)
    {
        if (isSceneLoadInProgress)
        {
            return;
        }

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            isSceneLoadInProgress = false;
            yield break;
        }

        isSceneLoadInProgress = true;

        while (isFading)
        {
            yield return null;
        }

        if (fadeImage == null)
        {
            isSceneLoadInProgress = false;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            yield break;
        }

        yield return StartCoroutine(FadeOut());

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public static void FadeToScene(string sceneName)
    {
        var fader = FindObjectOfType<SceneFader>();
        if (fader != null)
        {
            fader.LoadSceneWithFade(sceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    private void SetInputBlocker(bool shouldBlock)
    {
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = shouldBlock;
        }
    }

    private void OnDisable()
    {
        isFading = false;
        isSceneLoadInProgress = false;
        SetInputBlocker(false);
    }
}
