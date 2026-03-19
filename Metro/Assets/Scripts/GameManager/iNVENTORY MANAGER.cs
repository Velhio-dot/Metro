using UnityEngine;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int inventorySize = 8;

    public event Action OnInventoryChanged;

    private PlayerInventory playerInventory;

    public PlayerInventory PlayerInventory => playerInventory;
    public bool IsFull => playerInventory?.IsFull ?? false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerInventory = new PlayerInventory();
    }

    public bool AddItem(ItemDataSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        if (playerInventory.AddItem(item, quantity))
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public void LoadFromGameData(GameData data, ItemDatabaseSO itemDatabase)
    {
        Debug.Log("=== InventoryManager.LOAD FROM GAME DATA ===");
        if (data == null || itemDatabase == null)
        {
            Debug.LogError("data = null");
            return;
        }
        //Debug.Log($"data.inventorySlots.Length = {data.inventorySlots.Length}");
        //Debug.Log($"playerInventory.Slots.Length = {playerInventory.Slots.Length}");
        playerInventory.ClearAll();

        for (int i = 0; i < data.inventorySlots.Length && i < playerInventory.Slots.Length; i++)
        {
            var slot = data.inventorySlots[i];
            //Debug.Log($"Слот {i}: isEmpty={slot.isEmpty}, itemId={slot.itemId}, quantity={slot.quantity}");
            if (!slot.isEmpty)
            {
                var item = itemDatabase.GetItemById(slot.itemId);
                if (item != null)
                {
                    playerInventory.Slots[i].itemData = item;
                    playerInventory.Slots[i].quantity = slot.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public void SaveToGameData(GameData data)
    {
        //Debug.Log("=== InventoryManager.SAVE TO GAME DATA ===");
        if (data == null)
        {
            Debug.LogError("data = null");
            return;
        }
        //Debug.Log($"Сохраняем инвентарь. Использовано слотов: {playerInventory.UsedSlots}");


        for (int i = 0; i < playerInventory.Slots.Length && i < data.inventorySlots.Length; i++)
        {
            var slot = playerInventory.Slots[i];
            data.inventorySlots[i].isEmpty = slot.IsEmpty;

            if (!slot.IsEmpty)
            {

                data.inventorySlots[i].itemId = slot.itemData.itemId;
                data.inventorySlots[i].quantity = slot.quantity;
                Debug.Log($"Слот {i}: сохраняем {slot.itemData.itemName} (ID: {slot.itemData.itemId}) x{slot.quantity}");
            }
        }
    }
}