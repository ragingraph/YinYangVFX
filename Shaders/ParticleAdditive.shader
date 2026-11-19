// Additive, HDR-friendly particle shader for URP 2D (sparks, motes, glows).
// Final colour = texture * particle vertex colour * _Tint (HDR, so values above 1 feed Bloom).
Shader "YinYangVFX/ParticleAdditive"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Tint ("Tint (HDR)", Color) = (2, 2, 2, 1)

        // Default = additive glow (SrcAlpha, One). Dark motes use SrcAlpha, OneMinusSrcAlpha.
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Tint;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 color : COLOR;
            float2 uv : TEXCOORD0;
        };

        Varyings vert(Attributes v)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(v.positionOS);
            o.color = v.color;
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        float4 frag(Varyings i) : SV_Target
        {
            float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
            return float4(tex.rgb * i.color.rgb * _Tint.rgb, tex.a * i.color.a);
        }
        ENDHLSL

        Pass
        {
            Name "Particle2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Name "ParticleForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }

    Fallback Off
}
