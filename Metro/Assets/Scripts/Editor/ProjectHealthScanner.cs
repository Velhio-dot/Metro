using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Редакторский скрипт для поиска «битых» объектов, которые вызывают ошибки Render Graph в Unity 6.
/// </summary>
public class ProjectHealthScanner : EditorWindow
{
    [MenuItem("Tools/Project Health Check")]
    public static void ShowWindow()
    {
        GetWindow<ProjectHealthScanner>("Project Health");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Сканировать Сцену на Ошибки", GUILayout.Height(40)))
        {
            ScanScene();
        }
    }

    private void ScanScene()
    {
        Debug.Log("<color=cyan><b>[HealthScanner] Начало сканирования...</b></color>");
        int issuesFound = 0;

        // 1. Поиск пустых спрайтов (белые квадраты)
        SpriteRenderer[] renderers = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var sr in renderers)
        {
            if (sr.sprite == null)
            {
                Debug.LogWarning($"[Mising Sprite] Объект: <b>{sr.name}</b> не имеет назначенного спрайта. Это может ломать Render Graph!", sr.gameObject);
                issuesFound++;
            }
        }

        // 2. Поиск ошибок в Light2D
        Light2D[] lights = Object.FindObjectsByType<Light2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            // В Unity 6 пустой тип света может вызвать NRE
            if (light.lightType == Light2D.LightType.Freeform && (light.shapePath == null || light.shapePath.Length < 3))
            {
                Debug.LogError($"[Broken Light2D] Объект: <b>{light.name}</b> имеет некорректную форму (Freeform).", light.gameObject);
                issuesFound++;
            }
        }

        // 3. Поиск некорректных ShadowCaster2D
        ShadowCaster2D[] casters = Object.FindObjectsByType<ShadowCaster2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var caster in casters)
        {
            // Если кастер на тайлмепе, проверим колайдер
            var tilemap = caster.GetComponent<UnityEngine.Tilemaps.Tilemap>();
            if (tilemap != null && caster.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() == null)
            {
                Debug.LogError($"[Shadow Error] <b>{caster.name}</b>: ShadowCaster2D на тайлмепе требует TilemapCollider2D!", caster.gameObject);
                issuesFound++;
            }
        }

        if (issuesFound == 0)
        {
            Debug.Log("<color=green><b>[HealthScanner] Проблем не обнаружено! Всё чисто.</b></color>");
        }
        else
        {
            Debug.Log($"<color=orange><b>[HealthScanner] Сканирование завершено. Найдено проблем: {issuesFound}. Кликните на предупреждения в консоли, чтобы найти объекты.</b></color>");
        }
    }
}
