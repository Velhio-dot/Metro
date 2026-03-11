using UnityEngine;
using UnityEngine.UI;

public class SimpleInventoryUI : MonoBehaviour
{
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Sprite emptySlotSprite;

    void Start()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("GameDataManager не найден!");
            return;
        }

        // Подписываемся на изменения инвентаря
        InventoryManager.Instance.PlayerInventory.OnInventoryChanged += UpdateUI;
        UpdateUI(); // Первоначальное обновление
    }

    void UpdateUI()
    {
        if (InventoryManager.Instance == null) return;

        var inventory = InventoryManager.Instance.PlayerInventory;
        var slots = inventory.Slots;

        for (int i = 0; i < slotImages.Length && i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemData != null)
            {
                // Заполненный слот
                slotImages[i].sprite = slots[i].itemData.icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                // Пустой слот
                slotImages[i].sprite = emptySlotSprite;
                slotImages[i].color = new Color(1, 1, 1, 0.3f);
            }
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.PlayerInventory.OnInventoryChanged -= UpdateUI;
        }
    }
}