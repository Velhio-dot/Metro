using UnityEngine;
using UnityEngine.Playables;

/// <summary>
/// Простой триггер для катсцен на Timeline
/// Версия: 1.0 - Базовая функциональность
/// </summary>
public class SimpleCutsceneTrigger : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Триггерные зоны")]
    [SerializeField] private bool useTrigger = false;
    [SerializeField] private LayerMask triggerMask = ~0;

    [Header("Управление игроком")]
    [SerializeField] private bool disablePlayerControl = true;
    [SerializeField] private bool reEnablePlayerAfter = true;

    [Header("Одноразовость")]
    [SerializeField] private bool playOnce = true;
    [SerializeField] private string saveKey = ""; // Для сохранения пройденности

    [Header("Загрузка сцены")]
    [SerializeField] private bool loadSceneAfter = false;
    [SerializeField] private string sceneToLoad = "";
    [SerializeField] private float sceneLoadDelay = 1f;

    private bool hasBeenPlayed = false;
    private bool isPlaying = false;
    private Player1 player;

    void Start()
    {
        player = Player1.Instance;

        // Проверяем сохраненную пройденность
        if (playOnce && !string.IsNullOrEmpty(saveKey))
        {
            hasBeenPlayed = PlayerPrefs.GetInt(saveKey, 0) == 1;
        }

        // Автозапуск
        if (playOnStart && !hasBeenPlayed)
        {
            if (startDelay > 0)
                Invoke(nameof(Play), startDelay);
            else
                Play();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger || hasBeenPlayed || isPlaying) return;

        if (((1 << other.gameObject.layer) & triggerMask) != 0)
        {
            Play();
        }
    }

    public void Play()
    {
        if (isPlaying || (playOnce && hasBeenPlayed)) return;
        if (timeline == null)
        {
            Debug.LogError($"SimpleCutsceneTrigger: Timeline не назначен на {name}");
            return;
        }

        isPlaying = true;

        // Отключаем управление игроком
        if (disablePlayerControl && player != null)
        {
            player.enabled = false;
        }

        Debug.Log($"Запуск катсцены: {name}");

        // Запускаем Timeline
        timeline.Play();
        timeline.stopped += OnTimelineFinished;

        // Помечаем как запущенную
        if (playOnce)
        {
            hasBeenPlayed = true;
            if (!string.IsNullOrEmpty(saveKey))
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
            }
        }
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        director.stopped -= OnTimelineFinished;
        isPlaying = false;

        // Включаем игрока
        if (reEnablePlayerAfter && player != null)
        {
            player.enabled = true;
        }

        // Загружаем сцену если нужно
        if (loadSceneAfter && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (sceneLoadDelay > 0)
                Invoke(nameof(LoadScene), sceneLoadDelay);
            else
                LoadScene();
        }

        Debug.Log($"Катсцена завершена: {name}");
    }

    void LoadScene()
    {
        Debug.Log($"Загрузка сцены: {sceneToLoad}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    // Методы для Timeline Signals
    public void Signal_EnablePlayer() => player.enabled = true;
    public void Signal_DisablePlayer() => player.enabled = false;
    public void Signal_LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        LoadScene();
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Запустить катсцену")]
    void TestPlay() => Play();

    [ContextMenu("Тест: Сбросить катсцену")]
    void TestReset()
    {
        hasBeenPlayed = false;
        PlayerPrefs.DeleteKey(saveKey);
        Debug.Log("Катсцена сброшена");
    }
#endif

    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            string status = hasBeenPlayed ? "Пройдена" : "Готова";
            if (isPlaying) status = "Играет";

            GUI.Label(new Rect(10, 250, 300, 20), $"🎬 {name}: {status}");
        }
    }
}