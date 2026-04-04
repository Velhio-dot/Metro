using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleInventoryUI : MonoBehaviour
{
    [SerializeField] private Image[] slotImages;
    [SerializeField] private Sprite emptySlotSprite;

    private bool isSubscribed;

    private void OnEnable()
    {
        StartCoroutine(BindAndRefreshRoutine());
    }

    private IEnumerator BindAndRefreshRoutine()
    {
        while (InventoryManager.Instance == null || InventoryManager.Instance.PlayerInventory == null)
        {
            yield return null;
        }

        SubscribeIfNeeded();
        UpdateUI();
    }

    private void SubscribeIfNeeded()
    {
        if (isSubscribed || InventoryManager.Instance == null || InventoryManager.Instance.PlayerInventory == null)
        {
            return;
        }

        InventoryManager.Instance.PlayerInventory.OnInventoryChanged += UpdateUI;
        isSubscribed = true;
    }

    private void UnsubscribeIfNeeded()
    {
        if (!isSubscribed || InventoryManager.Instance == null || InventoryManager.Instance.PlayerInventory == null)
        {
            return;
        }

        InventoryManager.Instance.PlayerInventory.OnInventoryChanged -= UpdateUI;
        isSubscribed = false;
    }

    private void UpdateUI()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

        var inventory = InventoryManager.Instance.PlayerInventory;
        var slots = inventory.Slots;

        for (int i = 0; i < slotImages.Length && i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemData != null)
            {
                slotImages[i].sprite = slots[i].itemData.icon;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = emptySlotSprite;
                slotImages[i].color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    private void OnDisable()
    {
        UnsubscribeIfNeeded();
    }
}
