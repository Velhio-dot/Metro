Shader "GPU2D/Lit_Pro"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint", Color) = (1,1,1,1)
        _Ambient ("Ambient Light", Range(0, 1)) = 0.2
        _Intensity ("Base Intensity", Range(0, 5)) = 1.0
        _RaySteps ("Shadow Quality", Range(4, 64)) = 16
        _ShadowBias ("Shadow Bias", Range(0, 0.1)) = 0.02
        _LightHeight ("Light Z-Height", Range(0, 1)) = 0.5
        _WallHeight ("Wall Z-Height", Range(0, 2)) = 1.0
        [Toggle] _IsWall ("Is Wall", Float) = 0
        [Toggle] _DebugMode ("Display Occlusion Map", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "CanUseSpriteAtlas"="True" }
        LOD 100
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_OcclusionMap);
            SAMPLER(sampler_OcclusionMap);

            float4 _MainTex_ST;
            float4 _Color;
            float _LightRadius;
            float _Ambient;
            float _Intensity;
            int _RaySteps;
            float _ShadowBias;
            float _LightHeight;
            float _WallHeight;
            float _DebugMode;
            float4 _CamBounds;
            float _IsWall;

            // Глобальные переменные от Render Feature и Менеджера
            float4 _LightPos;
            float _GlobalIntensity;
            float _ShadowSoftness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.worldPos = mul(unity_ObjectToWorld, input.positionOS);
                output.uv = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                output.color = input.color * _Color;
                return output;
            }

            float SampleOcclusion(float2 worldPos)
            {
                float2 uv = (worldPos - _CamBounds.xy + _CamBounds.zw * 0.5) / _CamBounds.zw;
                
                // Если мы вышли за границы камеры - препятствий нет
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return 0;
                
                return SAMPLE_TEXTURE2D_LOD(_OcclusionMap, sampler_OcclusionMap, uv, 0).r;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 worldPos = input.worldPos.xy;

                if (_DebugMode > 0.5)
                {
                    float occ = SampleOcclusion(worldPos);
                    // Чистая отладка: белые стены на черном фоне
                    return half4(occ, occ, occ, 1);
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                if (col.a <= 0) discard;

                float2 lightPos = _LightPos.xy;
                float dist = distance(worldPos, lightPos);
                
                if (dist > _LightRadius)
                    return half4(col.rgb * _Ambient, col.a);

                float shadow = 1.0;
                float2 dir = lightPos - worldPos;
                
                // Высота текущего пикселя (0 для пола, градиент для стен)
                float pixelHeight = _IsWall > 0.5 ? input.uv.y * _WallHeight : 0.0;
                
                // Дизеринг (шум) для сглаживания шагов
                float noise = frac(52.9829189 * frac(dot(input.positionCS.xy, float2(0.06711056, 0.00583715))));
                
                int steps = (int)_RaySteps;
                float stepSize = 1.0 / (float)steps;

                [loop]
                for (int s = 0; s < steps; s++)
                {
                    float t = ((float)s + noise) * stepSize;
                    if (t < _ShadowBias) continue;

                    // В 2.5D высота луча меняется от pixelHeight до _LightHeight
                    float rayHeight = lerp(pixelHeight, _LightHeight, t);
                    
                    float2 samplePos = worldPos + dir * t;
                    
                    // Если высота луча на этом этапе меньше высоты стены (1.0), то это тень
                    if (SampleOcclusion(samplePos) > 0.5 && rayHeight < _WallHeight)
                    {
                        shadow = 0.0;
                        break;
                    }
                }

                float atten = saturate(1.0 - (dist / _LightRadius));
                atten = pow(atten, 2);

                float finalLight = _Ambient + (atten * shadow * _Intensity);
                col.rgb *= finalLight;

                return half4(col.rgb, col.a);
            }
            ENDHLSL
        }
    }
}
