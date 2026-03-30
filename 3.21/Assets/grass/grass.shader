Shader "Unlit/grass"
{
    Properties
    {
        _GroundColorTex("_GroundColorTex",2D)="white"{}
        _ButtomColor("Buttom Color",Color)=(1,1,1,1)
        _TopColor("Top_Color",Color)=(1,1,1,1)
        _BendRotationRandom("Bend Rotation Random",Range(0,1))=0.2

        _BladeWidth("Blade Width",Float)=0.05
        _BladeWidthRandom("Blade Width Random",Float)=0.02
        _BladeHeight("Blade Height",Float)=0.5
        _BladeHeightRandom("Blade Height Random",Float)=0.3
        _BladeForward("Blade Forward",Float)=0.1
        _BladeCurve("Blade Curve",Range(1,4))=2

        _Count("Tess Count",int)=1

        _WindDistortionMap("Wind Distortion Map",2D)="white"{}
        _WindFrequency("Wind Frequency",Vector)=(0.05,0.05,0,0)
        _WindStrength("Wind Strength",Float)=1

        _GroundTexSize("_GroundTexSize ",Float)=10
        _GroundAndGrassBlendValue("Ground And Grass Blend Value",Float)=1
        _TopAndGroundBlendValue("Top And Ground Blend Value",Range(0,1))=0.7
        _GroundBlendColor("_GroundBlendColor",Color)=(0.8,0.8,0.8,1.0)
        _GroundContrast("Ground Contrast",Range(0,5))=2

        _Saturation("Saturation",Float)=1
        _Radius("Interact Radius",Float)=0.1
        _InteractStrength("_InteractStrength",Range(0,10))=1
    }
    SubShader
    {
        Tags 
    	{ 
    		"RenderPipeline" = "UniversalRenderPipeline"
    		"RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }


            Cull Off

            HLSLINCLUDE

            

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            // 阴影所需的宏定义
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		    #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            TEXTURE2D(_GroundColorTex);SAMPLER(sampler_GroundColorTex);float4 _GroundColorTex_ST;
            TEXTURE2D(_WindDistortionMap);SAMPLER(sampler_WindDistortionMap);float4 _WindDistortionMap_ST;
            
            CBUFFER_START(UnityPerMarterial)//常量缓冲区开头
            half4 _TopColor;
            half4 _ButtomColor;
            half4 _GroundBlendColor;
            float _BendRotationRandom;
            float _BladeWidth;
            float _BladeHeight;
            float _BladeForward;
            float _BladeCurve;
            float _BladeWidthRandom;
            float _BladeHeightRandom;
            int _Count;
            float2 _WindFrequency;
            float _WindStrength;
            float _GroundTexSize;
            float _GroundAndGrassBlendValue;
            float _TopAndGroundBlendValue;
            float _GroundContrast;
            float _Saturation;
            float3 _InteractPos;
            float _Radius;
            float _InteractStrength;
            CBUFFER_END

            #define PI 3.1415926
            #define BLADE_SEGMENTS 3
        //函数
            //随机生成一个随机数
            float rand(float3 co)
            {
                return frac(sin(dot(co.xyz,float3(12.9898,78.233,53.539)))*43758.5453);
                }
            //输入角度，生成绕轴旋转矩阵
            float3x3 AngleAxis3x3(float angle,float3 axis)
            {
                float c,s;
                sincos(angle,s,c);//s = sin(angle);c = cos(angle);

                float t=1-c;
                float x=axis.x;
                float y=axis.y;
                float z=axis.z;

                return float3x3(
                    t*x*x+c,   t*x*y-s*z, t*x*z+s*y,
                    t*x*y+s*z, t*y*y+c,   t*y*z-s*x,
                    t*x*z-s*y, t*y*z+s*x, t*z*z+c
                    );

                }

            struct Attributes
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
            };

            struct Varyings
            {
                float4 pos:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos=v.vertex;
                o.normal=v.normal;
                o.tangent=v.tangent;
                return o;
            }

             //常量外壳着色器
            struct PatchTess{
                float edgeTess[3]:SV_TessFactor;     // 三条边的细分因子
                float insideTess:SV_InsideTessFactor;// 内部细分因子
                };

            PatchTess ConstantHS(InputPatch<Varyings,3>patch,//处理三个控制点的面片
                uint patchID:SV_PrimitiveID){
                    PatchTess pt;

                    //将该面片从各方面均匀地镶嵌处理为3等分
                    pt.edgeTess[0]=_Count;
                    pt.edgeTess[1]=_Count;
                    pt.edgeTess[2]=_Count;

                    pt.insideTess=_Count;//三角形内部的细分份数
                    return pt;
                    }

            //控制点外壳着色器
            struct HullOut{
                float4 posOS:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                };

            [domain("tri")]                  //patch的类型
            [partitioning("integer")]        //曲面细分的模式；integer是突变均分
            [outputtopology("triangle_cw")]  //通过细分所创的三角形的绕序，CW=Clockwise = 顺时针;CCW = Counter-Clockwise = 逆时针
            [outputcontrolpoints(3)]         //外壳着色器的执行次数，每次执行都输出一个控制点
            [patchconstantfunc("ConstantHS")]//指定常量外壳着色器函数名称的字符串
            [maxtessfactor(64.0)]            //告知驱动程序该着色器所用的最大细分因子
            HullOut HS(InputPatch<Varyings,3> input,
                uint controlPointId:SV_OutputControlPointID,uint patchId:SV_PrimitiveID){
                    HullOut output;

                    output.posOS=input[controlPointId].pos;
                    output.normal=input[controlPointId].normal;
                    output.tangent=input[controlPointId].tangent;
                    return output;
            }


            //域着色器
            struct DomainOut{
                float4 pos:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
                };

            [domain("tri")]
            DomainOut DS(PatchTess patchTess,float3 bary:SV_DomainLocation,//当前处理的细分顶点在原始三角形面片中的重心坐标
                const OutputPatch<HullOut,3>patch)//patch[]是原本三角形三顶点的坐标
                {
                    DomainOut o;
                    //使用重心坐标插值出每个细分顶点的实际位置
                    float3 p=patch[0].posOS*bary.x+patch[1].posOS*bary.y+patch[2].posOS*bary.z;
                    float3 n=patch[0].normal*bary.x+patch[1].normal*bary.y+patch[2].normal*bary.z;
                    float4 t=patch[0].tangent*bary.x+patch[1].tangent*bary.y+patch[2].tangent*bary.z;
                    float3 t1=normalize(t).xyz;
                    o.pos=float4(p,1);
                    o.normal=n;
                    o.tangent=float4(t1,t.w);
                    return o;
                    }

        //几何着色器
            struct GeomOut{
                float4 pos:SV_POSITION;
                float3 posOS:TEXCOORD0;
                float2 uv:TEXCOORD1;
                float4 shadowCoord:TEXCOORD2;
                float3 normal:NORMAL;
                };

            GeomOut VertexOutput(float3 pos,float2 uv,float3 normal){
                GeomOut o;
                o.pos=TransformObjectToHClip(pos);
                float3 posWS=TransformObjectToWorld(pos);
                o.shadowCoord=TransformWorldToShadowCoord(posWS);
                o.posOS=pos;
                o.uv=uv;
                o.normal=TransformObjectToWorldNormal(normal);
                return o;
                }
                //封装一下计算顶点位置的函数
            GeomOut GenerateGrassVertex(float3 vertexPosition,float width,float height,float forward,float2 uv,float3x3 transformMatrix){
                float3 tangentPoint=float3(width,forward,height);
                float3 tangentNormal = normalize(float3(0, -1.0, forward));
			    float3 localNormal = mul(transformMatrix, tangentNormal);
                float3 localPosition=vertexPosition+mul(transformMatrix,tangentPoint);
                return VertexOutput(localPosition,uv,localNormal);
                }

            [maxvertexcount(BLADE_SEGMENTS*2+1)]
            void geom(
                triangle DomainOut input[3],
                inout TriangleStream<GeomOut> triStream
                )
            {
                float3 pos=input[0].pos.xyz;

                float3 vNormal=input[0].normal;
                float4 vTangent=input[0].tangent;
                float3 vBinormal=normalize(cross(vNormal,vTangent)*vTangent.w);
                half3x3 tangentToLocal=half3x3(
                    vTangent.x,vBinormal.x,vNormal.x,
                    vTangent.y,vBinormal.y,vNormal.y,
                    vTangent.z,vBinormal.z,vNormal.z
                    );

                //旋转矩阵
                float3x3 facingRotationMatrix=AngleAxis3x3(rand(pos)*2*PI,float3(0,0,1));
                float3x3 bendRotationMatrix=AngleAxis3x3(rand(pos.zzx)*_BendRotationRandom*2*PI,float3(1,0,0));

                //交互旋转矩阵构建
                float3 posWS=TransformObjectToWorld(pos);
                float dist=distance(posWS,_InteractPos);
                float interactMask=1-saturate(dist/_Radius);
                float3 dirWS=normalize(posWS-_InteractPos);//草弯折的方向
                float3 disLS=mul((float3x3)unity_WorldToObject, dirWS);//转到模型空间

                float angle=interactMask*_InteractStrength;
                float3 bendAxis=normalize(cross(float3(0,0,1),disLS));

                float3x3 interactionRotation=AngleAxis3x3(angle,bendAxis);

                    //风动
                float2 uv=pos.xz*_WindDistortionMap_ST.xy+_WindDistortionMap_ST.zw+_WindFrequency*_Time.y;
                float2 windSample=(SAMPLE_TEXTURE2D_LOD(_WindDistortionMap,sampler_WindDistortionMap,uv,0).xy*2-1)*_WindStrength;
                float3 wind=normalize(float3(windSample.x,windSample.y,0));

                float3x3 windRotation=AngleAxis3x3(PI*windSample,wind);//2π*[0,1]和π*[-1,1]等价，所以不用乘2PI

                float3x3 transformationMartix=mul(mul(mul(tangentToLocal,facingRotationMatrix),bendRotationMatrix),windRotation);
                transformationMartix=mul(transformationMartix,interactionRotation);
                float3x3 transformationMartixFacing=mul(tangentToLocal,facingRotationMatrix);//用于三角形底部的矩阵

                //随机草叶高度宽度
                float width=(rand(pos.zyx)*2-1)*_BladeWidthRandom+_BladeWidth;
                float height=(rand(pos.xzy)*2-1)*_BladeHeightRandom+_BladeHeight;

                GeomOut o;

                    //弯曲
                float forward=rand(pos.yyz)*_BladeForward;

                //细分草叶顶点，并进行三角形stream
                for(int i=0;i<BLADE_SEGMENTS;i++)
                {
                    float t=i/(float)BLADE_SEGMENTS;
                    float segmentHeight=height*t;
                    float segmentWidth=width;
                    float sefmentForward=pow(t,_BladeCurve)*forward;


                    //对底部两个顶点使用transformationMartixFacing,对其他顶点使用transformationMartix
                    float3x3 transformMartix=i==0?transformationMartixFacing:transformationMartix;
                    
                    triStream.Append(GenerateGrassVertex(pos,segmentWidth,segmentHeight, sefmentForward,float2(0, t),transformMartix));
                    triStream.Append(GenerateGrassVertex(pos,-segmentWidth,segmentHeight, sefmentForward,float2(1, t),transformMartix));
            
                    }
                    triStream.Append(GenerateGrassVertex(pos,0,height,forward,float2(0.5, 1),transformationMartix));
                } 
                ENDHLSL
                    
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
        	
			HLSLPROGRAM
			#pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma hull HS
            #pragma domain DS
			#pragma target 4.6
            half4 frag(GeomOut i,half facing:VFACE):SV_Target
            {
                //float3 nDirWS=facing>0?i.normal:-i.normal;
                Light mainLight=GetMainLight(i.shadowCoord);
                //half3 lDirWS=normalize(mainLight.direction);
                half shadow=saturate(mainLight.shadowAttenuation+0.6);//+0.6让阴影颜色浅些
                //float nDotl=saturate(dot(nDirWS,lDirWS)*0.5+0.5);
                //float3 lightIntensity=nDotl* mainLight.color.rgb;

                float2 uvWS=(-i.posOS.xz/_GroundTexSize+float2(1,1)/2);

                half3 GroundColor=SAMPLE_TEXTURE2D(_GroundColorTex,sampler_GroundColorTex,uvWS)*_GroundBlendColor;

                half3 TopCol=lerp(GroundColor,_TopColor,_TopAndGroundBlendValue);
                half3 finalCol=lerp(GroundColor,TopCol,pow(i.uv.y,_GroundAndGrassBlendValue));
                finalCol = 0.5 + _GroundContrast * (finalCol - 0.5);//线性对比度
                float luminance=dot(finalCol,float3(0.299,0.587,0.114));
                finalCol=lerp(luminance,finalCol,_Saturation);//饱和度
                
                return half4(finalCol*shadow,1.0);
            }
            ENDHLSL
        }
        /*
        Pass{
            Tags{"LightMode"="ShadowCaster"}

            ZWrite On      // 开启深度写入
            ZTest LEqual   // 深度测试条件：小于等于当前深度时通过
            ColorMask 0    // 关闭颜色写入（只写深度，不输出颜色）
            Cull Off       // 关闭背面剔除（双面阴影）

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma hull HS
            #pragma domain DS
			#pragma target 4.6

			float4 frag (GeomOut i) : SV_Target
			{
				return 0;
			}
            
            ENDHLSL
            }*/
        
    }
}
