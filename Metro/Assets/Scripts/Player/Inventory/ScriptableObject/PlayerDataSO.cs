using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Статистика")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Позиция и прогресс")]
    [SerializeField] private Vector2 lastCheckpointPosition = Vector2.zero;
    [SerializeField] private string lastCheckpointScene = "";

    [Header("Фонарик")]
    [SerializeField] private bool hasFlashlight = false;
    [SerializeField] private float flashlightBattery = 100f;

    [Header("Навыки")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    // Свойства
    public float CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = Mathf.Clamp(value, 0, maxHealth);
    }

    public float MaxHealth => maxHealth;
    public Vector2 LastCheckpointPosition { get => lastCheckpointPosition; set => lastCheckpointPosition = value; }
    public string LastCheckpointScene { get => lastCheckpointScene; set => lastCheckpointScene = value; }
    public bool HasFlashlight { get => hasFlashlight; set => hasFlashlight = value; }
    public float FlashlightBattery { get => flashlightBattery; set => flashlightBattery = Mathf.Clamp(value, 0, 100); }
    public float MoveSpeed => moveSpeed;
    public float SprintMultiplier => sprintMultiplier;

    // Методы
    public void TakeDamage(float damage) => CurrentHealth -= damage;
    public void Heal(float amount) => CurrentHealth += amount;
    public void RestoreFullHealth() => currentHealth = maxHealth;

    public void ResetData()
    {
        currentHealth = maxHealth;
        lastCheckpointPosition = Vector2.zero;
        lastCheckpointScene = "";
        hasFlashlight = false;
        flashlightBattery = 100f;
    }
    // В конец класса PlayerDataSO добавь:
    public void ApplyGameData(GameData data)
    {
        if (data == null) return;

        currentHealth = data.currentHealth;
        lastCheckpointPosition = data.lastCheckpointPosition;
        lastCheckpointScene = data.lastCheckpointScene;
        hasFlashlight = data.hasFlashlight;
        flashlightBattery = data.flashlightBattery;

        // Здесь мы НЕ меняем maxHealth, moveSpeed и т.д. - это настройки шаблона
    }
    
    
}