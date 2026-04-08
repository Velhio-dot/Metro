using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int inventorySize = 8;

    public event Action OnInventoryChanged;

    private PlayerInventory playerInventory;

    public PlayerInventory PlayerInventory => playerInventory;
    public bool IsFull => playerInventory?.IsFull ?? false;

    private void Awake()
    {
        if (!TryInitializeSingleton())
        {
            return;
        }

        EnsureInventoryCreated();
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

    private void EnsureInventoryCreated()
    {
        if (playerInventory == null)
        {
            playerInventory = new PlayerInventory();
        }
    }

    public bool AddItem(ItemDataSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0)
        {
            return false;
        }

        EnsureInventoryCreated();

        bool added = playerInventory.AddItem(item, quantity);
        if (added)
        {
            OnInventoryChanged?.Invoke();
        }

        return added;
    }

    public void LoadFromGameData(GameData data, ItemDatabaseSO itemDatabase)
    {
        if (!CanLoadFromData(data, itemDatabase))
        {
            return;
        }

        EnsureInventoryCreated();
        playerInventory.ClearAll();

        int maxSlotsToCopy = Mathf.Min(data.inventorySlots.Length, playerInventory.Slots.Length);
        for (int i = 0; i < maxSlotsToCopy; i++)
        {
            var sourceSlot = data.inventorySlots[i];
            if (sourceSlot.isEmpty)
            {
                continue;
            }

            ItemDataSO item = itemDatabase.GetItemById(sourceSlot.itemId);
            if (item == null)
            {
                continue;
            }

            playerInventory.Slots[i].itemData = item;
            playerInventory.Slots[i].quantity = sourceSlot.quantity;
        }

        // Р вЂ™Р В°Р В¶Р Р…Р С•: РЎРѓР В»Р С•РЎвЂљРЎвЂ№ Р В·Р В°Р С—Р С•Р В»Р Р…РЎРЏРЎР‹РЎвЂљРЎРѓРЎРЏ Р Р…Р В°Р С—РЎР‚РЎРЏР СРЎС“РЎР‹, Р С—Р С•РЎРЊРЎвЂљР С•Р СРЎС“ Р Р†РЎР‚РЎС“РЎвЂЎР Р…РЎС“РЎР‹ РЎС“Р Р†Р ВµР Т‘Р С•Р СР В»РЎРЏР ВµР С UI Р С—Р С•РЎРѓР В»Р Вµ Р В·Р В°Р С–РЎР‚РЎС“Р В·Р С”Р С‘.
        playerInventory.NotifyInventoryChanged();
        OnInventoryChanged?.Invoke();
    }

    public void SaveToGameData(GameData data)
    {
        if (data == null)
        {
            return;
        }

        EnsureInventoryCreated();

        int maxSlotsToCopy = Mathf.Min(playerInventory.Slots.Length, data.inventorySlots.Length);
        for (int i = 0; i < maxSlotsToCopy; i++)
        {
            var sourceSlot = playerInventory.Slots[i];
            var targetSlot = data.inventorySlots[i];

            targetSlot.isEmpty = sourceSlot.IsEmpty;
            if (sourceSlot.IsEmpty)
            {
                targetSlot.itemId = "";
                targetSlot.quantity = 0;
                continue;
            }

            targetSlot.itemId = sourceSlot.itemData.itemId;
            targetSlot.quantity = sourceSlot.quantity;
        }
    }

    private static bool CanLoadFromData(GameData data, ItemDatabaseSO itemDatabase)
    {
        if (data == null || itemDatabase == null)
        {
            return false;
        }

        if (data.inventorySlots == null)
        {
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
