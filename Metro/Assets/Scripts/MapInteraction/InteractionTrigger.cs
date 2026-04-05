using UnityEngine;
using System;

[Obsolete("Используйте BannerInteractable или DialogueInteractable вместо этого скрипта на новых объектах.")]
public class InteractionTrigger : BaseInteractable
{
    public InteractionType interactionType;

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

    protected override void ExecuteInteractionLogic()
    {
        switch (interactionType)
        {
            case InteractionType.Dialogue:
                StartDialogue();
                break;
            case InteractionType.Banner:
                StartBanner();
                break;
        }
    }

    private void StartDialogue()
    {
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
        Debug.Log($"[InteractionTrigger] StartBanner() вызван для {gameObject.name}");
        if (BannerManager.Instance == null)
        {
            Debug.LogError($"[InteractionTrigger] ОШИБКА: BannerManager.Instance РАВЕН NULL! Убедитесь, что скрипт BannerManager или канвас с ним активен на сцене!");
            EndInteraction();
            return;
        }

        Debug.Log($"[InteractionTrigger] Передача текста баннеру: {bannerText}");
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

    private bool CheckRequiredItem()
    {
        if (!requireItem || requiredItem == null) return true;
        if (InventoryManager.Instance == null) return false;

        return InventoryManager.Instance.PlayerInventory.HasItem(requiredItem.itemId);
    }

    private void ConsumeRequiredItem()
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

    private DialogueData GetDialogueToUse(bool hasRequiredItem)
    {
        if (hasRequiredItem && mainDialogue != null) return mainDialogue;
        if (alternativeDialogue != null) return alternativeDialogue;
        return mainDialogue;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (DialogueManager.Instance != null) DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        if (BannerManager.Instance != null) BannerManager.Instance.OnBannerHidden -= OnBannerEnded;
    }

    public enum InteractionType
    {
        Dialogue,
        Banner
    }
}
