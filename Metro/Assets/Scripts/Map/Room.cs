using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class OptimizedRoom : MonoBehaviour
{
    [System.Serializable]
    public class TilemapGroup
    {
        public Tilemap tilemap;
        public TilemapCollider2D tilemapCollider;
        public CompositeCollider2D compositeCollider;
        public Color originalColor;
        public int originalLayer;
        public bool hasCollider;
    }

    [Header("Tilemap References")]
    [SerializeField] private TilemapGroup[] tilemapGroups;

    [Header("Room Content")]
    [SerializeField] private GameObject[] roomObjects;
    [SerializeField] private Light2D[] roomLights;

    [Header("Room Settings")]
    [SerializeField] private bool isStartingRoom = false;
    [SerializeField] private Color gizmoColor = Color.green;

    private bool isVisible = true;
    private bool isInitialized = false;

    void Start()
    {
        InitializeRoom();
    }

    void InitializeRoom()
    {
        if (isInitialized) return;

        Debug.Log($"Initializing room: {name} (Starting: {isStartingRoom})");

        // 1. Запоминаем оригинальные настройки
        foreach (var group in tilemapGroups)
        {
            if (group.tilemap != null)
            {
                group.originalColor = group.tilemap.color;
                group.originalLayer = group.tilemap.gameObject.layer;

                // Автоматически находим коллайдеры если не назначены
                if (group.tilemapCollider == null)
                    group.tilemapCollider = group.tilemap.GetComponent<TilemapCollider2D>();

                if (group.compositeCollider == null)
                    group.compositeCollider = group.tilemap.GetComponent<CompositeCollider2D>();

                group.hasCollider = group.tilemapCollider != null || group.compositeCollider != null;

                Debug.Log($"Tilemap: {group.tilemap.name}, HasCollider: {group.hasCollider}");
            }
        }

        // 2. Скрываем все комнаты КРОМЕ стартовой
        if (!isStartingRoom)
        {
            HideRoomImmediate(); // ← НЕМЕДЛЕННОЕ скрытие
        }
        else
        {
            // Стартовая комната должна быть видима
            ShowRoomImmediate();
        }

        isInitialized = true;
    }

    void ShowRoomImmediate()
    {
        Debug.Log($"Immediate show: {name}");

        foreach (var group in tilemapGroups)
        {
            if (group.tilemap != null)
            {
                group.tilemap.color = group.originalColor;
                group.tilemap.gameObject.layer = group.originalLayer;

                if (group.tilemapCollider != null)
                    group.tilemapCollider.enabled = true;

                if (group.compositeCollider != null)
                    group.compositeCollider.enabled = true;
            }
        }

        foreach (var light in roomLights)
            if (light != null) light.enabled = true;

        foreach (var obj in roomObjects)
            if (obj != null) obj.SetActive(true);

        isVisible = true;
    }

    void HideRoomImmediate()
    {
        Debug.Log($"Immediate hide: {name}");

        foreach (var group in tilemapGroups)
        {
            if (group.tilemap != null)
            {
                group.tilemap.color = Color.clear;
                group.tilemap.gameObject.layer = LayerMask.NameToLayer("Hidden");

                if (group.tilemapCollider != null)
                    group.tilemapCollider.enabled = false;

                if (group.compositeCollider != null)
                    group.compositeCollider.enabled = false;
            }
        }

        foreach (var light in roomLights)
            if (light != null) light.enabled = false;

        foreach (var obj in roomObjects)
            if (obj != null) obj.SetActive(false);

        isVisible = false;
    }

    public void ShowRoom()
    {
        if (!isInitialized) InitializeRoom();
        if (isVisible) return;

        Debug.Log($"Showing room: {name}");
        ShowRoomImmediate();
    }

    public void HideRoom()
    {
        if (!isInitialized) InitializeRoom();
        if (!isVisible) return;

        Debug.Log($"Hiding room: {name}");
        HideRoomImmediate();
    }

    [ContextMenu("Force Show Room")]
    void ForceShowRoom()
    {
        ShowRoomImmediate();
    }

    [ContextMenu("Force Hide Room")]
    void ForceHideRoom()
    {
        HideRoomImmediate();
    }

    [ContextMenu("Auto Setup Tilemaps")]
    void AutoSetupTilemaps()
    {
        List<TilemapGroup> groups = new List<TilemapGroup>();

        Tilemap[] tilemaps = GetComponentsInChildren<Tilemap>();

        foreach (Tilemap tilemap in tilemaps)
        {
            TilemapGroup group = new TilemapGroup
            {
                tilemap = tilemap,
                tilemapCollider = tilemap.GetComponent<TilemapCollider2D>(),
                compositeCollider = tilemap.GetComponent<CompositeCollider2D>(),
                originalColor = tilemap.color,
                originalLayer = tilemap.gameObject.layer
            };

            groups.Add(group);
            Debug.Log($"Found: {tilemap.name}");
        }

        tilemapGroups = groups.ToArray();
        Debug.Log($"Auto-setup: {groups.Count} tilemaps");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 5);
    }
}