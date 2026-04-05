using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneDirector : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private PlayableDirector director;
    [Header("Настройки")]
    [SerializeField] private bool playOnStart = false; // Если TRUE, запустится само при старте сцены
    [SerializeField] private bool disablePlayerOnStart = true;
    [SerializeField] private bool freezePlayerPosition = true;
    [SerializeField] private bool reenablePlayerAfter = true;
    [SerializeField] private float playerReenableDelay = 0.5f;

    [Header("Загрузка сцены")]
    [SerializeField] private bool loadSceneAfter = false;
    [SerializeField] private string sceneToLoad = "";
    [SerializeField] private float sceneLoadDelay = 1f;

    [Header("Одноразовость (Постоянная)")]
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private string cutsceneId = "intro_car_exit";

    private bool isPlaying = false;
    private bool hasBeenPlayed = false;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();

        if (oneTimeOnly && ProgressManager.Instance != null && !string.IsNullOrEmpty(cutsceneId))
        {
            hasBeenPlayed = ProgressManager.Instance.IsCutscenePlayed(cutsceneId);
        }
    }

    void Start()
    {
        if (hasBeenPlayed && oneTimeOnly)
        {
            Debug.Log($"[CutsceneDirector] Катсцена {cutsceneId} уже была проиграла. Пропускаем.");
            EnablePlayerControl();
            return;
        }

        // Запуск ТОЛЬКО если стоит галочка playOnStart
        if (playOnStart)
        {
            if (disablePlayerOnStart)
            {
                DisablePlayerControl();
                Invoke("Play", 0.5f);
            }
            else
            {
                Play();
            }
        }
    }

    public void Play()
    {
        if (director != null)
        {
            director.Play();
            isPlaying = true;

            if (oneTimeOnly && ProgressManager.Instance != null && !string.IsNullOrEmpty(cutsceneId))
            {
                ProgressManager.Instance.MarkCutsceneAsPlayed(cutsceneId);
            }

            director.stopped += OnCutsceneEnded;
        }
    }

    private void OnCutsceneEnded(PlayableDirector aDirector)
    {
        if (reenablePlayerAfter)
        {
            Invoke("EnablePlayerControl", playerReenableDelay);
        }

        if (loadSceneAfter)
        {
            Invoke("LoadScene", sceneLoadDelay);
        }

        isPlaying = false;
        director.stopped -= OnCutsceneEnded;
    }

    private void DisablePlayerControl()
    {
        if (Player1.Instance != null) Player1.Instance.enabled = false;
    }

    private void EnablePlayerControl()
    {
        if (Player1.Instance != null) Player1.Instance.enabled = true;
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"[CutsceneDirector] Запуск перехода в сцену: {sceneToLoad}");
            SceneFader.FadeToScene(sceneToLoad);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Запустить катсцену")]
    void TestPlay() => Play();

    [ContextMenu("Тест: Сбросить флаг")]
    void TestReset()
    {
        if (ProgressManager.Instance != null && !string.IsNullOrEmpty(cutsceneId))
        {
            ProgressManager.Instance.PlayedCutsceneIds.Remove(cutsceneId);
            Debug.Log("Катсцена сброшена");
        }
    }
#endif
}
