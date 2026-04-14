using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Reflection;

/// <summary>
/// Автоматизированный инструмент для быстрой настройки GPU2D освещения в сцене.
/// </summary>
public class GPU2DSetupTool : EditorWindow
{
    [MenuItem("Metro/Lighting/GPU2D Scene Setup")]
    public static void ShowWindow()
    {
        GetWindow<GPU2DSetupTool>("GPU2D Setup");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Этот инструмент создаст необходимые объекты для работы Shader Plugin в текущей сцене.", MessageType.Info);
        GUILayout.Space(10);

        if (GUILayout.Button("ОЧИСТИТЬ И НАСТРОИТЬ ВСЁ", GUILayout.Height(40)))
        {
            SetupLighting();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("ПОПРОБОВАТЬ ПОЧИНИТЬ RENDERER (АВТОМАТИЧЕСКИ)"))
        {
            RepairRenderer();
        }
    }

    private void RepairRenderer()
    {
        // Ищем Renderer Data
        string[] guids = AssetDatabase.FindAssets("t:UniversalRendererData");
        if (guids.Length == 0)
        {
            Debug.LogError("[GPU2D Repair] Не найден ни один UniversalRendererData ассет!");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        UniversalRendererData rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);

        if (rendererData == null)
        {
            Debug.LogError("[GPU2D Repair] Ошибка загрузки Renderer Data по пути: " + path);
            return;
        }

        // Пытаемся добавить фитчу если её нет
        bool found = false;
        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature != null && feature.GetType().Name == "GPU2DLightFeature")
            {
                found = true;
                break;
            }
        }

        if (!found)
        {
            var feature = ScriptableObject.CreateInstance<GPU2DLightFeature>();
            feature.name = "GPU 2D Light Feature";
            AssetDatabase.AddObjectToAsset(feature, rendererData);
            
            // Через рефлексию добавляем в приватный список m_RendererFeatures
            FieldInfo featuresField = typeof(ScriptableRendererData).GetField("m_RendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
            if (featuresField != null)
            {
                var list = (System.Collections.Generic.List<ScriptableRendererFeature>)featuresField.GetValue(rendererData);
                list.Add(feature);
            }
            
            EditorUtility.SetDirty(rendererData);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=green>[GPU2D Repair]</color> Feature успешно добавлена в " + rendererData.name);
        }
        else
        {
            Debug.Log("[GPU2D Repair] Feature уже присутствует в рендерере.");
        }
    }

    private void SetupLighting()
    {
        // 1. Ищем или создаем Риг
        GameObject rig = GameObject.Find("GPU_LIGHT_RIG");
        if (rig == null)
        {
            rig = new GameObject("GPU_LIGHT_RIG");
            Undo.RegisterCreatedObjectUndo(rig, "Create GPU Rig");
        }

        // 2. Добавляем менеджер
        var manager = rig.GetComponent<GPU2DLightManager>();
        if (manager == null) manager = rig.AddComponent<GPU2DLightManager>();

        // 3. Пытаемся найти игрока
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            manager.lightSource = player.transform;
            Debug.Log("<color=green>[GPU2D Setup]</color> Игрок найден и назначен источником света.");
        }
        else
        {
            Debug.LogWarning("[GPU2D Setup] Игрок с тегом 'Player' не найден. Назначьте источник света в GPU2DLightManager вручную.");
        }

        // 4. Пытаемся связать с LightingManager
        LightingManager lm = Object.FindAnyObjectByType<LightingManager>();
        if (lm != null)
        {
            lm.pluginVolumeContainer = rig;
            EditorUtility.SetDirty(lm);
            Debug.Log("<color=green>[GPU2D Setup]</color> Риг привязан к LightingManager.");
        }

        Selection.activeGameObject = rig;
        Debug.Log("<color=cyan><b>[GPU2D Setup]</b> Настройка завершена!</color>");
    }

    private void HighlightRenderer()
    {
        Debug.Log("<b>[ИНСТРУКЦИЯ]</b>:");
        Debug.Log("1. Найдите файл <b>Renderer2D.asset</b> в папке Assets/Settings.");
        Debug.Log("2. В нижней части Инспектора нажмите кнопку <b>Add Renderer Feature</b>.");
        Debug.Log("3. Выберите из списка <b>GPU 2D Light Feature</b>.");
        Debug.Log("4. В настройках фитчи убедитесь, что <b>Occlusion Layer</b> установлен в <b>Obstacle</b>.");
        
        // Попытка автоматически найти ассет
        string[] guids = AssetDatabase.FindAssets("t:UniversalRendererData");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (renderer != null) EditorGUIUtility.PingObject(renderer);
        }
    }
}
