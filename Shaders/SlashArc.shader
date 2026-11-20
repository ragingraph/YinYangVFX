// Procedural crescent sword-slash for URP 2D. No texture needed.
// Put it on a SpriteRenderer whose sprite is a plain white, Full Rect, non-atlased square
// (the builder makes one). The shader works in the sprite's UV space (-1..1 after remap).
//
// Driven by SlashEffect.cs through a MaterialPropertyBlock:
//   _Progress  0..1  how far the blade has swept
//   _Dissolve  0..1  noise burn-out at the end
//   _Seed            per-slash noise offset
//   _ArcCenter / _ArcHalfSpan / _Radius / _TailLength  arc geometry
Shader "YinYangVFX/SlashArc"
{
    Properties
    {
        [HDR] _CoreColor ("Core Color (HDR)", Color) = (3, 3, 3.2, 1)
        [HDR] _EdgeColor ("Edge Color (HDR)", Color) = (0.25, 1.1, 3.2, 1)

        _Progress ("Progress", Range(0, 1)) = 1
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _Seed ("Seed", Float) = 7

        _ArcCenter ("Arc Center Angle (deg)", Float) = 0
        _ArcHalfSpan ("Arc Half Span (deg)", Float) = 78
        _Radius ("Radius", Range(0.1, 0.95)) = 0.62
        _Thickness ("Thickness", Range(0.02, 0.9)) = 0.22
        _OuterRatio ("Outer Edge Ratio", Range(0.02, 0.9)) = 0.18
        _TipSharp ("Tip Sharpness", Range(0.2, 2)) = 0.8
        _TailLength ("Tail Length", Range(0.1, 2)) = 0.85
        _HeadTaper ("Head Taper", Range(0.02, 0.6)) = 0.16
        _TrailPow ("Trail Power", Range(0.2, 4)) = 1.2
        _CorePower ("Core Power", Range(0.5, 8)) = 4
        _InnerFalloff ("Inner Falloff", Range(0.3, 4)) = 1.8

        _NoiseScale ("Noise Scale (along, across)", Vector) = (3, 14, 0, 0)
        _NoiseSpeed ("Noise Speed", Float) = 2
        _Distort ("Edge Distortion", Range(0, 1)) = 0.35
        _Streak ("Streak Amount", Range(0, 1)) = 0.45
        _DissolveScale ("Dissolve Noise Scale", Float) = 3

        // Blend mode. Default = additive glow (SrcAlpha, One).
        // Ink / dark slashes use SrcAlpha, OneMinusSrcAlpha and an Alpha Boost > 1 so the dark body is opaque.
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1
        _AlphaBoost ("Alpha Boost", Range(0.5, 6)) = 1

        // Forward-sweep look (thin and far at the start, thick and near at the end). 0 = off, the normal crescent.
        _Ramp ("Width Ramp (0 = crescent, 1 = grows along the arc)", Range(0, 1)) = 0
        _RampPow ("Width Ramp Power", Range(0.3, 4)) = 1.6
        _RadiusRamp ("Radius Ramp (later part is bigger)", Range(0, 1)) = 0
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

        CBUFFER_START(UnityPerMaterial)
            float4 _CoreColor;
            float4 _EdgeColor;
            float _Progress;
            float _Dissolve;
            float _Seed;
            float _ArcCenter;
            float _ArcHalfSpan;
            float _Radius;
            float _Thickness;
            float _OuterRatio;
            float _TipSharp;
            float _TailLength;
            float _HeadTaper;
            float _TrailPow;
            float _CorePower;
            float _InnerFalloff;
            float4 _NoiseScale;
            float _NoiseSpeed;
            float _Distort;
            float _Streak;
            float _DissolveScale;
            float _AlphaBoost;
            float _Ramp;
            float _RampPow;
            float _RadiusRamp;
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
            float2 p : TEXCOORD0; // -1..1 across the sprite
        };

        float hash21(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float vnoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float a = hash21(i);
            float b = hash21(i + float2(1, 0));
            float c = hash21(i + float2(0, 1));
            float d = hash21(i + float2(1, 1));
            float2 u = f * f * (3.0 - 2.0 * f);
            return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
        }

        float fbm(float2 p)
        {
            float v = 0.0;
            float a = 0.5;
            [unroll]
            for (int k = 0; k < 3; k++)
            {
                v += a * vnoise(p);
                p = p * 2.03 + 17.1;
                a *= 0.5;
            }
            return v;
        }

        Varyings vert(Attributes v)
        {
            Varyings o;
            o.positionCS = TransformObjectToHClip(v.positionOS);
            o.color = v.color;
            o.p = v.uv * 2.0 - 1.0;
            return o;
        }

        float4 frag(Varyings i) : SV_Target
        {
            float2 p = i.p;
            float r = length(p);

            // Position along the arc: t = 0 at the top end, 1 at the bottom end (clockwise sweep).
            float angDeg = degrees(atan2(p.y, p.x));
            float delta = angDeg - _ArcCenter;
            delta = delta - 360.0 * floor((delta + 180.0) / 360.0);
            float t = 0.5 - delta / (2.0 * _ArcHalfSpan);
            float inSpan = step(0.0, t) * step(t, 1.0);
            float prof = pow(saturate(sin(PI * t)), _TipSharp) * inSpan;
            float rampProf = pow(saturate(t), _RampPow) * saturate((1.0 - t) / 0.06) * inSpan;
            prof = lerp(prof, rampProf, _Ramp);

            // Moving window: the head leads, the tail follows.
            float tHead = _Progress * (1.0 + _TailLength);
            float taper = pow(saturate((tHead - t) / _HeadTaper), 0.6);
            float trail = saturate((t - (tHead - _TailLength)) / _TailLength);
            trail = pow(trail, _TrailPow);

            float w = _Thickness * prof * taper * (0.5 + 0.5 * trail);

            float s = r - _Radius * (1.0 + _RadiusRamp * saturate(t));
            float2 nuv = float2(t * _NoiseScale.x - _Time.y * _NoiseSpeed + _Seed,
                                s * _NoiseScale.y + _Seed * 1.7);
            float n = fbm(nuv);
            s += (n - 0.5) * _Distort * w;

            float outerW = max(w * _OuterRatio, 1e-4);
            float innerW = max(w * (1.0 - _OuterRatio), 1e-4);
            float sd = s > 0.0 ? s / outerW : -s / innerW;

            float bodyOut = 1.0 - smoothstep(0.75, 1.0, sd);
            float bodyIn = pow(saturate(1.0 - sd), _InnerFalloff);
            float body = (s > 0.0 ? bodyOut : bodyIn) * step(1e-4, w);

            float heat = pow(body, _CorePower);
            float3 col = lerp(_EdgeColor.rgb, _CoreColor.rgb, heat);
            float streak = (1.0 - _Streak) + _Streak * saturate(n * 1.8);

            float a = body * saturate(trail * 1.4 + 0.1);

            // Noise burn-out
            float dn = fbm(float2(p.x * _DissolveScale + _Seed, p.y * _DissolveScale - _Time.y * 0.3));
            float thr = lerp(-0.12, 1.0, _Dissolve);
            a *= smoothstep(thr, thr + 0.12, dn);

            return float4(col * streak * i.color.rgb, saturate(a * _AlphaBoost) * i.color.a);
        }
        ENDHLSL

        // 2D Renderer picks the pass by LightMode, same pattern as Sprite-Unlit-Default.
        Pass
        {
            Name "Slash2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Name "SlashForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }

    Fallback Off
}
