using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Visual Reference")]
    [SerializeField] private PlayerVisual playerVisual;

    private float currentHealth;
    private bool isDead = false;

    public float Health => currentHealth;
    public bool IsDead => isDead;

    void Start()
    {
        // Загружаем здоровье из DataCoordinator
        if (DataCoordinator.Instance != null)
        {
            currentHealth = DataCoordinator.Instance.PlayerHealth;
            Debug.Log($"PlayerHealth: загружено здоровье {currentHealth}");
        }
        else
        {
            currentHealth = maxHealth;
            Debug.LogWarning("PlayerHealth: DataCoordinator не найден, используется maxHealth");
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
        if (isDead || damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // Обновляем в DataCoordinator
        SaveToCoordinator();


        Debug.Log($"Player получил урон: {damage}, здоровье: {currentHealth}");

        // Проверяем на смерть
        if (currentHealth <= 0 || damage >= 999f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        // Обновляем в DataCoordinator
        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.PlayerHealth = currentHealth;
        }

        Debug.Log($"Player healed. Health: {currentHealth}");
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SaveToCoordinator();
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        SaveToCoordinator();
        Debug.Log("Player fully healed");
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        // Запускаем анимацию смерти через PlayerVisual
        if (playerVisual != null)
        {
            playerVisual.PlayDeathAnimation();
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

        // Отключаем коллайдеры
        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

        // Перезагружаем сцену через 3 секунды
        Invoke(nameof(ReloadScene), 3f);
    }

    private void SaveToCoordinator()
    {
        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.PlayerHealth = currentHealth;
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Respawn()
    {
        isDead = false;
        RestoreFullHealth();

       

        // Включаем коллайдер
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }

        Debug.Log("Player respawned");
    }
}







