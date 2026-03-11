using UnityEngine;
using System;

/// <summary>
/// Управление runtime инвентарем игрока
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Настройки инвентаря")]
    [SerializeField] private int inventorySize = 8;

    // События
    public event Action OnInventoryChanged;
    public event Action<ItemDataSO, int> OnItemAdded; // предмет, количество
    public event Action<ItemDataSO, int> OnItemRemoved;

    // Данные
    private PlayerInventory playerInventory;

    // Публичные свойства
    public PlayerInventory PlayerInventory => playerInventory;
    public int UsedSlots => playerInventory?.UsedSlots ?? 0;
    public bool IsFull => playerInventory?.IsFull ?? false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeInventory();
    }

    void InitializeInventory()
    {
        playerInventory = new PlayerInventory();
        Debug.Log($"InventoryManager: инвентарь инициализирован ({inventorySize} слотов)");
    }

    // ===== ОСНОВНЫЕ МЕТОДЫ =====

    /// <summary>
    /// Добавить предмет в инвентарь
    /// </summary>
    public bool AddItem(ItemDataSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            Debug.LogWarning("InventoryManager: попытка добавить null предмет или количество ≤ 0");
            return false;
        }

        bool success = playerInventory.AddItem(item, quantity);

        if (success)
        {
            Debug.Log($"InventoryManager: добавлен {item.itemName} x{quantity}");
            OnItemAdded?.Invoke(item, quantity);
            OnInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"InventoryManager: не удалось добавить {item.itemName} (инвентарь полон?)");
        }

        return success;
    }

    /// <summary>
    /// Проверить наличие предмета по ID
    /// </summary>
    public bool HasItem(string itemId)
    {
        return playerInventory.HasItem(itemId);
    }

    /// <summary>
    /// Проверить наличие предмета
    /// </summary>
    public bool HasItem(ItemDataSO item)
    {
        if (item == null) return false;
        return HasItem(item.itemId);
    }

    /// <summary>
    /// Удалить предмет из слота
    /// </summary>
    public void RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
        {
            Debug.LogError($"InventoryManager: неверный индекс слота {slotIndex}");
            return;
        }

        var slot = playerInventory.Slots[slotIndex];
        if (slot.IsEmpty) return;

        var item = slot.itemData;
        playerInventory.RemoveItem(slotIndex, quantity);

        Debug.Log($"InventoryManager: удален {item.itemName} x{quantity} из слота {slotIndex}");
        OnItemRemoved?.Invoke(item, quantity);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Удалить предмет по ID
    /// </summary>
    public bool RemoveItemById(string itemId, int quantity = 1)
    {
        for (int i = 0; i < playerInventory.Slots.Length; i++)
        {
            var slot = playerInventory.Slots[i];
            if (!slot.IsEmpty && slot.itemData.itemId == itemId)
            {
                RemoveItem(i, quantity);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Получить количество предмета
    /// </summary>
    public int GetItemCount(string itemId)
    {
        int count = 0;
        foreach (var slot in playerInventory.Slots)
        {
            if (!slot.IsEmpty && slot.itemData.itemId == itemId)
            {
                count += slot.quantity;
            }
        }
        return count;
    }

    /// <summary>
    /// Очистить весь инвентарь
    /// </summary>
    public void ClearInventory()
    {
        playerInventory.ClearAll();
        Debug.Log("InventoryManager: инвентарь очищен");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Загрузить инвентарь из сохраненных данных
    /// </summary>
    public void LoadFromGameData(GameData data, ItemDatabaseSO itemDatabase)
    {
        if (data == null || itemDatabase == null)
        {
            Debug.LogError("InventoryManager: не могу загрузить - нет данных или базы предметов");
            return;
        }

        ClearInventory();

        for (int i = 0; i < data.inventorySlots.Length && i < playerInventory.Slots.Length; i++)
        {
            var savedSlot = data.inventorySlots[i];
            if (!savedSlot.isEmpty && !string.IsNullOrEmpty(savedSlot.itemId))
            {
                ItemDataSO item = itemDatabase.GetItemById(savedSlot.itemId);
                if (item != null)
                {
                    playerInventory.Slots[i].itemData = item;
                    playerInventory.Slots[i].quantity = savedSlot.quantity;
                }
            }
        }

        Debug.Log("InventoryManager: инвентарь загружен из сохранения");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Сохранить инвентарь в данные
    /// </summary>
    public void SaveToGameData(GameData data)
    {
        if (data == null) return;

        for (int i = 0; i < playerInventory.Slots.Length && i < data.inventorySlots.Length; i++)
        {
            var slot = playerInventory.Slots[i];
            data.inventorySlots[i].isEmpty = slot.IsEmpty;

            if (!slot.IsEmpty && slot.itemData != null)
            {
                data.inventorySlots[i].itemId = slot.itemData.itemId;
                data.inventorySlots[i].quantity = slot.quantity;
            }
            else
            {
                data.inventorySlots[i].itemId = "";
                data.inventorySlots[i].quantity = 0;
            }
        }

        Debug.Log("InventoryManager: инвентарь сохранен в данные");
    }

    // ===== УТИЛИТЫ =====

    /// <summary>
    /// Найти слот с предметом
    /// </summary>
    public int FindSlotWithItem(string itemId)
    {
        for (int i = 0; i < playerInventory.Slots.Length; i++)
        {
            var slot = playerInventory.Slots[i];
            if (!slot.IsEmpty && slot.itemData.itemId == itemId)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Найти первый пустой слот
    /// </summary>
    public int FindEmptySlot()
    {
        for (int i = 0; i < playerInventory.Slots.Length; i++)
        {
            if (playerInventory.Slots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Получить информацию об инвентаре
    /// </summary>
    public string GetInventoryInfo()
    {
        return $"Инвентарь: {UsedSlots}/{inventorySize} слотов";
    }

    /// <summary>
    /// Распечатать инвентарь в консоль
    /// </summary>
    public void PrintInventory()
    {
        playerInventory?.PrintInventory();
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Добавить случайный предмет")]
    

    [ContextMenu("Тест: Очистить инвентарь")]
    void TestClear() => ClearInventory();

    [ContextMenu("Тест: Показать информацию")]
    void TestInfo() => PrintInventory();
#endif
}