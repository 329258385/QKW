Shader "myMatCap/Vertex/Textured Multiply"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _MatCap  ("MatCap (RGB",2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float2 cap      : TEXCOORD1;
                float4 vertex   : SV_POSITION;
                
            };

            sampler2D           _MainTex;
            float4              _MainTex_ST;


            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex        = UnityObjectToClipPos( v.vertex );
                o.uv            = TRANSFORM_TEX( v.texcoord, _MainTex );
                
                //  normal -- object space --- view space
                float3 worldnormal;
                worldnormal     = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
                worldnormal     = mul( (float3x3)UNITY_MATRIX_V, worldnormal );
                o.cap           = worldnormal.xy * 0.5 + 0.5;
                return o;
            }

            uniform sampler2D   _MatCap;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex      = tex2D( _MainTex, i.uv );
                float4 mc       = tex2D( _MatCap, i.cap ) * tex * unity_ColorSpaceDouble;
                return mc;
            }
            ENDCG
        }
    }
}
