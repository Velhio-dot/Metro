// GameEvent.cs
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game Systems/Game Event")]
public class GameEvent : ScriptableObject
{
    [TextArea(3, 5)]
    [SerializeField] private string description;

    [Header("Event Responses")]
    public UnityEvent onEventTriggered;

    [Header("Conditions")]
    [SerializeField] private bool canTriggerMultipleTimes = true;
    [SerializeField] private int maxTriggerCount = 0; // 0 = unlimited

    private int triggerCount = 0;
    private bool hasTriggered = false;

    public void Trigger()
    {
        // Проверяем условия
        if (!CanTrigger()) return;

        // Вызываем событие
        onEventTriggered?.Invoke();

        // Обновляем счетчики
        triggerCount++;
        hasTriggered = true;

        Debug.Log($"Событие '{name}' сработало (раз: {triggerCount})");
    }

    public bool CanTrigger()
    {
        if (!canTriggerMultipleTimes && hasTriggered) return false;
        if (maxTriggerCount > 0 && triggerCount >= maxTriggerCount) return false;

        return true;
    }

    public void ResetEvent()
    {
        triggerCount = 0;
        hasTriggered = false;
        Debug.Log($"Событие '{name}' сброшено");
    }

    public string GetStatus()
    {
        return $"{name}: triggered {triggerCount} times, can trigger: {CanTrigger()}";
    }
}