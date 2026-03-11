using UnityEngine;
using UnityEngine.SceneManagement;

public class DataCoordinator : MonoBehaviour
{
    public static DataCoordinator Instance { get; private set; }

    [SerializeField] private ItemDatabaseSO itemDatabase;
    [SerializeField] private bool autoSaveOnSceneChange = true;

    private SaveManager saveManager;
    private InventoryManager inventoryManager;
    private ProgressManager progressManager;

    private GameData currentGameData;  // ← GameData для сохранения в файл
    private PlayerDataSO playerDataTemplate; // ← ScriptableObject для runtime данных

    // Публичные свойства
    public PlayerInventory PlayerInventory => inventoryManager?.PlayerInventory;
    public PlayerDataSO PlayerData => playerDataTemplate; // ← PlayerHealth использует это

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Подписка на загрузку сцен
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeManagers();
        LoadOrCreateGame();
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

    void LoadOrCreateGame()
    {
        if (saveManager.SaveFileExists()) LoadGame();
        else CreateNewGame();
    }

    public void CreateNewGame()
    {
        currentGameData = new GameData();

        // Инициализируем ScriptableObject начальными значениями
        if (playerDataTemplate != null)
        {
            // ★★★ ВАЖНО: Инициализируем ScriptableObject (только для новой игры)
            playerDataTemplate.CurrentHealth = playerDataTemplate.MaxHealth; // если есть MaxHealth
            playerDataTemplate.LastCheckpointPosition = Vector2.zero;
            playerDataTemplate.LastCheckpointScene = "";
            playerDataTemplate.HasFlashlight = false;
            playerDataTemplate.FlashlightBattery = 100f;

            // Копируем в GameData
            currentGameData.currentHealth = playerDataTemplate.CurrentHealth;
            currentGameData.hasFlashlight = playerDataTemplate.HasFlashlight;
            currentGameData.flashlightBattery = playerDataTemplate.FlashlightBattery;
        }

        ApplyGameDataToRuntime();
        Debug.Log("Новая игра создана");
    }

    public void SaveGame()
    {
        if (currentGameData == null) return;

        // 1. Собираем текущие runtime данные
        CollectDataFromRuntime();

        // 2. Сохраняем в файл (GameData → JSON)
        saveManager.SaveToFile(currentGameData);

        // 3. ★★★ Синхронизируем ScriptableObject из GameData ★★★
        // (PlayerHealth читает из PlayerData, поэтому нужно обновить)
        if (playerDataTemplate != null)
        {
            // Только данные, которые нужны другим системам (например, PlayerHealth)
            playerDataTemplate.CurrentHealth = currentGameData.currentHealth;
            playerDataTemplate.LastCheckpointPosition = currentGameData.lastCheckpointPosition;
            playerDataTemplate.LastCheckpointScene = currentGameData.lastCheckpointScene;
            playerDataTemplate.HasFlashlight = currentGameData.hasFlashlight;
            playerDataTemplate.FlashlightBattery = currentGameData.flashlightBattery;
        }

        Debug.Log("Игра сохранена");
        Debug.Log($"Инвентарь: {GetInventoryItemCount()} предметов, Здоровье: {currentGameData.currentHealth}");
    }

    public void LoadGame()
    {
        currentGameData = saveManager.LoadFromFile();

        if (currentGameData == null)
        {
            CreateNewGame();
            return;
        }

        // Применяем загруженные данные
        ApplyGameDataToRuntime();

        // ★★★ Синхронизируем ScriptableObject из GameData ★★★
        if (playerDataTemplate != null)
        {
            playerDataTemplate.CurrentHealth = currentGameData.currentHealth;
            playerDataTemplate.LastCheckpointPosition = currentGameData.lastCheckpointPosition;
            playerDataTemplate.LastCheckpointScene = currentGameData.lastCheckpointScene;
            playerDataTemplate.HasFlashlight = currentGameData.hasFlashlight;
            playerDataTemplate.FlashlightBattery = currentGameData.flashlightBattery;
        }

        Debug.Log("Игра загружена");
        Debug.Log($"Инвентарь: {GetInventoryItemCount()} предметов, Здоровье: {currentGameData.currentHealth}");
    }

    public void DeleteSave()
    {
        saveManager.DeleteSaveFile();
        CreateNewGame();
    }

    void CollectDataFromRuntime()
    {
        // ★★★ ВСЕ ДАННЫЕ СОБИРАЕМ В GameData ★★★

        // 1. Здоровье (из PlayerHealth если есть)
        var playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            currentGameData.currentHealth = playerHealth.Health;
        }
        else if (playerDataTemplate != null)
        {
            // Запасной вариант
            currentGameData.currentHealth = playerDataTemplate.CurrentHealth;
        }

        // 2. Данные игрока
        if (Player1.Instance != null)
        {
            currentGameData.playerPosition = Player1.Instance.transform.position;
            currentGameData.playerLastDirection = Player1.Instance.LastMovementDirection;
            currentGameData.playerIsSprinting = Player1.Instance.IsSprinting;

            // Фонарик
            var flashlight = Player1.Instance.GetFlashlight();
            if (flashlight != null)
            {
                currentGameData.flashlightEnabled = flashlight.IsActive;
            }
        }

        // 3. Текущая сцена
        currentGameData.currentScene = SceneManager.GetActiveScene().name;

        // 4. Инвентарь
        inventoryManager.SaveToGameData(currentGameData);

        // 5. Прогресс
        progressManager.SaveToGameData(currentGameData);

        // 6. Данные из PlayerDataSO (чекпоинты)
        if (playerDataTemplate != null)
        {
            currentGameData.lastCheckpointPosition = playerDataTemplate.LastCheckpointPosition;
            currentGameData.lastCheckpointScene = playerDataTemplate.LastCheckpointScene;
            currentGameData.hasFlashlight = playerDataTemplate.HasFlashlight;
        }
    }

    void ApplyGameDataToRuntime()
    {
        // ★★★ ПРИМЕНЯЕМ ДАННЫЕ ИЗ GameData ★★★

        // 1. Инвентарь (первым - нужен для других систем)
        inventoryManager.LoadFromGameData(currentGameData, itemDatabase);

        // 2. Прогресс
        progressManager.LoadFromGameData(currentGameData);

        // 3. Позиция игрока
        if (Player1.Instance != null && currentGameData.playerPosition != Vector3.zero)
        {
            Player1.Instance.transform.position = currentGameData.playerPosition;
        }

        // 4. Фонарик
        if (Player1.Instance != null)
        {
            var flashlight = Player1.Instance.GetFlashlight();
            if (flashlight != null && currentGameData.flashlightEnabled != flashlight.IsActive)
            {
                flashlight.ToggleFlashlight();
            }
        }

        // 5. Здоровье - PlayerHealth сам возьмет из PlayerData
        // (PlayerData уже обновлен в LoadGame/SaveGame)
    }

    // Обратная совместимость
    public void MarkItemAsPermanentlyCollected(string itemId)
    {
        progressManager.MarkItemAsPermanentlyCollected(itemId);
        SaveGame();
    }

    public bool IsItemPermanentlyCollected(string itemId)
        => progressManager.IsItemPermanentlyCollected(itemId);

    public void MarkDialoguePointCompleted(string pointId)
        => progressManager.MarkDialoguePointCompleted(pointId);

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoSaveOnSceneChange) SaveGame();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Утилита для отладки
    int GetInventoryItemCount()
    {
        if (currentGameData == null) return 0;

        int count = 0;
        foreach (var slot in currentGameData.inventorySlots)
        {
            if (!slot.isEmpty) count++;
        }
        return count;
    }
}