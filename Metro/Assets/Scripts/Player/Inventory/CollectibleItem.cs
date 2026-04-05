using UnityEngine;

public class UltraSimpleCollectible : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private int amount = 1;
    [SerializeField] private bool unlocksFlashlight = false; // Если TRUE, разблокирует фонарик вProgressManager
    [SerializeField] private string persistentId; // Уникальный ID для сохранения факта сбора

    private bool isBeingPickedUp = false;

    private void Start()
    {
        if (!string.IsNullOrEmpty(persistentId) && ProgressManager.Instance != null)
        {
            if (ProgressManager.Instance.IsItemPermanentlyCollected(persistentId))
            {
                Debug.Log($"[Persistent] Предмет {persistentId} уже был собран. Уничтожение.");
                Destroy(gameObject);
            }
        }
    }

    // Для взаимодействия через кнопку (например, 'E')
    public void Interact()
    {
        HandlePickup();
    }

    // Для взаимодействия через триггер (вход в зону)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePickup();
        }
    }

    private void HandlePickup()
    {
        if (isBeingPickedUp) return;
        isBeingPickedUp = true;

        // 1. Добавление в инвентарь
        if (itemData != null && InventoryManager.Instance != null)
        {
            for (int i = 0; i < amount; i++)
            {
                InventoryManager.Instance.PlayerInventory.AddItem(itemData);
            }
            Debug.Log($"Подобран: {itemData.itemName} x{amount}");
        }

        // 2. Разблокировка фонарика
        if (unlocksFlashlight)
        {
            Debug.Log($"[Collectible] Предмет {name} разблокирует фонарик...");
            if (ProgressManager.Instance != null)
            {
                ProgressManager.Instance.SetFlashlightUnlocked(true);
            }
        }

        // 3. Пометка в глобальном прогрессе
        if (!string.IsNullOrEmpty(persistentId) && ProgressManager.Instance != null)
        {
            ProgressManager.Instance.MarkItemAsPermanentlyCollected(persistentId);
        }

        // 4. Удаление из мира
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}