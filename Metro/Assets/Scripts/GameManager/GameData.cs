using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // Здоровье и позиция
    public float currentHealth = 100f;
    public Vector3 playerPosition = Vector3.zero;
    public string currentScene = "";

    // Чекпоинты
    public Vector2 lastCheckpointPosition = Vector2.zero;
    public string lastCheckpointScene = "";

    // Фонарик
    public bool hasFlashlight = false;
    public bool flashlightEnabled = false;
    public float flashlightBattery = 100f;

    // Инвентарь (ГЛАВНОЕ - здесь хранится инвентарь!)
    public SavedInventorySlot[] inventorySlots = new SavedInventorySlot[8];

    // Прогресс
    public List<string> completedDialoguePoints = new List<string>();
    public List<string> permanentlyCollectedItemIds = new List<string>();

    // Дополнительно
    public Vector2 playerLastDirection = Vector2.down;
    public bool playerIsSprinting = false;
    public float playTimeSeconds = 0f;

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
        {
            inventorySlots[i] = new SavedInventorySlot();
        }
    }
}