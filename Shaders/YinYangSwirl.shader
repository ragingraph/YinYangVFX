// Procedural yin-yang vortex for URP 2D. No texture needed.
// A spinning taiji disc whose S-curve is twisted into a vortex, surrounded by two interleaved
// spiral arms of light and dark energy that drift inward. Alpha blended, so the dark half is really dark.
//
// Put it on a SpriteRenderer with a plain white, Full Rect square sprite (the builder makes one).
// The quad covers radius 1.6 in "disc units": the taiji disc has radius 1, the wisps live in 1..1.6.
//
// Driven by YinYangSwirl.cs through a MaterialPropertyBlock:
//   _Appear 0..1  grow in      _Fade 0..1  noise burn-out      _Spin  degrees      _Seed  per-instance noise offset
Shader "YinYangVFX/YinYangSwirl"
{
    Properties
    {
        [HDR] _YangColor ("Yang (light) colour (HDR)", Color) = (2.2, 2.0, 1.5, 1)
        [HDR] _YinColor ("Yin (dark) colour", Color) = (0.02, 0.02, 0.03, 1)
        [HDR] _RimColor ("Rim glow (HDR)", Color) = (3.0, 2.0, 0.5, 1)

        _Appear ("Appear", Range(0, 1)) = 1
        _Fade ("Fade", Range(0, 1)) = 0
        _Spin ("Spin (degrees)", Float) = 0
        _Seed ("Seed", Float) = 3

        _Twist ("Vortex Twist (radians)", Range(0, 4)) = 0.8
        [IntRange] _Arms ("Spiral Arms", Range(1, 6)) = 2
        _SpiralTight ("Spiral Tightness", Range(0, 6)) = 1.6
        _ArmSpeed ("Arm Inward Speed", Float) = 0.35
        _OuterRadius ("Wisp Outer Radius", Range(1.1, 1.6)) = 1.6
        _DissolveScale ("Dissolve Noise Scale", Float) = 2.5
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

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _YangColor;
            float4 _YinColor;
            float4 _RimColor;
            float _Appear;
            float _Fade;
            float _Spin;
            float _Seed;
            float _Twist;
            float _Arms;
            float _SpiralTight;
            float _ArmSpeed;
            float _OuterRadius;
            float _DissolveScale;
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

        float2 rot(float2 v, float a)
        {
            float c = cos(a);
            float s = sin(a);
            return float2(c * v.x - s * v.y, s * v.x + c * v.y);
        }

        // 1 inside the circle, 0 outside, with a small anti-aliased edge.
        float circle(float2 q, float2 center, float radius)
        {
            const float eps = 0.012;
            return 1.0 - smoothstep(radius - eps, radius + eps, length(q - center));
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
            const float eps = 0.012;
            const float quadExtent = 1.6;

            // Disc units: the taiji disc has radius 1. Grows from 35% while appearing.
            float grow = 1.0 - pow(1.0 - saturate(_Appear), 3.0);
            float scale = lerp(0.35, 1.0, grow);
            float2 p = i.p * quadExtent / scale;
            float r = length(p);
            float spinRad = radians(_Spin);

            // ---- taiji disc, twisted into a vortex (inner part turns more than the rim)
            float tw = spinRad + _Twist * (1.0 - saturate(r));
            float2 q = rot(p, -tw);

            float disc = 1.0 - smoothstep(1.0 - eps, 1.0 + eps, r);
            float white = smoothstep(-eps, eps, -q.x);                 // left half is light
            white = lerp(white, 1.0, circle(q, float2(0.0, 0.5), 0.5));    // light head on top
            white = lerp(white, 0.0, circle(q, float2(0.0, -0.5), 0.5));   // dark head at the bottom
            white = lerp(white, 0.0, circle(q, float2(0.0, 0.5), 0.13));   // dark dot in the light head
            white = lerp(white, 1.0, circle(q, float2(0.0, -0.5), 0.13));  // light dot in the dark head

            float nIn = fbm(q * 2.2 + _Seed);
            float3 discCol = lerp(_YinColor.rgb, _YangColor.rgb, white);
            discCol *= 0.8 + 0.4 * nIn;
            float rim = smoothstep(0.80, 0.99, r) * disc;
            discCol += _RimColor.rgb * rim;

            // ---- two spiral arms (light / dark) drifting inward around the disc
            float ang = atan2(p.y, p.x) / (2.0 * PI);
            float u = (ang - spinRad / (2.0 * PI)) * _Arms + r * _SpiralTight + _Time.y * _ArmSpeed;
            float f = frac(u);
            float sel = step(0.5, f);                // 0 = light arm, 1 = dark arm
            float lf = f - sel * 0.5;                // 0..0.5 inside the arm
            float bump = smoothstep(0.0, 0.08, lf) * (1.0 - smoothstep(0.40, 0.5, lf));

            float falloff = saturate((_OuterRadius - r) / max(_OuterRadius - 1.0, 1e-3));
            falloff = pow(falloff, 1.2);
            float outside = smoothstep(0.98, 1.04, r);
            float nW = fbm(p * 2.5 + float2(_Seed, -_Time.y * 0.3));
            float wisp = bump * falloff * outside * saturate((nW - 0.12) * 2.4);

            // ---- compose
            float wispA = wisp * (1.0 - disc);
            float outA = disc + wispA;
            float3 wispCol = lerp(_YangColor.rgb, _YinColor.rgb, sel);
            float3 rgb = (discCol * disc + wispCol * wispA) / max(outA, 1e-4);

            // appear / burn-out
            float a = outA * smoothstep(0.0, 0.35, _Appear);
            float dn = fbm(float2(p.x * _DissolveScale + _Seed, p.y * _DissolveScale - _Time.y * 0.25));
            float thr = lerp(-0.12, 1.0, _Fade);
            a *= smoothstep(thr, thr + 0.12, dn);

            return float4(rgb * i.color.rgb, saturate(a) * i.color.a);
        }
        ENDHLSL

        Pass
        {
            Name "Swirl2D"
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Name "SwirlForward"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }

    Fallback Off
}
