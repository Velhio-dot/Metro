using UnityEngine;

[ExecuteInEditMode]
public class GPU2DLightManager : MonoBehaviour
{
    [Header("Источник света")]
    public Transform lightSource;
    public float lightRadius = 10f;
    [Range(0, 5)] public float intensity = 1.0f;
    [Range(0, 1)] public float shadowSoftness = 0.5f;

    void Update()
    {
        if (lightSource == null) return;

        Vector3 lPos = lightSource.position;
        Shader.SetGlobalVector("_LightPos", new Vector4(lPos.x, lPos.y, 0, 0));
        Shader.SetGlobalFloat("_LightRadius", lightRadius);
        Shader.SetGlobalFloat("_GlobalIntensity", intensity);
        Shader.SetGlobalFloat("_ShadowSoftness", shadowSoftness);
    }
}
