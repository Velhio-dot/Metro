using UnityEngine;

public class DialogueInteractable : BaseInteractable
{
    [Header("Настройки Диалога")]
    [SerializeField] private DialogueData mainDialogue;
    [SerializeField] private DialogueData alternativeDialogue;
    [SerializeField] private bool requireItem = false;
    [SerializeField] private ItemDataSO requiredItem;
    [SerializeField] private bool consumeItem = false;
    [SerializeField] private string speakerName = "Персонаж";
    [SerializeField] private Sprite speakerPortrait;
    [SerializeField] private bool showSpeakerInAutoDialogue = false;
    [SerializeField] private bool useTimelineAfter = true;
    [SerializeField] private UnityEngine.Playables.PlayableDirector mainTimeline;
    [SerializeField] private UnityEngine.Playables.PlayableDirector altTimeline;

    private bool lastCheckResult = false;

    protected override void ExecuteInteractionLogic()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.IsDialogueActive)
        {
            EndInteraction();
            return;
        }

        lastCheckResult = CheckRequiredItem();
        DialogueData dialogueToUse = GetDialogueToUse(lastCheckResult);
        if (dialogueToUse == null)
        {
            Debug.LogWarning($"{name}: Нет диалоговых данных!");
            EndInteraction();
            return;
        }

        string actualSpeakerName = autoTrigger && !showSpeakerInAutoDialogue ? "" : speakerName;

        DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;

        // NEW: Сразу предупреждаем менеджер, если впереди возможна катсцена
        UnityEngine.Playables.PlayableDirector timelineToPlay = lastCheckResult ? mainTimeline : altTimeline;
        if (useTimelineAfter && timelineToPlay != null)
        {
            DialogueManager.Instance.ShouldDeferPlayerControl = true;
        }

        DialogueManager.Instance.StartDialogue(dialogueToUse, actualSpeakerName, speakerPortrait);

        if (lastCheckResult && requireItem && consumeItem && requiredItem != null)
        {
            ConsumeRequiredItem();
        }
    }

    private void OnDialogueEnded()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;

            // Если есть таймлайн для этой ветки, запускаем его
            UnityEngine.Playables.PlayableDirector timelineToPlay = lastCheckResult ? mainTimeline : altTimeline;

            if (useTimelineAfter && timelineToPlay != null)
            {
                Debug.Log($"[DialogueInteractable] Попытка запуска катсцены: {timelineToPlay.name}");
                DialogueManager.Instance.ShouldDeferPlayerControl = true;

                // Пытаемся найти наш скрипт-директор на этом объекте
                CutsceneDirector customDirector = timelineToPlay.GetComponent<CutsceneDirector>();
                if (customDirector != null)
                {
                    customDirector.Play();
                }
                else
                {
                    timelineToPlay.Play();
                }
            }
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

    protected override void OnDisable()
    {
        base.OnDisable();

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
        }
    }
}
