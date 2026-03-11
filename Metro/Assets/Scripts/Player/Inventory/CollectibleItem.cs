using UnityEngine;

public class UltraSimpleCollectible : MonoBehaviour
{
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private int amount = 1;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && itemData != null)
        {
            // Пробуем добавить в инвентарь
            if (InventoryManager.Instance != null)
            {
                for (int i = 0; i < amount; i++)
                {
                    InventoryManager.Instance.PlayerInventory.AddItem(itemData);
                }

                Debug.Log($"Подобран: {itemData.itemName} x{amount}");
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}