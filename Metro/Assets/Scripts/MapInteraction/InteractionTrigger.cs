using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class InteractionTrigger : MonoBehaviour, IInteractable
{
    [Header("Тип взаимодействия")]
    [SerializeField] private InteractionType interactionType = InteractionType.Dialogue;

    [Header("Общие настройки")]
    [SerializeField] private bool requireKeyPress = true;
    [SerializeField] private bool oneTimeOnly = false;
    [SerializeField] private bool autoTrigger = false;
    [SerializeField] private float autoTriggerDelay = 0f;

    [Header("Диалог (если тип Dialogue)")]
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData alternativeDialogue;
    [SerializeField] private bool requireItem = false;
    [SerializeField] private ItemDataSO requiredItem;
    [SerializeField] private bool consumeItem = false;
    [SerializeField] private string speakerName = "Персонаж";
    [SerializeField] private Sprite speakerPortrait;
    [SerializeField] private bool showSpeakerInAutoDialogue = false;

    [Header("Баннер (если тип Banner)")]
    [SerializeField] private string bannerText = "Информация";
    [SerializeField] private float bannerDuration = 3f;

    [Header("Визуальные подсказки")]
    [SerializeField] private GameObject interactionHint;
    [SerializeField] private bool showHintOnlyWhenClose = true;

    [Header("События")]
    public UnityEvent onInteractionStart;
    public UnityEvent onInteractionEnd;

    private bool hasBeenUsed = false;
    private bool playerInRange = false;
    private bool isInteracting = false;
    private Player1 player;
    private Coroutine autoTriggerCoroutine;

    void Start()
    {
        player = Player1.Instance;
        UpdateHintVisibility();
    }

    void Update()
    {
        if (playerInRange && !hasBeenUsed && autoTrigger && !requireKeyPress && !isInteracting)
        {
            if (autoTriggerCoroutine == null && autoTriggerDelay > 0)
            {
                autoTriggerCoroutine = StartCoroutine(DelayedAutoTrigger());
            }
            else if (autoTriggerDelay <= 0)
            {
                StartInteraction();
            }
        }
    }

    IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(autoTriggerDelay);
        StartInteraction();
    }

    public void Interact()
    {
        if (playerInRange && !hasBeenUsed && requireKeyPress && !isInteracting)
        {
            StartInteraction();
        }
    }

    void StartInteraction()
    {
        if (isInteracting) return;

        isInteracting = true;
        if (oneTimeOnly) hasBeenUsed = true;

        onInteractionStart?.Invoke();

        switch (interactionType)
        {
            case InteractionType.Dialogue:
                StartDialogue();
                break;
            case InteractionType.Banner:
                StartBanner();
                break;
        }

        UpdateHintVisibility();
    }

    void StartDialogue()
    {
        if (BannerManager.Instance != null && BannerManager.Instance.IsShowing)
        {
            EndInteraction();
            return;
        }

        if (DialogueManager.Instance == null)
        {
            EndInteraction();
            return;
        }

        if (DialogueManager.Instance.IsDialogueActive)
        {
            EndInteraction();
            return;
        }

        bool hasRequiredItem = CheckRequiredItem();
        DialogueData dialogueToUse = GetDialogueToUse(hasRequiredItem);

        if (dialogueToUse == null)
        {
            Debug.LogWarning($"{name}: Нет диалога!");
            EndInteraction();
            return;
        }

        string actualSpeakerName = speakerName;
        if (autoTrigger && !showSpeakerInAutoDialogue)
            actualSpeakerName = "";

        // Подписываемся на окончание диалога
        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        DialogueManager.Instance.StartDialogue(dialogueToUse, actualSpeakerName, speakerPortrait);

        if (hasRequiredItem && requireItem && consumeItem && requiredItem != null)
        {
            ConsumeRequiredItem();
        }
    }

    void StartBanner()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            EndInteraction();
            return;
        }

        if (BannerManager.Instance == null)
        {
            EndInteraction();
            return;
        }

        // Подписываемся на окончание баннера
        BannerManager.Instance.OnBannerHidden += OnBannerEnded;
        BannerManager.Instance.ShowBanner(bannerText, bannerDuration);
    }

    void OnDialogueEnded()
    {
        // Отписываемся
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;

        EndInteraction();
    }

    void OnBannerEnded()
    {
        // Отписываемся
        if (BannerManager.Instance != null)
            BannerManager.Instance.OnBannerHidden -= OnBannerEnded;

        EndInteraction();
    }

    void EndInteraction()
    {
        onInteractionEnd?.Invoke();
        isInteracting = false;

        if (autoTriggerCoroutine != null)
        {
            StopCoroutine(autoTriggerCoroutine);
            autoTriggerCoroutine = null;
        }

        UpdateHintVisibility();
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

        bool shouldShow = !hasBeenUsed && playerInRange && !isInteracting;

        if (showHintOnlyWhenClose)
            interactionHint.SetActive(shouldShow);
        else
            interactionHint.SetActive(!hasBeenUsed);
    }

    public enum InteractionType
    {
        Dialogue,
        Banner
    }
}