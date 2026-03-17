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
    
    // Данные для спавна
    private string targetSpawnId = "";
    
    // События
    public event System.Action OnPlayerDiedData;
    
    private GameData currentGame = new GameData();

    public GameData CurrentGame => currentGame;
    public string TargetSpawnId => targetSpawnId;

    public float PlayerHealth
    {
        get => currentGame.currentHealth;
        set
        {
            currentGame.currentHealth = Mathf.Clamp(value, 0, playerSettings != null ? playerSettings.MaxHealth : 100f);
            if (currentGame.currentHealth <= 0)
            {
                OnPlayerDiedData?.Invoke();
            }
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeManagers();
    }

    void InitializeManagers()
    {
        saveManager = GetComponent<SaveManager>();
        if (saveManager == null) saveManager = gameObject.AddComponent<SaveManager>();
        
        inventoryManager = GetComponent<InventoryManager>();
        if (inventoryManager == null) inventoryManager = gameObject.AddComponent<InventoryManager>();
        
        progressManager = GetComponent<ProgressManager>();
        if (progressManager == null) progressManager = gameObject.AddComponent<ProgressManager>();
    }

    // ===== УПРАВЛЕНИЕ СПАВНАМИ =====

    public void SetTargetSpawn(string spawnId)
    {
        targetSpawnId = spawnId;
        Debug.Log($"DataCoordinator: установлен целевой спавн {spawnId}");
    }

    public void ClearTargetSpawn()
    {
        targetSpawnId = "";
    }

    // ===== ОБРАБОТКА ЗАГРУЗКИ СЦЕНЫ =====

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"DataCoordinator: сцена загружена - {scene.name}");
        
        // Обновляем текущую сцену в данных
        currentGame.currentScene = scene.name;
        
        // Применяем данные к системам
        ApplyGameToAllSystems();
        
        // Автосохранение
        if (autoSaveOnSceneChange)
        {
            SaveGame();
        }
    }

    // ===== СОХРАНЕНИЕ/ЗАГРУЗКА =====

    public void CreateNewGame()
    {
        currentGame.ResetToDefault();
        ApplyGameToAllSystems();
        Debug.Log("Новая игра создана");
    }

    public void SaveGame()
    {
        CollectCurrentState();
        saveManager.SaveToFile(currentGame);
        Debug.Log($"Игра сохранена. Здоровье: {currentGame.currentHealth}");
    }

    public void LoadGame()
    {
        var loaded = saveManager.LoadFromFile();
        if (loaded != null)
        {
            currentGame = loaded;
            
            if (!string.IsNullOrEmpty(currentGame.currentScene))
            {
                // Используем SceneLoader для загрузки сцены
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadScene(currentGame.currentScene, true);
                }
                else
                {
                    SceneManager.LoadScene(currentGame.currentScene);
                }
            }
        }
        else
        {
            CreateNewGame();
        }
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
        
        // Здоровье применится в PlayerHealth.Start()
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}