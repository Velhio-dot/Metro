using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;

public class GPU2DLightFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        [Tooltip("Слой, который будет отбрасывать тени. По умолчанию - Obstacle.")]
        public LayerMask occlusionLayer = 1 << 8; // Слой Obstacle
        public int textureResolution = 512;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    public Settings settings = new Settings();
    private GPU2DLightPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new GPU2DLightPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    class GPU2DLightPass : ScriptableRenderPass
    {
        private Settings settings;
        private Shader m_MaskShader;
        private Material m_MaskMaterial;
        private RTHandle m_OcclusionHandle;
        
        private static readonly int OcclusionMapId = Shader.PropertyToID("_OcclusionMap");
        private static readonly int CamBoundsId = Shader.PropertyToID("_CamBounds");

        public GPU2DLightPass(Settings settings)
        {
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        private void EnsureMaterial()
        {
            if (m_MaskMaterial == null)
            {
                m_MaskShader = Shader.Find("Hidden/GPU2D/OcclusionMask");
                if (m_MaskShader != null) 
                {
                    m_MaskMaterial = new Material(m_MaskShader);
                }
            }
        }

        private class PassData
        {
            public RendererListHandle rendererList;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            
            if (cameraData.cameraType != CameraType.Game && cameraData.cameraType != CameraType.SceneView) return;

            EnsureMaterial();
            var cam = cameraData.camera;
            
            float height = cam.orthographicSize * 2;
            float width = height * cam.aspect;
            Shader.SetGlobalVector(CamBoundsId, new Vector4(cam.transform.position.x, cam.transform.position.y, width, height));

            var desc = cameraData.cameraTargetDescriptor;
            desc.width = settings.textureResolution;
            desc.height = Mathf.RoundToInt(settings.textureResolution / cam.aspect);
            desc.colorFormat = RenderTextureFormat.R8;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            // Используем современный метод переаллокации без предупреждений
            RenderingUtils.ReAllocateHandleIfNeeded(ref m_OcclusionHandle, desc, filterMode: FilterMode.Bilinear, wrapMode: TextureWrapMode.Clamp, name: "_OcclusionMap");
            TextureHandle occlusionTexture = renderGraph.ImportTexture(m_OcclusionHandle);

            // Настройка захвата для 2D
            var sortingSettings = new SortingSettings(cam) { criteria = SortingCriteria.CommonTransparent };
            var drawingSettings = new DrawingSettings(new ShaderTagId("Universal2D"), sortingSettings);
            drawingSettings.SetShaderPassName(1, new ShaderTagId("SRPDefaultUnlit"));
            drawingSettings.SetShaderPassName(2, new ShaderTagId("UniversalForward"));
            drawingSettings.SetShaderPassName(3, new ShaderTagId("Always"));
            
            if (m_MaskMaterial != null)
            {
                drawingSettings.overrideMaterial = m_MaskMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;
            }

            var filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.occlusionLayer);
            var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            RendererListHandle rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("GPU2D Occlusion Pass", out var passData))
            {
                passData.rendererList = rendererListHandle;

                builder.SetRenderAttachment(occlusionTexture, 0);
                builder.AllowPassCulling(false);
                builder.UseRendererList(rendererListHandle);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    // Штатная очистка черным
                    context.cmd.ClearRenderTarget(true, true, Color.black);
                    
                    if (data.rendererList.IsValid())
                    {
                        context.cmd.DrawRendererList(data.rendererList);
                    }
                });

                // Принудительная установка для глобального доступа
                Shader.SetGlobalTexture(OcclusionMapId, m_OcclusionHandle);
                builder.SetGlobalTextureAfterPass(occlusionTexture, OcclusionMapId);
            }
        }

        public void Dispose()
        {
            m_OcclusionHandle?.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        m_ScriptablePass?.Dispose();
    }
}
