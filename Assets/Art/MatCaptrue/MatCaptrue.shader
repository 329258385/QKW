Shader "Unlit/MatCaptrue"
{
    Properties
    {
        _MainTex  ("Texture", 2D) = "white" {}
        _MainColor("MainColor", Color) = (1.0, 1.0, 1.0, 1.0)
        _MatCap  ("MatCap",  2D) = "white" {}
        _MatCapScale("_MatCapScale", Float ) = 1
        _ReflectionMap("ReflectionMap", Cube) = "" {}
        _ReflectionColor("ReflectionColor", Color) = (0.2, 0.2, 0.2, 1.0)
        _ReflectionStrength("ReflectionStrength", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            NAME "BASE"
            Tags { "LightMode" = "Always" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                //#if NORMALMAP_ON
                //float4 TtoV0    : TEXCOORD1;
                //float4 TtoV1    : TEXCOORD2;
                //float4 TtoV2    : TEXCOORD3;
                //#else
                float2 cap          : TEXCOORD1;
                float3 wsReflection : TEXCOORD2;
                //#endif
            };

            sampler2D           _MainTex;
            float4              _MainTex_ST;

            float               _ReflectionStrength;
            float4              _ReflectionColor;
            samplerCUBE         _ReflectionMap;
            
            //#if NORMALMAP_ON
            //uniform sampler2D   _BumpMap;
            //uniform float4      _BumpMap_ST;
            //#endif

            uniform sampler2D   _MatCap;
            uniform float       _MatCapScale;
            v2f vert (appdata_tan  v)
            {
                v2f o;
                o.vertex        = UnityObjectToClipPos(v.vertex);
                o.uv            = TRANSFORM_TEX(v.texcoord, _MainTex);

                // 方法 1.0
                //TANGENT_SPACE_ROTATION;

                //o.cap.x         = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
                //o.cap.y         = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);
                //o.cap           = o.cap * 0.5 + 0.5;
                o.cap.x           = dot(normalize(UNITY_MATRIX_IT_MV[0]), normalize(v.normal));
                o.cap.y           = dot(normalize(UNITY_MATRIX_IT_MV[1]), normalize(v.normal));
                o.cap             = o.cap * 0.5 + 0.5;

                float3 w          = mul(unity_ObjectToWorld, v.vertex);
                //float3 wsNormal   = mul(UNITY_MATRIX_IT_MV, v.normal );
                float3 n          = UnityObjectToWorldNormal(v.normal );
                float3 e          = w - _WorldSpaceCameraPos.xyz;
                o.wsReflection      = reflect( e, n );
                //----------------------------------------------------------------------
                //#if NORMALMAP_ON
                // 方法 2.0
                //float3 worldpos = mul(UnityObjectToWorld, v.vertex);
                //float3 worldnor = UnityObjectToWorldNormal(v.normal);
                //float3 worldtag = UnityObjectToWorldDir(v.tangent.xyz);
                //float3 worldbin = cross(worldnor, worldtag) * worldtag.w;


                //o.TtoV0         = float4(worldtag, worldpos.x);
                //o.TtoV1         = float4(worldbin, worldpos.y);
                //o.TtoV2         = float4(worldnor, worldpos.z);
                //#else
                // 方法 3.0
                //float3 e        = UnityObjectToViewPos(v.vertex);
                //float3 n        = mul( (float3x3)UNITY_MATRIX_IT_MV, v.normal);
                //float3 r        = normalize(reflect( e, n));

                //float m         = 2.82842712474619 * sqrt( r.z + 1.0 );
                //half2 mc        = r.xy / m + 0.5;
                //o.cap           = mc;
                //#endif
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 maincolor = tex2D(_MainTex, i.uv);
                fixed4 matcap;

                // 镜面反射
                float3 reflection = texCUBE(_ReflectionMap, i.wsReflection).rgb * _ReflectionColor.rgb;
                float3 color      = lerp(maincolor, reflection, _ReflectionStrength);

                //#if NORMALMAP_ON
                //float3 worldpos = float3(i.TtoV0.w, i.TtoV1.w, i.TtoV2.w);
                /// nor ---> TBN 空间
                //float3 normal   = UnpackNormal(tex2D(_BumpMap, i.uv));
                //float3 worldnor;
                //worldnor.x      = dot(i.TtoV0, normal);
                //worldnor.y      = dot(i.TtoV1, normal);
                //worldnor.z      = dot(i.TtoV2, normal);
                //worldnor        = normalize(worldnor);

                // eye pos 
                //float3 e        = normalize(worldpos - _WorldSpaceCameraPos.xyz);
                //float3 n        = worldnor;
                //float3 r        = reflect(e, n);
                //float3 vr       = normalize(mul(UNITY_MATRIX_V, r));
                //float m         = 2.82842712474619 * sqrt(vr.z + 1.0);
                //half2 cap       = vr.xy / m + 0.5;
               
                //matcap          = tex2D(_MatCap, cap * 0.5 + 0.5);
                //#else
                
                //matcap          = tex2D(_MatCap, i.cap);
                //#endif

                /// 一下对于立方体的平面会有问题
                //half2 vn;
                //vn.x            = dot(i.TtoV0, normal);
                //vn.y            = dot(i.TotV1, normal);
                matcap            = tex2D(_MatCap, i.cap);
                matcap.a          = 1;

                return fixed4(color * matcap.rgb * _MatCapScale, maincolor.a);
            }
            ENDCG
        }
    }
}
