using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerPortrait;
    [SerializeField] private GameObject continuePrompt;

    [Header("Настройки")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private KeyCode continueKey = KeyCode.E;

    // События
    public event Action OnDialogueEnded;

    private DialogueData currentDialogue;
    private string currentSpeaker;
    private Sprite currentPortrait;
    private int currentLine = 0;
    private bool isTyping = false;
    private bool isActive = false;
    private Coroutine typingCoroutine;

    public bool IsDialogueActive => isActive;

    /// <summary>
    /// Если true, после завершения диалога управление игроку НЕ вернется автоматически.
    /// Полезно для перехода в катсцены.
    /// </summary>
    public bool ShouldDeferPlayerControl { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);              
        }
        else
        {
            Destroy(gameObject);
        }

        HideDialogue();
    }

    void Update()
    {
        // Теперь и E, и Пробел будут листать диалог
        if (isActive && (Input.GetKeyDown(continueKey) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (isTyping)
            {
                StopTyping();
                DisplayFullLine();
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueData dialogue, string speakerName = "", Sprite portrait = null)
    {
        if (dialogue == null || dialogue.lines.Length == 0) return;
        if (isActive)
        {
            Debug.LogError("Попытка запустить диалог, когда другой уже активен!");
            return;
        }
        currentDialogue = dialogue;
        currentSpeaker = string.IsNullOrEmpty(speakerName) ? "Персонаж" : speakerName;
        currentPortrait = portrait;
        currentLine = 0;
        isActive = true;

        // Останавливаем игрока
        if (Player1.Instance != null)
            Player1.Instance.enabled = false;

        ShowDialogue();
        DisplayLine();
    }

    void DisplayLine()
    {
        if (currentDialogue == null || currentLine >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        var line = currentDialogue.lines[currentLine];

        // Устанавливаем данные
        speakerNameText.text = !string.IsNullOrEmpty(line.speakerName)
            ? line.speakerName
            : currentSpeaker;

        speakerPortrait.sprite = line.speakerPortrait != null
            ? line.speakerPortrait
            : currentPortrait;

        // Запускаем печать текста
        typingCoroutine = StartCoroutine(TypeText(line.text));

        // Обработка событий строки
        if (line.eventAfterLine != null)
            line.eventAfterLine.Trigger();

        if (line.giveItemAfterLine && line.itemToGive != null && InventoryManager.Instance != null)
            InventoryManager.Instance.PlayerInventory.AddItem(line.itemToGive);
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        continuePrompt.SetActive(false);

        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        continuePrompt.SetActive(true);
    }

    void StopTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        isTyping = false;
    }

    void DisplayFullLine()
    {
        if (currentDialogue == null || currentLine >= currentDialogue.lines.Length) return;

        var line = currentDialogue.lines[currentLine];
        dialogueText.text = line.text;
        continuePrompt.SetActive(true);
    }

    void NextLine()
    {
        currentLine++;
        DisplayLine();
    }

    void EndDialogue()
    {
        HideDialogue();
        isActive = false;

        StartCoroutine(EndDialogueCoroutine());
    }

    IEnumerator EndDialogueCoroutine()
    {
        // Пропускаем один кадр. Это гарантирует, что кнопка E (на которую
        // мы нажали, чтобы закрыть диалог) не считается скриптом игрока
        // как нажатие для ОТКРЫТИЯ диалога в этом же самом кадре!
        yield return null;

        // Возвращаем управление, если не было указано иное
        if (!ShouldDeferPlayerControl && Player1.Instance != null)
            Player1.Instance.enabled = true;

        // Сбрасываем флаг задержки для следующего диалога
        ShouldDeferPlayerControl = false;

        // Сбрасываем
        currentDialogue = null;
        currentSpeaker = "";
        currentPortrait = null;

        // Вызываем событие окончания
        OnDialogueEnded?.Invoke();
    }

    void ShowDialogue()
    {
        dialoguePanel.SetActive(true);
    }

    void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    public void ForceEndDialogue()
    {
        EndDialogue();
    }

#if UNITY_EDITOR
    [ContextMenu("Тест: Создать тестовый диалог")]
    void CreateTestDialogue()
    {
        // Для отладки в редакторе
        Debug.Log("Диалог активен: " + isActive);
    }
#endif
}