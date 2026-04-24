using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class GPU2D_Diagnostic : MonoBehaviour
{
    [ContextMenu("Run Diagnostic")]
    void Start()
    {
        Debug.Log("<color=cyan><b>[GPU2D Diagnostic]</b> Running check...</color>");

        // 1. Проверка Pipeline Asset
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (pipeline == null)
        {
            Debug.LogError("<b>[ERROR]</b> Active pipeline is NOT Universal Render Pipeline!");
            return;
        }

        // 2. Проверка слоев
        int layer = LayerMask.NameToLayer("occlusion");
        if (layer == -1)
        {
            Debug.LogError("<b>[ERROR]</b> Layer 'occlusion' not found! Create it in Project Settings -> Tags and Layers.");
        }
        else
        {
            var objects = GameObject.FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None);
            bool foundWall = false;
            foreach (var obj in objects)
            {
                if (obj.gameObject.layer == layer) { foundWall = true; break; }
            }
            if (!foundWall) Debug.LogWarning("<b>[WARNING]</b> No Tilemaps found on 'occlusion' layer.");
            else Debug.Log("<b>[OK]</b> Walls on 'occlusion' layer found.");
        }

        // 3. Проверка параметров шейдера
        var lightPos = Shader.GetGlobalVector("_LightPos");
        if (lightPos == Vector4.zero) Debug.LogWarning("<b>[WARNING]</b> _LightPos is zero. Make sure GPU2DLightManager is in the scene and player is assigned.");
        else Debug.Log("<b>[OK]</b> Global light parameters are being set.");

        Debug.Log("<color=cyan><b>[DIAGNOSTIC COMPLETE]</b> Check Console for warnings/errors.</color>");
    }
}
