Shader "Unlit/HighFog"
{
    Properties
    {
        _MainTex ("Texture",    2D) = "white" {}
        _FogTex  ("FogTex",     2D) = "white" {}
        _CloudTex("CloudTex",   2D) = "white" {}
        _EdgeTex ("EdgeTex",    2D) = "white" {}

        _FogDensity("FogDensity", Float ) = 0.2
        _FogStart  ("FogStart",   Float ) = 0.2
        _FogEnd    ("FogEnd",     Float ) = 0.2

        _FogSensitivity("FogSensitivity",       Float) = 0.2
        _EdgeSensitivity("EdgeSensitivity",     Float) = 0.2
        _InnerSensitivity("InnerSensitivity",   Float) = 0.2
        _CloudSensitivity("CloudSensitivity",   Float) = 0.2

        _ScrollX ("ScrollX",        Range(-1, 1)) = 1
        _ScrollY ("ScrollY",        Range(-1, 1)) = 1
        _Scroll2X("Scroll2X",       Range(-1, 1)) = 1
        _Scroll2Y("Scroll2Y",       Range(-1, 1)) = 1
        _ScrollEdgeX("ScrollEdgeX", Range(-1, 1)) = 1
        _ScrollEdgeY("ScrollEdgeY", Range(-1, 1)) = 1

        _FogColor("FogColor",           Color ) = (1,1,1,1)
        _EdgeColor("EdgeColor",         Color) = (1,1,1,1)
        _HighFogColor("HighFogColor",   Color) = (1,1,1,1)
    }
    SubShader
    {
        ZTest Always Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D           _MainTex;
            sampler2D           _FogTex;
            sampler2D           _CloudTex;
            sampler2D           _EdgeTex;
            sampler2D           _CameraDepthTexture;

            struct Input
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                //float3 uv0    : TEXCOORD0;
                float2 uv1    : TEXCOORD1;
                float2 uv2    : TEXCOORD2;
                float2 uv3    : TEXCOORD3;
                float2 uv4    : TEXCOORD4;
                float2 uv5    : TEXCOORD5;
            };

           
            float4              _MainTex_ST;
            float4              _CloudTex_ST;
			float4 				_FogTex_ST;
            float4              _EdgeTex_ST;
            float4              _MainTex_TexelSize;
            
            // fog
            float               _FogDensity;
            float               _FogEnd;
			float				_FogStart;
            float               _FogSensitivity;
            float               _EdgeSensitivity;
            float               _InnerSensitivity;
            float               _CloudSensitivity;
            float               _ScrollX;
            float               _ScrollY;
            float               _Scroll2X;
            float               _Scroll2Y;
            float               _ScrollEdgeX;
            float               _ScrollEdgeY;
            float               _worldWidth;
            float               _worldHeight;
            float4              _FogColor;
            float4              _EdgeColor;
            float4              _HighFogColor;



            float4 GetWorldPositionFromDepth(float2 uv, float linearDepth )
            {
                float camPosZ   = _ProjectionParams.y + (_ProjectionParams.z - _ProjectionParams.y) * linearDepth;
                float height    = 2 * camPosZ / unity_CameraProjection._m11;
                float width     = _ScreenParams.x / _ScreenParams.y * height;

                float camPosX   = width * uv.x - width / 2;
                float camPosY   = height * uv.y - height / 2;
                float4 camPos   = float4(camPosX, camPosY, camPosZ, 1.0);
                return mul(unity_CameraToWorld, camPos);
            }

            v2f vert (Input v, uint vid : SV_VertexID)
            {
                v2f o;

                half index          = v.vertex.z;
                v.vertex.z          = 0.1;

                o.vertex            = UnityObjectToClipPos(v.vertex);

                /// 复制各种参数到PS
                o.uv1          		= v.uv;
                o.uv2          		= v.uv;

                float2 scroll;
                scroll.xy           = _Time * float2(_ScrollX, _ScrollY);
                o.uv3          		= frac(scroll.xy);

                scroll.xy           = _Time * (-float2(_Scroll2X, _Scroll2Y ));
                o.uv4          		= frac(scroll.xy);

                scroll.xy           = _Time * float2(_ScrollEdgeX, _ScrollEdgeY);
                o.uv5          		= frac( scroll.xy );
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float rawDepth      = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv2);
                float dpth          = Linear01Depth(rawDepth);

                float4 campos;
                campos.xyz          = GetWorldPositionFromDepth(i.uv2, dpth);
                campos.xz           = campos.xz / float2( _worldWidth, _worldHeight );

                // 高度差
                float deltaH		= (_FogEnd * _FogSensitivity - campos.y) * _FogDensity;
				campos.xy 			= campos.xy * _FogTex_ST.xy + _FogTex_ST.zw;
                
				float fogtexR          = tex2D( _FogTex, campos.xz ).r;
                bool fogmark1       = false;
                bool fogmark2       = false;
                fogmark1            = fogtexR >= 0.1 ? true : false;
                fogmark2            = fogtexR < 1.0 ? true : false;
				fogtexR				= 1 - fogtexR;

                fogmark1       		= fogmark1 * fogmark2;
                float2 _u_xlat1     = campos.xz * _EdgeTex_ST.xy + _EdgeTex_ST.zw;
                _u_xlat1            = _u_xlat1 + i.uv5.xy;

                float mark      	= 1 - tex2D(_EdgeTex, _u_xlat1 ).r;
                mark            	= mark * _EdgeSensitivity;
                mark                = fogtexR * mark;

                float3 edgefog;
                float innerSensitivity    = mark * _InnerSensitivity;
                edgefog    			= _EdgeColor.xyz - _HighFogColor.xyz;
				edgefog    			+= _HighFogColor.xyz;
                edgefog             = edgefog * innerSensitivity + _HighFogColor.xyz;
                edgefog             = lerp(edgefog, _HighFogColor, fogmark1 );
				edgefog				*= mark;

				// 云流动
                float2 clouduv2     = campos.xz * _CloudTex_ST.xy + i.uv3;
                float2 clouduv1     = campos.xz * _CloudTex_ST.xy + i.uv4;

                float4 cloud1, cloud2;
                cloud1              = tex2D(_CloudTex, clouduv1);
                cloud2              = tex2D(_CloudTex, clouduv2);
                cloud1              += cloud2;
                cloud1              = cloud1 * _FogColor * _CloudSensitivity;
                //cloud1        		= cloud1 * _CloudSensitivity * mark * fogtexR;
				
				
				fixed4 col;
				fixed4 maincolor 	= tex2D(_MainTex, i.uv1 );
                col.xzw             = _EdgeColor.rgb - maincolor.rgb;
				maincolor.rgb		= mark * col.xzw + maincolor.rgb;
				cloud1				= fogtexR * cloud1 + maincolor;
				col.xzw				= cloud1.xyz - edgefog.xyz;
				
				maincolor.x			= (_FogEnd - _FogStart) * _FogSensitivity;
				deltaH				= deltaH / maincolor.x;
				deltaH              = clamp(deltaH, 0, 1 );
				maincolor.x 		= (deltaH * -2.0) + 3.0;
				deltaH				*= deltaH;
				deltaH				= deltaH * maincolor.x;
				deltaH				= fogtexR * deltaH;
				
				//deltaH				*= mark;
				//col.rgb             = maincolor.rgb * ( 1 - deltaH + 0.1) + cloud1.rgb * deltaH;
				//col.a 				= 1;
				//return fixed4(mark, 0, 0,1 );
				col.xyz 			= deltaH * col.xzw + cloud1.rgb;
				col.a				= 1;
				return col;
            }
            ENDCG
        }
    }

    Fallback off
}
