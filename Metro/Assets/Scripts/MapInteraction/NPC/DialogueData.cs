using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueLine
    {
        [Header("Кто говорит")]
        public string speakerName;

        [Header("Текст")]
        [TextArea(3, 5)] public string text;

        [Header("Портрет")]
        public Sprite speakerPortrait;

        [Header("Действие после строки")]
        public GameEvent eventAfterLine;
        public bool giveItemAfterLine = false;
        public ItemDataSO itemToGive;
    }

    [Header("Строки диалога")]
    public DialogueLine[] lines;

    [Header("Настройки")]
    public bool canBeSkipped = true;
    public bool blockPlayerMovement = true;

    // Простой метод для проверки
    public bool IsValid()
    {
        return lines != null && lines.Length > 0;
    }
}