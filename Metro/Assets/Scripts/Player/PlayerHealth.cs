using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Visual Reference")]
    [SerializeField] private PlayerVisual playerVisual; // ← ДОБАВЬ ССЫЛКУ

    private bool isDead = false;

    public float Health => currentHealth;
    public bool IsDead => isDead;

    void Start()
    {
        if (DataCoordinator.Instance != null && DataCoordinator.Instance.PlayerData != null)
        {
            currentHealth = DataCoordinator.Instance.PlayerData.CurrentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        // Автоматически находим PlayerVisual если не назначен
        if (playerVisual == null)
        {
            playerVisual = GetComponentInChildren<PlayerVisual>();
            Debug.Log($"PlayerHealth: PlayerVisual найден автоматически - {playerVisual != null}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        
        //if (DataCoordinator.Instance != null)
        //{
        //    DataCoordinator.Instance.PlayerData.CurrentHealth = currentHealth;
        //}
        // Проверяем на смерть (включая мгновенную)
        if (currentHealth <= 0 || damage >= 999f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Запускаем анимацию смерти через PlayerVisual
        if (playerVisual != null)
        {
            playerVisual.PlayDeathAnimation(); // ← ДОБАВЬ ЭТОТ МЕТОД В PlayerVisual
        }
        else
        {
            Debug.LogError("PlayerHealth: PlayerVisual не найден!");
        }

        // Отключаем управление
        if (TryGetComponent<Player1>(out Player1 player))
        {
            player.enabled = false;
        }

        // Отключаем коллайдеры чтобы провалиться сквозь землю
        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

        // Отключаем инвентарь и другие компоненты если есть
       

        // Перезагружаем сцену через 3 секунды
        Invoke(nameof(ReloadScene), 3f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Для лечения или установки здоровья извне
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed. Health: {currentHealth}");
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        Debug.Log($"Health set to: {currentHealth}");
    }
}