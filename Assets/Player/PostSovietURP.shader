Shader "Hidden/PostSovietURP"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _Desaturation ("Desaturation", Range(0, 1)) = 0.85
        _Tint ("Cold Tint", Color) = (0.7, 0.8, 0.85, 1)
        _NoiseAmount ("Noise Amount", Range(0, 1)) = 0.12
        _Vignette ("Vignette Strength", Range(0, 5)) = 1.8
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "PostSovietPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Desaturation;
            float4 _Tint;
            float _NoiseAmount;
            float _Vignette;

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                // Обесцвечивание
                float luminance = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(col.rgb, luminance.xxx, _Desaturation);

                // Холодный оттенок
                col.rgb *= _Tint.rgb;

                // Зернистость / Шум
                float noise = random(input.texcoord + _Time.y) * _NoiseAmount;
                col.rgb -= noise;

                // Виньетка
                float2 coord = input.texcoord - 0.5;
                float vignette = 1.0 - dot(coord, coord) * _Vignette;
                col.rgb *= smoothstep(0.0, 1.0, vignette);

                return col;
            }
            ENDHLSL
        }
    }
}