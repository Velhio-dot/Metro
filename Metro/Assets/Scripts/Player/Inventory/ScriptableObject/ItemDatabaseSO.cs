using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabaseSO : ScriptableObject
{
    [SerializeField] private ItemDataSO[] itemsArray;
    private Dictionary<string, ItemDataSO> itemsDictionary;

    public void Initialize()
    {
        itemsDictionary = new Dictionary<string, ItemDataSO>();
        foreach (var item in itemsArray)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemId))
                itemsDictionary[item.itemId] = item;
        }
    }

    public ItemDataSO GetItemById(string itemId)
    {
        if (itemsDictionary == null || itemsDictionary.Count == 0)
            Initialize();

        itemsDictionary.TryGetValue(itemId, out var item);
        return item;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Collect Items")]
    void CollectAllItemsInEditor()
    {
        var items = Resources.LoadAll<ItemDataSO>("Items");
        itemsArray = items;
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Найдено {items.Length} предметов");
    }
#endif
}