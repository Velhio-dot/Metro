using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Здоровье
    public float currentHealth = 100f;

    // Позиция
    public Vector3 playerPosition = Vector3.zero;
    public string currentScene = "";

    // Чекпоинты
    public Vector2 lastCheckpointPosition = Vector2.zero;
    public string lastCheckpointScene = "";

    // Фонарик
    public bool hasFlashlight = false;
    public bool flashlightEnabled = false;
    public float flashlightBattery = 100f;

    // Инвентарь
    public SavedInventorySlot[] inventorySlots = new SavedInventorySlot[8];

    // Прогресс
    public List<string> completedDialoguePoints = new List<string>();
    public List<string> permanentlyCollectedItemIds = new List<string>();

    // Направление игрока
    public Vector2 playerLastDirection = Vector2.down;
    public bool playerIsSprinting = false;

    [System.Serializable]
    public class SavedInventorySlot
    {
        public string itemId = "";
        public int quantity = 0;
        public bool isEmpty = true;
    }

    public GameData()
    {
        for (int i = 0; i < 8; i++)
            inventorySlots[i] = new SavedInventorySlot();
    }

    // Сброс к начальным значениям
    public void ResetToDefault()
    {
        currentHealth = 100f;
        playerPosition = Vector3.zero;
        currentScene = "";
        lastCheckpointPosition = Vector2.zero;
        lastCheckpointScene = "";
        hasFlashlight = false;
        flashlightEnabled = false;
        flashlightBattery = 100f;
        playerLastDirection = Vector2.down;
        playerIsSprinting = false;

        completedDialoguePoints.Clear();
        permanentlyCollectedItemIds.Clear();

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].itemId = "";
            inventorySlots[i].quantity = 0;
            inventorySlots[i].isEmpty = true;
        }
    }
}