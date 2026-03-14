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
        if (data == null || itemDatabase == null) return;

        playerInventory.ClearAll();

        for (int i = 0; i < data.inventorySlots.Length && i < playerInventory.Slots.Length; i++)
        {
            var slot = data.inventorySlots[i];
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
        if (data == null) return;

        for (int i = 0; i < playerInventory.Slots.Length && i < data.inventorySlots.Length; i++)
        {
            var slot = playerInventory.Slots[i];
            data.inventorySlots[i].isEmpty = slot.IsEmpty;

            if (!slot.IsEmpty)
            {
                data.inventorySlots[i].itemId = slot.itemData.itemId;
                data.inventorySlots[i].quantity = slot.quantity;
            }
        }
    }
}