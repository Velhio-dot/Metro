Shader "GPU2D/Lit"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        [MainColor] _Color ("Tint", Color) = (1,1,1,1)
        _Ambient ("Ambient Light", Range(0, 1)) = 0.2
        _Intensity ("Light Intensity", Range(0, 5)) = 1.0
        _RaySteps ("Raymarching Steps", Range(4, 32)) = 16
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _OcclusionMap;
            float4 _MainTex_ST;
            float4 _Color;
            float _Ambient;
            float _Intensity;
            int _RaySteps;

            // Глобальные переменные от менеджера
            float4 _LightPos;
            float _LightRadius;
            float _ShadowSoftness;
            float4 _CamBounds; // x,y = pos, z,w = width,height

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            float SampleOcclusion(float2 worldPos)
            {
                // Проектируем мировые координаты в UV карты окклюзии
                float2 uv = (worldPos - _CamBounds.xy + _CamBounds.zw * 0.5) / _CamBounds.zw;
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return 0;
                
                // Используем tex2Dlod для избежания ошибок градиента в цикле
                return tex2Dlod(_OcclusionMap, float4(uv, 0, 0)).r;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Проверка на валидность границ камеры (если ширина или высота 0 - выводим красный)
                if (_CamBounds.z <= 0.01 || _CamBounds.w <= 0.01) return fixed4(1,0,0,1);

                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                if (col.a <= 0) discard;

                float2 pixelPos = i.worldPos.xy;
                float2 lightPos = _LightPos.xy;
                float dist = distance(pixelPos, lightPos);
                
                float attenuation = saturate(1.0 - (dist / _LightRadius));
                attenuation = pow(attenuation, 2);

                // Raymarching Shadow
                float shadow = 1.0;
                float2 dir = lightPos - pixelPos;
                
                [loop]
                for (int s = 1; s <= _RaySteps; s++)
                {
                    if (_RaySteps <= 0) break; // Для теста без теней

                    float t = (float)s / _RaySteps;
                    float2 samplePos = pixelPos + dir * t;
                    float occ = SampleOcclusion(samplePos);
                    
                    if (occ > 0.5)
                    {
                        shadow = lerp(shadow, 0, _ShadowSoftness * 2);
                        if (_ShadowSoftness > 0.9) { shadow = 0; break; }
                    }
                }

                float finalLight = _Ambient + (attenuation * shadow * _Intensity);
                col.rgb *= finalLight;

                return col;
            }
            ENDCG
        }
    }
}
