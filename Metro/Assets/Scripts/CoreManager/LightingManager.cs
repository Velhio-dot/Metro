using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Управляет глобальными настройками освещения и переключением технологий (Standard vs Plugin).
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

    [Header("Ссылки на компоненты (для плагина)")]
    [Tooltip("Здесь будут ссылки на объекты плагина, когда мы их вернем из бэкапа.")]
    [SerializeField] private GameObject pluginVolumeContainer;

    public void SetLightingTech(LightingTech tech)
    {
        currentTech = tech;
        ApplySettings();
    }

    private void ApplySettings()
    {
        bool isPlugin = currentTech == LightingTech.ShaderPlugin;

        // Включаем/выключаем контейнер плагина (если он есть)
        if (pluginVolumeContainer != null)
        {
            pluginVolumeContainer.SetActive(isPlugin);
        }

        // В будущем здесь можно управлять глобальными Light2D или пресетами URP
        Debug.Log($"[LightingManager] Переключено на: {currentTech}");
    }

    private void Awake()
    {
        ApplySettings();
    }
}
