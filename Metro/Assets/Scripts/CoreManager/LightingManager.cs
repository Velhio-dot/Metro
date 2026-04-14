using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Управляет глобальными настройками освещения и переключением технологий (Standard vs Plugin).
/// Автоматически меняет материалы у всех рендереров в сцене.
/// </summary>
public class LightingManager : MonoBehaviour
{
    public enum LightingTech
    {
        Standard2D,
        ShaderPlugin
    }

    [Header("Текущие настройки")]
    [SerializeField] private LightingTech currentTech = LightingTech.Standard2D;

    [Header("Материалы")]
    public Material standardMaterial;
    public Material pluginMaterial;

    [Header("Настройки Shader Plugin")]
    [Range(0, 1)] public float ambientIntensity = 0.2f;

    [Header("Исключения")]
    [Tooltip("Канвас, объекты внутри которого не должны менять материал.")]
    public Canvas uiCanvas;

    [Header("Ссылки на компоненты (для плагина)")]
    [Tooltip("Здесь будут ссылки на объекты плагина (Light Volumes и т.д.).")]
    public GameObject pluginVolumeContainer;

    [Tooltip("Стандартные источники света Unity, которые нужно выключать при работе плагина.")]
    public List<Light2D> standardLights = new List<Light2D>();

    public void SetLightingTech(LightingTech tech)
    {
        currentTech = tech;
        ApplySettings();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySettings();
    }

    [ContextMenu("Apply Current Settings")]
    public void ApplySettings()
    {
        bool isPlugin = currentTech == LightingTech.ShaderPlugin;
        Material targetMat = isPlugin ? pluginMaterial : standardMaterial;

        // 1. Включаем/выключаем контейнеры света
        if (pluginVolumeContainer != null) pluginVolumeContainer.SetActive(isPlugin);
        
        // Включаем/выключаем стандартные источники света
        foreach (var light in standardLights)
        {
            if (light != null) light.enabled = !isPlugin;
        }

        // 2. Меняем материалы по всей сцене
        if (targetMat != null)
        {
            SwapAllMaterials(targetMat);
        }

        Debug.Log($"[LightingManager] Технология освещения: {currentTech}. Материалы обновлены.");
    }

    private void SwapAllMaterials(Material targetMat)
    {
        // Ищем вообще все рендереры
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var r in renderers)
        {
            if (r is SpriteRenderer || r is UnityEngine.Tilemaps.TilemapRenderer)
            {
                // Проверка на исключение (UI Canvas)
                if (uiCanvas != null && r.transform.IsChildOf(uiCanvas.transform))
                {
                    continue;
                }

                // Меняем материал
                r.sharedMaterial = targetMat;
            }
        }
    }
}
