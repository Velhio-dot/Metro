using UnityEngine;
using UnityEngine.SceneManagement;

public class DataCoordinator : MonoBehaviour
{
    public static DataCoordinator Instance { get; private set; }

    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private PlayerDataSO playerSettings;  // Только настройки!
    [SerializeField] private bool autoSaveOnSceneChange = true;

    private SaveManager saveManager;
    private InventoryManager inventoryManager;
    private ProgressManager progressManager;

    // ТЕКУЩИЕ ДАННЫЕ ИГРЫ (единственный источник)
    private GameData currentGame = new GameData();

    // Публичные свойства для доступа из других скриптов
    public GameData CurrentGame => currentGame;

    // Для обратной совместимости с вашими скриптами
    public PlayerInventory PlayerInventory => inventoryManager?.PlayerInventory;

    // Здоровье теперь прямо из GameData
    public float PlayerHealth
    {
        get => currentGame.currentHealth;
        set
        {
            currentGame.currentHealth = Mathf.Clamp(value, 0, playerSettings.MaxHealth);

            // Если здоровье упало до 0, можно добавить логику смерти
            if (currentGame.currentHealth <= 0)
            {
                Debug.Log("Player health reached 0 in DataCoordinator");
            }
        }
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeManagers();
        LoadOrCreateGame();
    }

    void InitializeManagers()
    {
        saveManager = GetComponent<SaveManager>() ?? gameObject.AddComponent<SaveManager>();
        inventoryManager = GetComponent<InventoryManager>() ?? gameObject.AddComponent<InventoryManager>();
        progressManager = GetComponent<ProgressManager>() ?? gameObject.AddComponent<ProgressManager>();
    }

    void LoadOrCreateGame()
    {
        if (saveManager.SaveFileExists())
            LoadGame();
        else
            CreateNewGame();
    }

    public void CreateNewGame()
    {
        currentGame.ResetToDefault();
        ApplyGameToAllSystems();
        Debug.Log("Новая игра создана");
    }

    public void SaveGame()
    {
        // Собираем текущее состояние в currentGame
        CollectCurrentState();

        // Сохраняем в файл
        saveManager.SaveToFile(currentGame);

        Debug.Log($"Игра сохранена. Здоровье: {currentGame.currentHealth}");
    }

    public void LoadGame()
    {
        var loaded = saveManager.LoadFromFile();
        if (loaded != null)
        {
            currentGame = loaded;
            ApplyGameToAllSystems();
            Debug.Log($"Игра загружена. Здоровье: {currentGame.currentHealth}");
        }
        else
        {
            CreateNewGame();
        }
    }

    private void CollectCurrentState()
    {
        // Позиция игрока
        if (Player1.Instance != null)
        {
            currentGame.playerPosition = Player1.Instance.transform.position;
            currentGame.playerLastDirection = Player1.Instance.LastMovementDirection;
            currentGame.playerIsSprinting = Player1.Instance.IsSprinting;

            // Фонарик
            var flashlight = Player1.Instance.GetFlashlight();
            if (flashlight != null)
                currentGame.flashlightEnabled = flashlight.IsActive;
        }

        // Текущая сцена
        currentGame.currentScene = SceneManager.GetActiveScene().name;

        // Инвентарь
        inventoryManager?.SaveToGameData(currentGame);

        // Прогресс
        progressManager?.SaveToGameData(currentGame);
    }

    private void ApplyGameToAllSystems()
    {
        // Инвентарь
        inventoryManager?.LoadFromGameData(currentGame, itemDatabase);

        // Прогресс
        progressManager?.LoadFromGameData(currentGame);

        // Позиция игрока (если есть и не нулевая)
        if (Player1.Instance != null && currentGame.playerPosition != Vector3.zero)
        {
            Player1.Instance.transform.position = currentGame.playerPosition;
        }

        // Фонарик
        if (Player1.Instance != null)
        {
            var flashlight = Player1.Instance.GetFlashlight();
            if (flashlight != null && currentGame.flashlightEnabled != flashlight.IsActive)
                flashlight.ToggleFlashlight();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoSaveOnSceneChange) SaveGame();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Публичные методы
    public void MarkItemAsPermanentlyCollected(string itemId)
    {
        progressManager?.MarkItemAsPermanentlyCollected(itemId);
        SaveGame();
    }

    public bool IsItemPermanentlyCollected(string itemId)
        => progressManager?.IsItemPermanentlyCollected(itemId) ?? false;

    public void MarkDialoguePointCompleted(string pointId)
        => progressManager?.MarkDialoguePointCompleted(pointId);

    public void DeleteSave()
    {
        saveManager.DeleteSaveFile();
        CreateNewGame();
    }
}