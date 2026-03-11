using UnityEngine;
using UnityEngine.Rendering.Universal;

public class KeycardPickup : MonoBehaviour, IInteractable
{
    [Header("Ключ-карта")]
    [SerializeField] private ItemDataSO keycardData;

    [Header("Эффекты активации")]
    [SerializeField] private GameObject[] objectsToActivate; // Что активировать при подборе
    [SerializeField] private GameObject[] objectsToDeactivate; // Что деактивировать

    [Header("Визуал")]
    [SerializeField] private Light2D keycardGlow;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private AudioClip keycardPickupSound;

    private bool isCollected = false;
    private float pulseTimer = 0f;

    void Start()
    {
        // Если предмет уже собран — удаляем
        if (ProgressManager.Instance != null &&
            ProgressManager.Instance.IsItemPermanentlyCollected(keycardData.itemId))
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Пульсация света если не собран
        if (!isCollected && keycardGlow != null)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float intensity = 0.5f + Mathf.Sin(pulseTimer) * 0.3f;
            keycardGlow.intensity = intensity;
        }
    }

    public void Interact()
    {
        if (isCollected) return;

        CollectKeycard();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectKeycard();
        }
    }

    void CollectKeycard()
    {
        if (isCollected || keycardData == null) return;

        isCollected = true;

        // 1. Добавляем в инвентарь
        if (InventoryManager.Instance != null)
        {
            bool added = InventoryManager.Instance.AddItem(keycardData);
            if (!added)
            {
                Debug.LogWarning("Не удалось добавить ключ-карту!");
                isCollected = false;
                return;
            }
        }

        // 2. Отмечаем как навсегда собранный
        ProgressManager.Instance.MarkItemAsPermanentlyCollected(keycardData.itemId);

        // 2. Активируем/деактивируем объекты
        foreach (var obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }

        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 3. Эффекты
        PlayPickupEffects();

        // 4. Уведомляем
        Debug.Log($"🔑 Ключ-карта '{keycardData.itemName}' получена!");

        // 5. Скрываем объект
        StartCoroutine(Disappear());
    }

    void PlayPickupEffects()
    {
        // Звук
        if (keycardPickupSound != null)
        {
            AudioSource.PlayClipAtPoint(keycardPickupSound, transform.position);
        }

        // Частицы
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Play();
        }

        // Отключаем свет
        if (keycardGlow != null)
        {
            keycardGlow.enabled = false;
        }
    }

    System.Collections.IEnumerator Disappear()
    {
        // Плавное исчезновение
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            float fadeTime = 0.5f;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                sprite.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
        }

        // Отключаем коллайдер
        GetComponent<Collider2D>().enabled = false;

        // Через секунду уничтожаем
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Рисуем связи к активируемым объектам
        Gizmos.color = Color.cyan;

      

        Gizmos.color = Color.red;

        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                Gizmos.DrawLine(transform.position, obj.transform.position);
                Gizmos.DrawWireCube(obj.transform.position, Vector3.one * 0.3f);
            }
        }

        // Сама ключ-карта
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}