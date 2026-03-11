using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class FixedDialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Диалоги")]
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData alternativeDialogue;

    [Header("Требования")]
    [SerializeField] private bool requireItem = false;
    [SerializeField] private ItemDataSO requiredItem;
    [SerializeField] private bool consumeItem = false;

    [Header("Спикер")]
    [SerializeField] private string speakerName = "Персонаж";
    [SerializeField] private Sprite speakerPortrait;

    [Header("Настройки запуска")]
    [SerializeField] private bool autoTrigger = false;
    [SerializeField] private bool requireKeyPress = true;
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Для автодиалогов")]
    [SerializeField] private float autoTriggerDelay = 0f;
    [SerializeField] private bool showSpeakerInAutoDialogue = false;

    [Header("Визуальные подсказки")]
    [SerializeField] private GameObject interactionHint;
    [SerializeField] private bool showHintOnlyWhenClose = true;

    [Header("События")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private bool isDialogueStarting = false; // ЗАЩИТА
    private Player1 player;
    private Coroutine autoTriggerCoroutine;
    private Coroutine waitForEndCoroutine;

    void Start()
    {
        player = Player1.Instance;
        UpdateHintVisibility();
    }

    void Update()
    {
        // ТОЛЬКО автозапуск, без проверки клавиши
        if (playerInRange && !hasBeenUsed && autoTrigger && !requireKeyPress)
        {
            if (autoTriggerCoroutine == null && autoTriggerDelay > 0)
            {
                autoTriggerCoroutine = StartCoroutine(DelayedAutoTrigger());
            }
            else if (autoTriggerDelay <= 0 && !isDialogueStarting)
            {
                StartDialogue();
            }
        }
    }

    IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(autoTriggerDelay);
        StartDialogue();
    }

    public void Interact()
    {
        // ЕДИНСТВЕННЫЙ способ ручного запуска
        if (playerInRange && !hasBeenUsed && requireKeyPress && !isDialogueStarting)
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        // ЗАЩИТА ОТ ПОВТОРНОГО ЗАПУСКА
        if (isDialogueStarting || DialogueManager.Instance == null) return;
        if (DialogueManager.Instance.IsDialogueActive) return;

        isDialogueStarting = true;

        if (oneTimeOnly) hasBeenUsed = true;

        bool hasRequiredItem = CheckRequiredItem();
        DialogueData dialogueToUse = GetDialogueToUse(hasRequiredItem);

        if (dialogueToUse == null)
        {
            Debug.LogWarning($"{name}: Нет диалога!");
            isDialogueStarting = false;
            return;
        }

        string actualSpeakerName = speakerName;
        if (autoTrigger && !showSpeakerInAutoDialogue)
            actualSpeakerName = "";

        onDialogueStart?.Invoke();

        DialogueManager.Instance.StartDialogue(dialogueToUse, actualSpeakerName, speakerPortrait);

        // Запускаем ожидание конца диалога
        if (waitForEndCoroutine != null)
            StopCoroutine(waitForEndCoroutine);
        waitForEndCoroutine = StartCoroutine(WaitForDialogueEnd(hasRequiredItem));

        if (autoTriggerCoroutine != null)
        {
            StopCoroutine(autoTriggerCoroutine);
            autoTriggerCoroutine = null;
        }

        UpdateHintVisibility();
    }

    IEnumerator WaitForDialogueEnd(bool hadRequiredItem)
    {
        // Сохраняем, какой диалог был выбран
        bool wasMainDialogue = hadRequiredItem && mainDialogue != null;
        // Ждем пока диалог активен
        while (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return null;
        }

        // Диалог закончился
        if (hadRequiredItem && requireItem && consumeItem && requiredItem != null)
            ConsumeRequiredItem();

        if (wasMainDialogue)
        {
            onDialogueEnd?.Invoke();
        }
        UpdateHintVisibility();
        isDialogueStarting = false; // СБРАСЫВАЕМ ЗАЩИТУ
    }

    bool CheckRequiredItem()
    {
        if (!requireItem || requiredItem == null) return true;
        if (InventoryManager.Instance != null)
            return InventoryManager.Instance.PlayerInventory.HasItem(requiredItem.itemId);
        return false;
    }

    void ConsumeRequiredItem()
    {
        if (InventoryManager.Instance == null) return;

        var inventory = InventoryManager.Instance.PlayerInventory;
        var slots = inventory.Slots;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemData.itemId == requiredItem.itemId)
            {
                inventory.RemoveItem(i, 1);
                Debug.Log($"Использован предмет: {requiredItem.itemName}");
                break;
            }
        }
    }

    DialogueData GetDialogueToUse(bool hasRequiredItem)
    {
        if (hasRequiredItem && mainDialogue != null)
            return mainDialogue;
        if (alternativeDialogue != null)
            return alternativeDialogue;
        return mainDialogue;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateHintVisibility();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (autoTriggerCoroutine != null)
            {
                StopCoroutine(autoTriggerCoroutine);
                autoTriggerCoroutine = null;
            }

            UpdateHintVisibility();
        }
    }

    void UpdateHintVisibility()
    {
        if (interactionHint == null) return;

        bool shouldShow = !hasBeenUsed && playerInRange;

        if (showHintOnlyWhenClose)
            interactionHint.SetActive(shouldShow);
        else
            interactionHint.SetActive(!hasBeenUsed);
    }
}