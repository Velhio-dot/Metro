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
    [Tooltip("Материал для стен (с включенным Is Wall).")]
    public Material pluginWallMaterial;

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

    [Header("Настройки слоев")]
    [Tooltip("Имя слоя, на котором находятся препятствия (стены). На ноуте это был Слой 6.")]
    public string wallLayerName = "occlusion";
    private int wallLayerIndex = 6; 

    public void SetLightingTech(LightingTech tech)
    {
        currentTech = tech;
        UpdateWallLayerIndex();
        ApplySettings();
    }

    private void UpdateWallLayerIndex()
    {
        int layer = LayerMask.NameToLayer(wallLayerName);
        if (layer != -1) wallLayerIndex = layer;
        else Debug.LogWarning($"[LightingManager] Слой '{wallLayerName}' не найден. По умолчанию использую индекс {wallLayerIndex}.");
        
        Debug.Log($"[LightingManager] Текущий слой окклюзии: {wallLayerIndex} ({wallLayerName})");
    }

    private void Awake()
    {
        UpdateWallLayerIndex();
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
        try 
        {
            bool isPlugin = currentTech == LightingTech.ShaderPlugin;
            
            // 1. Управление компонентами плагина
            if (isPlugin)
            {
                if (pluginVolumeContainer != null) pluginVolumeContainer.SetActive(true);
                UpdateShaderPluginParameters();
            }
            else
            {
                if (pluginVolumeContainer != null) pluginVolumeContainer.SetActive(false);
            }
            
            // 2. Стандартные источники света
            foreach (var light in standardLights)
            {
                if (light != null) light.enabled = !isPlugin;
            }

            // 3. Смена материалов
            SwapAllMaterials();

            Debug.Log($"[LightingManager] Технология освещения переключена на: {currentTech}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LightingManager] Ошибка при смене освещения: {e.Message}");
        }
    }

    private void SwapAllMaterials()
    {
        bool isPlugin = currentTech == LightingTech.ShaderPlugin;
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int spriteCount = 0;
        int wallCount = 0;
        int skipCount = 0;

        foreach (var r in renderers)
        {
            if (r is SpriteRenderer || r is UnityEngine.Tilemaps.TilemapRenderer)
            {
                // Пропускаем UI
                if (uiCanvas != null && r.transform.IsChildOf(uiCanvas.transform))
                {
                    skipCount++;
                    continue;
                }

                if (isPlugin)
                {
                    // Режим ПЛАГИНА
                    if (r.gameObject.layer == wallLayerIndex)
                    {
                        // Это стена/окно - даем специфичный материал стен (с текстурой)
                        if (pluginWallMaterial != null)
                        {
                            r.sharedMaterial = pluginWallMaterial;
                            wallCount++;
                        }
                        else
                        {
                            r.sharedMaterial = pluginMaterial; // Фолбэк на обычный плагин
                            spriteCount++;
                        }
                    }
                    else
                    {
                        // Обычный спрайт
                        if (pluginMaterial != null)
                        {
                            r.sharedMaterial = pluginMaterial;
                            spriteCount++;
                        }
                    }
                }
                else
                {
                    // Режим СТАНДАРТНЫЙ
                    if (standardMaterial != null)
                    {
                        r.sharedMaterial = standardMaterial;
                        spriteCount++;
                    }
                }
            }
        }
        
        Debug.Log($"[LightingManager] Материалы обновлены. Спрайтов: {spriteCount}, Стен (Occluders): {wallCount}, Пропущено (UI): {skipCount}.");
    }

    private void UpdateShaderPluginParameters()
    {
        // Синхронизируем настройки эмбиента с менеджером плагина
        var pluginMgr = Object.FindAnyObjectByType<GPU2DLightManager>();
        if (pluginMgr != null)
        {
            pluginMgr.ambient = ambientIntensity;
        }
    }
}
