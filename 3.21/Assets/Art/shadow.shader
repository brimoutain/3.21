Shader "Custom/shadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _BaseMap("Base Map", 2D) = "white" {}  // 为兼容LitInput添加
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,1)
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        // 在HLSLINCLUDE中声明所有Pass共享的变量
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float _Cutoff;
            float4 _ShadowColor;
            // 为兼容LitInput添加
            float4 _BaseMap_ST;
            float4 _BaseColor;
        CBUFFER_END
        ENDHLSL

        // 主渲染Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;  // 添加法线
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;  // 添加世界坐标
                float3 normalWS:TEXCOORD2;
            };
            
            v2f vert(a2v v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld(v.vertex.xyz);  // 计算世界坐标
                o.normalWS=TransformObjectToWorldNormal(v.normal);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(i.uv,_MainTex)) ;
                
                // Alpha测试
                clip(col.a - _Cutoff);
                
                // 计算阴影
                float4 shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(shadowCoord);

                // 应用阴影
                float shadowFactor = mainLight.shadowAttenuation;
                col.rgb = lerp(col.rgb * _ShadowColor.rgb, col.rgb, shadowFactor); 

                half4 noisecol=SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, TRANSFORM_TEX(i.uv,_BaseMap));
                col*=noisecol;
       

                float3 normalWS=normalize(i.normalWS);
                float NdotL=saturate(dot(normalWS,mainLight.direction));
                half3 diffuse=mainLight.color.rgb*NdotL;
                half3 ambient=col.rgb*SampleSH(normalWS);
                
               
               half3 finalColor=diffuse+ambient+ _Color*(1-noisecol);
               return half4(finalColor,col.a);
           }
            ENDHLSL
        }
        
        // 阴影投射Pass - 自定义版本（推荐）
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"="ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            float3 _LightDirection;
            float3 _LightPosition;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_TARGET
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                clip(col.a - _Cutoff);  // Alpha测试
                return 0;
            }
            ENDHLSL
        }
    }
    
    // 添加Fallback
    Fallback "Universal Render Pipeline/Lit"
}
