using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public abstract class BaseInteractable : MonoBehaviour, IInteractable
{
    [Header("Общие настройки")]
    [SerializeField] protected bool requireKeyPress = true;
    [SerializeField] protected bool oneTimeOnly = false;
    [SerializeField] protected bool autoTrigger = false;
    [SerializeField] protected float autoTriggerDelay = 0f;

    [Header("Визуальные подсказки")]
    [SerializeField] protected GameObject interactionHint;
    [SerializeField] protected bool showHintOnlyWhenClose = true;

    [Header("События")]
    public UnityEvent onInteractionStart;
    public UnityEvent onInteractionEnd;

    protected bool hasBeenUsed;
    protected bool playerInRange;
    protected bool isInteracting;
    protected Coroutine autoTriggerCoroutine;

    protected virtual void Start()
    {
        UpdateHintVisibility();
    }

    protected virtual void Update()
    {
        HandleAutoTrigger();
    }

    public virtual void Interact()
    {
        if (!CanStartInteraction(requireKeyPressNeeded: true))
        {
            return;
        }
        StartInteraction();
    }

    protected void HandleAutoTrigger()
    {
        if (!CanStartInteraction(requireKeyPressNeeded: false)) return;

        if (autoTriggerDelay <= 0f)
        {
            StartInteraction();
            return;
        }

        if (autoTriggerCoroutine == null)
        {
            autoTriggerCoroutine = StartCoroutine(DelayedAutoTrigger());
        }
    }

    protected virtual bool CanStartInteraction(bool requireKeyPressNeeded)
    {
        if (hasBeenUsed || isInteracting) return false;

        if (requireKeyPressNeeded)
            return requireKeyPress;

        return playerInRange && autoTrigger && !requireKeyPress;
    }

    protected IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(autoTriggerDelay);
        autoTriggerCoroutine = null;

        if (CanStartInteraction(requireKeyPressNeeded: false))
        {
            StartInteraction();
        }
    }

    protected void StartInteraction()
    {
        if (isInteracting) return;

        isInteracting = true;
        if (oneTimeOnly) hasBeenUsed = true;

        onInteractionStart?.Invoke();

        ExecuteInteractionLogic();

        UpdateHintVisibility();
    }

    // Конкретная логика выполняется в наследниках
    protected abstract void ExecuteInteractionLogic();

    public virtual void EndInteraction()
    {
        onInteractionEnd?.Invoke();
        isInteracting = false;
        StopAutoTriggerCoroutine();
        UpdateHintVisibility();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        UpdateHintVisibility();
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        StopAutoTriggerCoroutine();
        UpdateHintVisibility();
    }

    protected void StopAutoTriggerCoroutine()
    {
        if (autoTriggerCoroutine != null)
        {
            StopCoroutine(autoTriggerCoroutine);
            autoTriggerCoroutine = null;
        }
    }

    protected void UpdateHintVisibility()
    {
        if (interactionHint == null) return;
        bool shouldShowWhenClose = !hasBeenUsed && playerInRange && !isInteracting;
        interactionHint.SetActive(showHintOnlyWhenClose ? shouldShowWhenClose : !hasBeenUsed);
    }

    protected virtual void OnDisable()
    {
        StopAutoTriggerCoroutine();
        isInteracting = false;
        UpdateHintVisibility();
    }
}
