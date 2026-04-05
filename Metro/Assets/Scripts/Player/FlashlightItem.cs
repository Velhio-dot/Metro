using UnityEngine;

public class FlashlightItem : MonoBehaviour, IInteractable
{
    [Header("Flashlight Item Settings")]
    [SerializeField] private GameObject flashlightPrefab;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private Vector3 flashlightOffset = new Vector3(0.2f, 0.1f, 0);

    [Header("Новая система данных")]
    [SerializeField] private ItemDataSO flashlightItemData; // Ссылка на SO фонарика
    [SerializeField] private bool addToInventory = true; // Добавлять в инвентарь как предмет

    private bool isPickedUp = false;

    public void Interact()
    {
        if (isPickedUp) return;
        PickupFlashlight();
    }

    private void PickupFlashlight()
    {
        Debug.Log("Подобран фонарик!");
        isPickedUp = true;

        Player1 player = Player1.Instance;
        if (player == null)
        {
            Debug.LogError("Player1 instance not found!");
            return;
        }

        // 1. СОЗДАЕМ ФОНАРИК КАК ДОЧЕРНИЙ ОБЪЕКТ
        GameObject flashlightInstance = Instantiate(flashlightPrefab, player.transform);
        flashlightInstance.transform.localPosition = flashlightOffset;

        // 2. ПОЛУЧАЕМ КОМПОНЕНТ ФОНАРИКА
        Flashlight flashlightComponent = flashlightInstance.GetComponent<Flashlight>();
        if (flashlightComponent == null)
        {
            Debug.LogError("Flashlight component not found on prefab!");
            return;
        }

        // 3. ДАЕМ ФОНАРИК ИГРОКУ (старый метод)
        player.SetFlashlight(flashlightComponent);

        // 4. РАЗБЛОКИРОВКА В СИСТЕМЕ ПРОГРЕССА
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SetFlashlightUnlocked(true);
        }
        else
        {
            Debug.LogWarning("[FlashlightItem] ProgressManager.Instance не найден для разблокировки!");
        }

        // 6. ЗВУК И ВИЗУАЛЬНЫЕ ЭФФЕКТЫ
        PlayPickupEffects();

        // 7. УБИРАЕМ ПРЕДМЕТ ИЗ МИРА
        Destroy(gameObject);
    }

    

    

    private void PlayPickupEffects()
    {
        // Звук
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Можно добавить частицы
        // if (pickupParticles != null) Instantiate(pickupParticles, transform.position, Quaternion.identity);
    }

    // Метод для автоматического назначения ItemDataSO если забыли
#if UNITY_EDITOR
    void Reset()
    {
        // Попробуем найти подходящий ItemDataSO автоматически
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemDataSO Flashlight");
        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            flashlightItemData = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemDataSO>(path);

            if (flashlightItemData != null)
            {
                Debug.Log($"Автоматически назначен ItemDataSO: {flashlightItemData.name}");
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }
#endif

    private void OnDrawGizmos()
    {
        // Визуализация в редакторе
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);

        // Стрелка к игроку
        if (Player1.Instance != null)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawLine(transform.position, Player1.Instance.transform.position);
        }
    }

    void OnGUI()
    {
        // Дебаг информация в игре
        if (Debug.isDebugBuild && !isPickedUp)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.z > 0)
            {
                Rect labelRect = new Rect(screenPos.x - 50, Screen.height - screenPos.y - 30, 100, 20);
                GUI.Label(labelRect, "🔦 Фонарик");

                if (flashlightItemData != null)
                {
                    Rect descRect = new Rect(screenPos.x - 75, Screen.height - screenPos.y - 50, 150, 20);
                    GUI.Label(descRect, flashlightItemData.itemName);
                }
            }
        }
    }
}