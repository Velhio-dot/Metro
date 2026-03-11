using UnityEngine;
using System.Collections.Generic;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private List<IInteractable> interactablesInRange = new List<IInteractable>();

    private void Update()
    {
        HandleInteraction();
        UpdateInteractablesList();

        if (showDebug && Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log(GetDebugInfo());
        }
        HandleInteraction();
        UpdateInteractablesList();

        // Быстрое сохранение по F5
        if (Input.GetKeyDown(KeyCode.F5) && InventoryManager.Instance != null)
        {
            DataCoordinator.Instance.SaveGame();
            Debug.Log("Быстрое сохранение выполнено!");
        }

        // Быстрая загрузка по F9 (опционально)
        if (Input.GetKeyDown(KeyCode.F9) && DataCoordinator.Instance != null)
        {
            DataCoordinator.Instance.LoadGame();
            Debug.Log("Загрузка сохранения!");
        }
    }

    // Обновляем список объектов в зоне каждый кадр
    private void UpdateInteractablesList()
    {
        interactablesInRange.Clear();

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableMask);

        foreach (Collider2D collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null && !interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Add(interactable);
            }
        }
    }

    private void HandleInteraction()
    {
        if (GameInput.Instance == null)
        {
            Debug.LogWarning("GameInput.Instance is null!");
            return;
        }

        // Используем новый универсальный метод
        if (GameInput.Instance.IsInteractPressed())
        {
            if (interactablesInRange.Count > 0)
            {
                IInteractable closest = GetClosestInteractable();
                if (closest != null)
                {
                    Debug.Log($"Взаимодействие с: {((MonoBehaviour)closest).name}");
                    closest.Interact();
                }
            }
            else
            {
                Debug.Log("Нет объектов для взаимодействия в радиусе");
            }
        }
    }

    // Находим ближайший интерактивный объект
    private IInteractable GetClosestInteractable()
    {
        if (interactablesInRange.Count == 0) return null;
        if (interactablesInRange.Count == 1) return interactablesInRange[0];

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (IInteractable interactable in interactablesInRange)
        {
            MonoBehaviour mono = interactable as MonoBehaviour;
            if (mono == null) continue;

            float distance = Vector2.Distance(transform.position, mono.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }

    // Информация для отладки
    private string GetDebugInfo()
    {
        string info = $"=== PlayerInteract Debug ===\n";
        info += $"Position: {transform.position}\n";
        info += $"Range: {interactRange}\n";
        info += $"Objects in range: {interactablesInRange.Count}\n";

        foreach (IInteractable interactable in interactablesInRange)
        {
            MonoBehaviour mono = interactable as MonoBehaviour;
            if (mono != null)
            {
                float distance = Vector2.Distance(transform.position, mono.transform.position);
                info += $" - {mono.name} ({distance:F1} units)\n";
            }
        }

        info += $"Input Status: {GameInput.Instance?.GetInputStatus()}\n";

        return info;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        // Рисуем линии к объектам в зоне
        if (Application.isPlaying && showDebug)
        {
            Gizmos.color = Color.yellow;
            foreach (IInteractable interactable in interactablesInRange)
            {
                MonoBehaviour mono = interactable as MonoBehaviour;
                if (mono != null)
                {
                    Gizmos.DrawLine(transform.position, mono.transform.position);
                }
            }
        }
    }
}