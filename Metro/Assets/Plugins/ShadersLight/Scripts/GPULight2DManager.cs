using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GPULight2DManager : MonoBehaviour
{
    [Header("Настройки карты")]
    public Camera mainCamera;
    public LayerMask occlusionLayer;
    public int textureResolution = 512;
    
    [Header("Параметры света")]
    public Transform lightSource;
    public float lightRadius = 10f;
    [Range(0, 10)] public float lightIntensity = 2.0f;
    [Range(0, 1)] public float shadowSoftness = 0.5f;

    [Header("Отладка")]
    public bool showDebugPreview = true;
    public RenderTexture debugMap;

    private RenderTexture occlusionMap;
    private Camera occlusionCamera;

    void OnEnable() 
    { 
        SetupCamera();
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable() 
    { 
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        if (occlusionCamera != null) DestroyImmediate(occlusionCamera.gameObject);
        if (occlusionMap != null) occlusionMap.Release();
    }

    void SetupCamera()
    {
        if (occlusionCamera != null) return;
        GameObject camObj = new GameObject("Internal_GPULight_Cam");
        camObj.hideFlags = HideFlags.HideAndDontSave;
        occlusionCamera = camObj.AddComponent<Camera>();
        occlusionCamera.enabled = false;
        occlusionCamera.clearFlags = CameraClearFlags.Color;
        occlusionCamera.backgroundColor = Color.black;
        occlusionCamera.orthographic = true;
        occlusionCamera.nearClipPlane = 0.01f;
        occlusionCamera.farClipPlane = 100f;
        occlusionCamera.cullingMask = occlusionLayer;
    }

    // Этот метод вызывается самой Unity прямо перед отрисовкой любой камеры
    void OnBeginCameraRendering(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
    {
        // Нам нужно работать только когда отрисовывается наша основная камера
        if (camera != mainCamera) return;
        if (lightSource == null) return;

        UpdateCameraAndMap();
        
        // Рендерим карту окклюзии ПРАВИЛЬНО для URP
        occlusionCamera.targetTexture = occlusionMap;
        UniversalRenderPipeline.RenderSingleCamera(context, occlusionCamera);
        
        Shader.SetGlobalTexture("_OcclusionMap", occlusionMap);
        debugMap = occlusionMap;

        UpdateShaderGlobals();
    }

    void UpdateCameraAndMap()
    {
        occlusionCamera.orthographicSize = mainCamera.orthographicSize;
        occlusionCamera.aspect = mainCamera.aspect;
        occlusionCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, -50f);
        
        if (occlusionMap == null || occlusionMap.width != textureResolution)
        {
            if (occlusionMap != null) occlusionMap.Release();
            occlusionMap = new RenderTexture(textureResolution, Mathf.RoundToInt(textureResolution / mainCamera.aspect), 16);
            occlusionMap.filterMode = FilterMode.Bilinear;
            occlusionMap.Create();
        }
    }

    void UpdateShaderGlobals()
    {
        Vector3 lPos = lightSource.position;
        Shader.SetGlobalVector("_LightPos", new Vector4(lPos.x, lPos.y, 0, 0));
        Shader.SetGlobalFloat("_LightRadius", lightRadius);
        Shader.SetGlobalFloat("_Intensity", lightIntensity);
        Shader.SetGlobalFloat("_ShadowSoftness", shadowSoftness);
        
        float height = mainCamera.orthographicSize * 2;
        float width = height * mainCamera.aspect;
        Shader.SetGlobalVector("_CamBounds", new Vector4(mainCamera.transform.position.x, mainCamera.transform.position.y, width, height));
    }
}
