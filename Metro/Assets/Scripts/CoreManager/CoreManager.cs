using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }

    [Header("Менеджеры подсистем")]
    public DataCoordinator Data { get; private set; }
    public InventoryManager Inventory { get; private set; }
    public SaveManager Save { get; private set; }
    public ProgressManager Progress { get; private set; }
    public GameInput Input { get; private set; }
    public SceneFader Fader { get; private set; }

    [Header("UI Ссылки")]
    [SerializeField] private GameObject uiCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log($"[CoreManager] Уничтожен дубликат на сцене {SceneManager.GetActiveScene().name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject); // Теперь CoreManager НЕ удаляется при смене сцены!
        Debug.Log("[CoreManager] Инициализирован.");

        // Собираем все локальные подсистемы
        Data = GetComponentInChildren<DataCoordinator>();
        Inventory = GetComponentInChildren<InventoryManager>();
        Save = GetComponentInChildren<SaveManager>();
        Progress = GetComponentInChildren<ProgressManager>();
        Input = GetComponentInChildren<GameInput>();
        Fader = GetComponentInChildren<SceneFader>(true); // (true), чтобы найти даже в выключенном состоянии при старте
        if (Fader == null) Fader = FindFirstObjectByType<SceneFader>();

        if (Data == null) Debug.LogWarning("[CoreManager] DataCoordinator не найден!");
        if (Inventory == null) Debug.LogWarning("[CoreManager] InventoryManager не найден!");
        if (Save == null) Debug.LogWarning("[CoreManager] SaveManager не найден!");
        if (Progress == null) Debug.LogWarning("[CoreManager] ProgressManager не найден!");

        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateUIVisibility(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUIVisibility(scene.name);
    }

    private void UpdateUIVisibility(string sceneName)
    {
        if (uiCanvas == null) return;

        // Список сцен, где интерфейс должен быть скрыт (Главное меню)
        bool isMenu = sceneName == "GameMenu" || sceneName == "Menu" || sceneName == "MainMenu";
        uiCanvas.SetActive(!isMenu);

        Debug.Log($"[CoreManager] UI {(isMenu ? "скрыт" : "показан")} для сцены: {sceneName}");
    }
}
