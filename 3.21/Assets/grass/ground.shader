Shader "Unlit/ground"
{
     Properties
    {
        _MainTex ("BaseTex", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		    #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            half4 _Color;

            struct Attributes
            {
                float4 vertex:POSITION;
                float2 uv:TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos:SV_POSITION;
                float3 posOS:TEXCOORD1;
                float2 uv:TEXCOORD0;
                float4 shadowCoord:TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos=TransformObjectToHClip(v.vertex);
                o.posOS=v.vertex;
                o.uv=v.uv;
                float3 posWS=TransformObjectToWorld(v.vertex);
                o.shadowCoord=TransformWorldToShadowCoord(posWS);
                return o;
            }

            half4 frag(Varyings i):SV_Target
            {
                Light mainLight=GetMainLight(i.shadowCoord);
                half shadow=saturate(mainLight.shadowAttenuation+0.6);
                half3 MainCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb*_Color;
                return half4(MainCol*shadow,1.0);
            }
            ENDHLSL
        }
    }
}
