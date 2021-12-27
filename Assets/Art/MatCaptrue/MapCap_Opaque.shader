Shader "myMatCap/Vertex/Opaque"
{
    Properties
    {
        _Color ("Main Color", Color ) = ( 0.5, 0.5, 0.5, 1 )
        _MatCap("MatCap RGB", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry"  }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"


            struct v2f
            {
                float2 cap    : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            uniform sampler2D   _MatCap;
            uniform fixed4      _Color;

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);

                float3 n    = normalize(unity_WorldToObject[0].xyz * v.normal.x + unity_WorldToObject[1].xyz * v.normal.y + unity_WorldToObject[2].xyz * v.normal.z);
                n           = mul((float3x3)UNITY_MATRIX_V, n );
                o.cap       = n.xy * 0.5 + 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MatCap, i.cap ) * _Color * unity_ColorSpaceDouble;
                return col;
            }
            ENDCG
        }
    }
}
