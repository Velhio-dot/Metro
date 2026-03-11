using UnityEngine;

public class LightReaction : MonoBehaviour
{
    public enum LightReactionType
    {
        Fear,    // Убегает от света
        Aggressive, // Атакует при свете
        Neutral  // Без реакции
    }

    [Header("Light Reaction Settings")]
    public LightReactionType reactionType = LightReactionType.Fear;
    [SerializeField] private float reactionDistance = 10f;
    [SerializeField] private float fearSpeedMultiplier = 1.5f;
    [SerializeField] private float aggressionSpeedMultiplier = 1.3f;

    private Flashlight playerFlashlight;
    private EnemyAI enemyAI;

    private void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        if (Player1.Instance != null)
        {
            playerFlashlight = Player1.Instance.GetFlashlight();
        }
    }

    private void Update()
    {
        // LightReaction больше не меняет состояния напрямую
        // Вся логика теперь в EnemyAI.CalculateNewState()

        // Можно оставить только визуальные/звуковые эффекты если нужно
        HandleVisualEffects();
    }

    private void HandleVisualEffects()
    {
        // Добавь здесь визуальные эффекты при реакции на свет
        // Например: изменение цвета, частицы, звуки и т.д.

        if (playerFlashlight != null && playerFlashlight.IsEnemyInLight(transform))
        {
            // Враг в свете - можно добавить эффекты
            switch (reactionType)
            {
                case LightReactionType.Fear:
                    // Эффекты страха: дрожание, изменение цвета на испуганный
                    break;
                case LightReactionType.Aggressive:
                    // Эффекты агрессии: красное свечение, рычание
                    break;
                case LightReactionType.Neutral:
                    // Нейтральные эффекты
                    break;
            }
        }
        else
        {
            // Враг не в свете - убрать эффекты
        }
    }

    public void SetReactionType(LightReactionType newType)
    {
        reactionType = newType;
    }

    // Эти методы можно оставить если они используются где-то ещё, 
    // но они больше не нужны для основной логики AI
    public float GetFearSpeedMultiplier() => fearSpeedMultiplier;
    public float GetAggressionSpeedMultiplier() => aggressionSpeedMultiplier;
}