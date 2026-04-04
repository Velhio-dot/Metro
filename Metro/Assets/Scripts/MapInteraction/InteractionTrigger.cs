using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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

    private bool hasBeenUsed;
    private bool playerInRange;
    private bool isInteracting;
    private Coroutine autoTriggerCoroutine;

    private void Start()
    {
        UpdateHintVisibility();
    }

    private void Update()
    {
        HandleAutoTrigger();
    }

    public void Interact()
    {
        if (!CanStartInteraction(requireKeyPressNeeded: true))
        {
            return;
        }

        StartInteraction();
    }

    private void HandleAutoTrigger()
    {
        if (!CanStartInteraction(requireKeyPressNeeded: false))
        {
            return;
        }

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

    private bool CanStartInteraction(bool requireKeyPressNeeded)
    {
        if (!playerInRange || hasBeenUsed || isInteracting)
        {
            return false;
        }

        if (requireKeyPressNeeded)
        {
            return requireKeyPress;
        }

        return autoTrigger && !requireKeyPress;
    }

    private IEnumerator DelayedAutoTrigger()
    {
        yield return new WaitForSeconds(autoTriggerDelay);
        autoTriggerCoroutine = null;

        if (CanStartInteraction(requireKeyPressNeeded: false))
        {
            StartInteraction();
        }
    }

    private void StartInteraction()
    {
        if (isInteracting)
        {
            return;
        }

        isInteracting = true;
        if (oneTimeOnly)
        {
            hasBeenUsed = true;
        }

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

    private void StartDialogue()
    {
        if (BannerManager.Instance != null && BannerManager.Instance.IsShowing)
        {
            EndInteraction();
            return;
        }

        if (DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive)
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

        string actualSpeakerName = autoTrigger && !showSpeakerInAutoDialogue ? "" : speakerName;

        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
        DialogueManager.Instance.StartDialogue(dialogueToUse, actualSpeakerName, speakerPortrait);

        if (hasRequiredItem && requireItem && consumeItem && requiredItem != null)
        {
            ConsumeRequiredItem();
        }
    }

    private void StartBanner()
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

        BannerManager.Instance.OnBannerHidden += OnBannerEnded;
        BannerManager.Instance.ShowBanner(bannerText, bannerDuration);
    }

    private void OnDialogueEnded()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }

        EndInteraction();
    }

    private void OnBannerEnded()
    {
        if (BannerManager.Instance != null)
        {
            BannerManager.Instance.OnBannerHidden -= OnBannerEnded;
        }

        EndInteraction();
    }

    private void EndInteraction()
    {
        onInteractionEnd?.Invoke();
        isInteracting = false;
        StopAutoTriggerCoroutine();
        UpdateHintVisibility();
    }

    private bool CheckRequiredItem()
    {
        if (!requireItem || requiredItem == null)
        {
            return true;
        }

        if (InventoryManager.Instance == null)
        {
            return false;
        }

        return InventoryManager.Instance.PlayerInventory.HasItem(requiredItem.itemId);
    }

    private void ConsumeRequiredItem()
    {
        if (InventoryManager.Instance == null)
        {
            return;
        }

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

    private DialogueData GetDialogueToUse(bool hasRequiredItem)
    {
        if (hasRequiredItem && mainDialogue != null)
        {
            return mainDialogue;
        }

        if (alternativeDialogue != null)
        {
            return alternativeDialogue;
        }

        return mainDialogue;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        UpdateHintVisibility();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        StopAutoTriggerCoroutine();
        UpdateHintVisibility();
    }

    private void StopAutoTriggerCoroutine()
    {
        if (autoTriggerCoroutine != null)
        {
            StopCoroutine(autoTriggerCoroutine);
            autoTriggerCoroutine = null;
        }
    }

    private void UpdateHintVisibility()
    {
        if (interactionHint == null)
        {
            return;
        }

        bool shouldShowWhenClose = !hasBeenUsed && playerInRange && !isInteracting;
        interactionHint.SetActive(showHintOnlyWhenClose ? shouldShowWhenClose : !hasBeenUsed);
    }

    private void OnDisable()
    {
        StopAutoTriggerCoroutine();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }

        if (BannerManager.Instance != null)
        {
            BannerManager.Instance.OnBannerHidden -= OnBannerEnded;
        }

        isInteracting = false;
        UpdateHintVisibility();
    }

    public enum InteractionType
    {
        Dialogue,
        Banner
    }
}
