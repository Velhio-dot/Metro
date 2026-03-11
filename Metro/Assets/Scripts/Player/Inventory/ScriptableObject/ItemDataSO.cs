using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [Header("Основная информация")]
    public string itemId;
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Тип предмета")]
    public ItemType type;

    [Header("Настройки")]
    public bool isStackable = false;
    public int maxStack = 1;
    public bool isUsable = true;

    [Header("Эффекты")]
    public float healthRestore = 0f;
    public float batteryCharge = 0f;

    public enum ItemType
    {
        Key,
        HealthPotion,
        Battery,
        Note,
        Flashlight,
        Other
    }
}