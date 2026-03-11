using System;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemDataSO itemData;
    public int quantity;

    public bool IsEmpty => itemData == null || quantity <= 0;

    public InventorySlot(ItemDataSO item = null, int qty = 1)
    {
        itemData = item;
        quantity = qty;
    }
}

// ТОЛЬКО ДЛЯ RUNTIME, НЕ СОХРАНЯЕТСЯ МЕЖДУ СЕССИЯМИ
[System.Serializable]
public class PlayerInventory
{
    [SerializeField] private InventorySlot[] slots = new InventorySlot[8];

    // События
    public event Action OnInventoryChanged;
    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    #region Свойства
    public InventorySlot[] Slots => slots;

    public int UsedSlots
    {
        get
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty) count++;
            }
            return count;
        }
    }

    public bool IsFull => UsedSlots >= 8;
    #endregion

    #region Конструктор
    public PlayerInventory()
    {
        // Просто инициализируем пустые слоты
        if (slots == null || slots.Length != 8)
        {
            slots = new InventorySlot[8];
        }

        for (int i = 0; i < 8; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = new InventorySlot();
            }
        }
    }
    #endregion

    #region Основные методы
    public bool AddItem(ItemDataSO item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        Debug.Log($"Добавляем: {item.itemName} x{quantity}");

        // 1. Если предмет стакается, ищем существующий
        if (item.isStackable)
        {
            for (int i = 0; i < 8; i++)
            {
                if (slots[i].itemData == item)
                {
                    slots[i].quantity += quantity;
                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }
        }

        // 2. Ищем пустой слот
        for (int i = 0; i < 8; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].itemData = item;
                slots[i].quantity = quantity;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        Debug.LogWarning($"Не удалось добавить {item.itemName} - инвентарь полон!");
        return false;
    }

    public bool HasItem(string itemId)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty && slot.itemData.itemId == itemId)
                return true;
        }
        return false;
    }

    public void ClearAll()
    {
        for (int i = 0; i < 8; i++)
        {
            slots[i] = new InventorySlot();
        }
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= 8) return;
        if (slots[slotIndex].IsEmpty) return;

        var item = slots[slotIndex].itemData;

        if (item.isStackable && slots[slotIndex].quantity > quantity)
        {
            slots[slotIndex].quantity -= quantity;
        }
        else
        {
            slots[slotIndex] = new InventorySlot();
        }

        OnInventoryChanged?.Invoke();
    }
    #endregion

    #region Дебаг
    public void PrintInventory()
    {
        Debug.Log("=== ИНВЕНТАРЬ ===");
        for (int i = 0; i < 8; i++)
        {
            if (slots[i].IsEmpty)
            {
                Debug.Log($"Слот {i}: [ПУСТО]");
            }
            else
            {
                Debug.Log($"Слот {i}: {slots[i].itemData.itemName} x{slots[i].quantity}");
            }
        }
    }
    #endregion
}