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

    private void Start()
    {
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

        if (playerVisual == null)
        {
            playerVisual = GetComponentInChildren<PlayerVisual>();
            Debug.Log($"PlayerHealth: PlayerVisual найден автоматически - {playerVisual != null}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(0f, currentHealth);

        SaveToCoordinator();

        Debug.Log($"Player получил урон: {damage}, здоровье: {currentHealth}");

        if (currentHealth <= 0f || damage >= 999f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        SaveToCoordinator();

        Debug.Log($"Player healed. Health: {currentHealth}");
    }

    public void SetHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        SaveToCoordinator();
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        SaveToCoordinator();
        Debug.Log("Player fully healed");
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Player died!");

        if (playerVisual != null)
        {
            playerVisual.PlayDeathAnimation();
        }
        else
        {
            Debug.LogError("PlayerHealth: PlayerVisual не найден!");
        }

        if (TryGetComponent<Player1>(out Player1 player))
        {
            player.enabled = false;
        }

        if (TryGetComponent<Collider2D>(out Collider2D collider))
        {
            collider.enabled = false;
        }

        Invoke(nameof(ReloadScene), 3f);
    }

    private void SaveToCoordinator()
    {
        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.PlayerHealth = currentHealth;
        }
    }

    private void ReloadScene()
    {
        if (DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.ResetLevelProgressForRespawn();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Respawn()
    {
        isDead = false;
        RestoreFullHealth();

        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }

        Debug.Log("Player respawned");
    }
}
