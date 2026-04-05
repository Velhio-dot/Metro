using UnityEngine;
using UnityEngine.SceneManagement;

public class DataCoordinator : MonoBehaviour
{
    public static DataCoordinator Instance { get; private set; }

    [Header("Ссылки")]
    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private PlayerDataSO playerSettings;

    [Header("Настройки")]
    [SerializeField] private bool autoSaveOnSceneChange = true;

    private SaveManager saveManager;
    private InventoryManager inventoryManager;
    private ProgressManager progressManager;

    private string targetSpawnId = "";

    public event System.Action OnPlayerDiedData;

    private GameData currentGame = new GameData();

    public GameData CurrentGame => currentGame;
    public string TargetSpawnId => targetSpawnId;

    public float PlayerHealth
    {
        get => currentGame.currentHealth;
        set
        {
            float maxHealth = playerSettings != null ? playerSettings.MaxHealth : 100f;
            currentGame.currentHealth = Mathf.Clamp(value, 0f, maxHealth);
            if (currentGame.currentHealth <= 0f)
            {
                OnPlayerDiedData?.Invoke();
            }
        }
    }

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged; // Подписка на смену сцены
        InitializeManagers();
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // ПЕРЕД тем, как новая сцена начнет работать, мы собираем данные из старой
        if (!string.IsNullOrEmpty(oldScene.name))
        {
            CollectCurrentState();
        }
    }

    private bool TryInitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return false;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // : изненный цикл теперь управляется CoreManager!
        return true;
    }

    private void InitializeManagers()
    {
        saveManager = GetOrAddManager<SaveManager>();
        inventoryManager = GetOrAddManager<InventoryManager>();
        progressManager = GetOrAddManager<ProgressManager>();
    }

    private T GetOrAddManager<T>() where T : Component
    {
        var manager = GetComponent<T>();
        if (manager == null)
        {
            manager = gameObject.AddComponent<T>();
        }

        return manager;
    }

    public void SetTargetSpawn(string spawnId)
    {
        targetSpawnId = spawnId;
    }

    public void ClearTargetSpawn()
    {
        targetSpawnId = "";
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentGame.currentScene = scene.name;
        ApplyGameToAllSystems();
    }

    public void CreateNewGame()
    {
        currentGame.ResetToDefault();
        ApplyGameToAllSystems();
        Debug.Log("Новая игра создана");
    }

    public void ResetLevelProgressForRespawn()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        currentGame.ResetToDefault();
        currentGame.currentScene = activeSceneName;
        targetSpawnId = "";

        if (playerSettings != null)
        {
            currentGame.currentHealth = playerSettings.MaxHealth;
        }

        ApplyGameToAllSystems();
    }

    public void SaveGame()
    {
        if (saveManager == null)
        {
            return;
        }

        CollectCurrentState();
        saveManager.SaveToFile(currentGame);
    }

    public void LoadGame()
    {
        if (saveManager == null)
        {
            CreateNewGame();
            return;
        }

        GameData loaded = saveManager.LoadFromFile();
        if (loaded == null)
        {
            CreateNewGame();
            return;
        }

        currentGame = loaded;
        TryLoadSceneFromData();
    }

    private void CollectCurrentState()
    {
        if (Player1.Instance != null)
        {
            currentGame.playerPosition = Player1.Instance.transform.position;
            currentGame.playerLastDirection = Player1.Instance.LastMovementDirection;

            var playerHealth = Player1.Instance.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                currentGame.currentHealth = playerHealth.Health;
            }
        }

        currentGame.currentScene = SceneManager.GetActiveScene().name;
        inventoryManager?.SaveToGameData(currentGame);
        progressManager?.SaveToGameData(currentGame);
    }

    private void ApplyGameToAllSystems()
    {
        inventoryManager?.LoadFromGameData(currentGame, itemDatabase);
        progressManager?.LoadFromGameData(currentGame);
    }

    private void TryLoadSceneFromData()
    {
        if (string.IsNullOrEmpty(currentGame.currentScene))
        {
            return;
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(currentGame.currentScene, true);
            return;
        }

        SceneManager.LoadScene(currentGame.currentScene);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged; // Отписка
            Instance = null;
        }
    }
}
