using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public enum CutsceneType
{
    Opening,        // Вступление уровня (игрок не управляем)
    Transition,     // Переход между сценами (загрузка после)
    Event,          // Событийная (игрок временно не управляем)
    Flashback,      // Флешбек (пауза игры)
    Quick           // Быстрая (игрок продолжает управление)
}

public class CutsceneDirector : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private CutsceneType cutsceneType = CutsceneType.Event;

    [Header("Автозапуск")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float startDelay = 0f;

    [Header("Управление игроком")]
    [SerializeField] private bool disablePlayerControl = true;
    [SerializeField] private bool freezePlayerPosition = true;
    [SerializeField] private bool reenablePlayerAfter = true;
    [SerializeField] private float playerReenableDelay = 0.5f;

    [Header("Загрузка сцены")]
    [SerializeField] private bool loadSceneAfter = false;
    [SerializeField] private string sceneToLoad = "";
    [SerializeField] private float sceneLoadDelay = 1f;
    [SerializeField] private bool saveBeforeLoad = true;

    [Header("Одноразовость")]
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private string saveKey = ""; // Ключ для сохранения пройденности

    [Header("Триггерные зоны")]
    [SerializeField] private bool useTrigger = false;
    [SerializeField] private LayerMask triggerMask = ~0;

    private bool isPlaying = false;
    private bool hasBeenPlayed = false;
    private Vector3 playerFreezePosition;

    void Start()
    {
        // Автопоиск Timeline
        if (timeline == null)
            timeline = GetComponent<PlayableDirector>();

        // Проверяем сохранённую пройденность
        if (oneTimeOnly && !string.IsNullOrEmpty(saveKey))
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
        if (isPlaying || (oneTimeOnly && hasBeenPlayed)) return;
        if (timeline == null)
        {
            Debug.LogError($"CutsceneDirector {name}: Timeline не назначен!");
            return;
        }

        StartCoroutine(PlayCutsceneRoutine());
    }

    IEnumerator PlayCutsceneRoutine()
    {
        isPlaying = true;
        Debug.Log($"Запуск катсцены ({cutsceneType}): {name}");

        // ===== ПОДГОТОВКА =====
        switch (cutsceneType)
        {
            case CutsceneType.Opening:
                // Вступление: полностью отключаем игрока
                DisablePlayerCompletely();
                break;

            case CutsceneType.Transition:
                // Переход: сохраняем позицию для заморозки
                if (freezePlayerPosition && Player1.Instance != null)
                {
                    playerFreezePosition = Player1.Instance.transform.position;
                }
                DisablePlayerControl();
                break;

            case CutsceneType.Event:
                // Событие: временно отключаем управление
                DisablePlayerControl();
                break;

            case CutsceneType.Flashback:
                // Флешбек: ставим игру на паузу
                Time.timeScale = 0f;
                break;

            case CutsceneType.Quick:
                // Быстрая: не трогаем управление
                break;
        }

        // ===== ЗАПУСК TIMELINE =====
        timeline.Play();
        timeline.stopped += OnTimelineFinished;

        // ===== ЗАМОРОЗКА ИГРОКА (если нужно) =====
        if (freezePlayerPosition && Player1.Instance != null)
        {
            StartCoroutine(FreezePlayerRoutine());
        }

        // Отправляем событие начала
        SendMessage("OnCutsceneStarted", cutsceneType, SendMessageOptions.DontRequireReceiver);

        yield return null;
    }

    IEnumerator FreezePlayerRoutine()
    {
        var player = Player1.Instance;
        if (player == null) yield break;

        Vector3 startPos = player.transform.position;

        while (isPlaying)
        {
            if (freezePlayerPosition)
            {
                player.transform.position = startPos;
            }
            yield return null;
        }
    }

    void OnTimelineFinished(PlayableDirector director)
    {
        director.stopped -= OnTimelineFinished;
        isPlaying = false;

        // ===== ВОССТАНОВЛЕНИЕ =====
        switch (cutsceneType)
        {
            case CutsceneType.Flashback:
                Time.timeScale = 1f;
                break;
        }

        // Включаем игрока
        if (reenablePlayerAfter)
        {
            Invoke(nameof(EnablePlayerControl), playerReenableDelay);
        }

        // Загружаем сцену
        if (loadSceneAfter && !string.IsNullOrEmpty(sceneToLoad))
        {
            if (saveBeforeLoad && DataCoordinator.Instance != null)
            {
                DataCoordinator.Instance.SaveGame();
            }
            Invoke(nameof(LoadScene), sceneLoadDelay);
        }

        // Помечаем как пройденную
        if (oneTimeOnly)
        {
            hasBeenPlayed = true;
            if (!string.IsNullOrEmpty(saveKey))
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
            }
        }

        Debug.Log($"Катсцена завершена: {name}");
        SendMessage("OnCutsceneFinished", cutsceneType, SendMessageOptions.DontRequireReceiver);
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

    void DisablePlayerCompletely()
    {
        if (Player1.Instance == null) return;

        Player1.Instance.enabled = false;

        // Можно также отключить коллайдеры, гравитацию и т.д.
        var rb = Player1.Instance.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        var collider = Player1.Instance.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
    }

    void DisablePlayerControl()
    {
        if (disablePlayerControl && Player1.Instance != null)
        {
            Player1.Instance.enabled = false;
        }
    }

    void EnablePlayerControl()
    {
        if (Player1.Instance != null)
        {
            Player1.Instance.enabled = true;

            // Восстанавливаем физику если отключали
            var rb = Player1.Instance.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;

            var collider = Player1.Instance.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = true;
        }
    }

    void LoadScene()
    {
        Debug.Log($"CutsceneDirector: Загрузка {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }

    // ===== МЕТОДЫ ДЛЯ TIMELINE SIGNALS =====

    public void Signal_DisablePlayer() => DisablePlayerControl();
    public void Signal_EnablePlayer() => EnablePlayerControl();
    public void Signal_LoadScene(string scene) { sceneToLoad = scene; LoadScene(); }
    public void Signal_SetCutsceneType(int type) { cutsceneType = (CutsceneType)type; }

    // ===== EDITOR УТИЛИТЫ =====

#if UNITY_EDITOR
    [ContextMenu("Тест: Запустить катсцену")]
    void TestPlay() => Play();

    [ContextMenu("Тест: Перезагрузить катсцену")]
    void TestReset()
    {
        hasBeenPlayed = false;
        PlayerPrefs.DeleteKey(saveKey);
        Debug.Log("Катсцена сброшена");
    }
#endif
}