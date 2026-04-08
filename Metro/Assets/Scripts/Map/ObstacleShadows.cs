using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

# if UNITY_EDITOR
using UnityEditor;
# endif

/// <summary>
/// Скрипт-помощник для автоматического добавления теней на стены и препятствия.
/// Избавляет от необходимости вручную вешать ShadowCaster2D на сотни объектов.
/// </summary>
public class ObstacleShadows : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Если включено, скрипт обходит всех детей этого объекта.")]
    [SerializeField] private bool includeChildren = true;
    
    [Tooltip("Слой, на котором должны быть объекты для добавления теней.")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Параметры теней")]
    [SerializeField] private bool selfShadows = true;

    /// <summary>
    /// Запустить генерацию теней из инспектора.
    /// Нажмите правой кнопкой на компонент в инспекторе -> Generate Shadows.
    /// </summary>
    [ContextMenu("Generate Shadows")]
    public void GenerateShadows()
    {
        int addedCount = 0;
        List<Collider2D> colliders = new List<Collider2D>();

        if (includeChildren)
        {
            colliders.AddRange(GetComponentsInChildren<Collider2D>(true));
        }
        else
        {
            var col = GetComponent<Collider2D>();
            if (col != null) colliders.Add(col);
        }

        foreach (var col in colliders)
        {
            // Проверяем слой (если маска не пустая)
            if (obstacleLayer != 0 && ((1 << col.gameObject.layer) & obstacleLayer) == 0)
                continue;

            // Добавляем ShadowCaster2D, если его еще нет
            if (col.GetComponent<ShadowCaster2D>() == null)
            {
                var caster = col.gameObject.AddComponent<ShadowCaster2D>();
                caster.selfShadows = selfShadows;
                addedCount++;
            }
        }

        Debug.Log($"[ObstacleShadows] Генерация завершена! Добавлено теней: {addedCount}");
    }

    [ContextMenu("Clear All Shadows")]
    public void ClearShadows()
    {
        var casters = GetComponentsInChildren<ShadowCaster2D>(true);
        int count = casters.Length;
        
        foreach (var caster in casters)
        {
            DestroyImmediate(caster);
        }
        
        Debug.Log($"[ObstacleShadows] Все компоненты ShadowCaster2D удалены ({count} шт.).");
    }
}
