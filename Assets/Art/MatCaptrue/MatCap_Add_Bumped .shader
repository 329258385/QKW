Shader "myMatCap/Bumped/Textured Add"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpTex ("Normal",     2D) = "Bump"  {}
        _MatCap  ("MatCap (RGB",2D) = "white" {}
        [Toggle(MATCAP_ACCURATE)]
        _MatCapAccurate ("MatCapAccurate", Int ) = 0
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
                float4 uv       : TEXCOORD0;
                float4 vertex   : SV_POSITION;

#if MATCAP_ACCURATE
                fixed3 T        : TEXCOORD1;
                fixed3 B        : TEXCOORD2;
                fixed3 N        : TEXCOORD3;
#else
                fixed3 TtoV0    : TEXCOORD1;
                fixed3 TtoV1    : TEXCOORD2;
#endif
            };

            sampler2D           _MainTex;
            float4              _MainTex_ST;
            float4              _BumpTex_ST;

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.vertex        = UnityObjectToClipPos( v.vertex );
                o.uv.xy         = TRANSFORM_TEX( v.texcoord, _MainTex );
                o.uv.zw         = TRANSFORM_TEX( v.texcoord, _BumpTex );

#if MATCAP_ACCURATE
                fixed3 n        = UnityObjectToWorldNormal( v.normal );
                fixed3 t        = UnityObjectToWorldDir( v.tagent.xyz );
                fixed3 b        = cross( n, t ) * t.w;

                o.T             = fixed3(t.x, b.x, n.x);
                o.B             = fixed3(t.y, b.y, n.y);
                o.N             = fixed3(t.z, b.z, n.z);
#else 
                v.normal        = normalize(v.normal);
                v.tangent       = normalize(v.tangent);

                TANGENT_SPACE_ROTATION;
                o.TtoV0         = mul(rotation, normalize(UNITY_MATRIX_IT_MV[0].xyz));
                o.TtoV1         = mul(rotation, normalize(UNITY_MATRIX_IT_MV[1].xyz));
#endif
                return o;
            }

            uniform sampler2D   _BumpTex;
            uniform sampler2D   _MatCap;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex      = tex2D( _MainTex, i.uv.xy );
                fixed3 normal   = UnpackNormal( tex2D( _BumpTex, i.uv.zw ));

#if MATCAP_ACCURATE
                float3 worldnormal;
                worldnormal.x   = dot( i.T.xyz, normal );
                worldnormal.y   = dot( i.B.xyz, normal );
                worldnormal.z   = dot( i.N.xyz, normal );
                worldnormal     = mul( (float3x3)UNITY_MATRIX_V, worldnormal );
                float4 mc       = tex2D( _MatCap, worldnormal.xy * 0.5 + 0.5 ) * tex * unity_ColorSpaceDouble;
#else
                half2 uv;
                uv.x            = dot(i.TtoV0, normal);
                uv.y            = dot(i.TtoV1, normal);
                float4 mc       = tex2D( _MatCap, uv * 0.5 + 0.5 ) * tex * unity_ColorSpaceDouble;
#endif
                
#ifndef UNITY_COLORSPACE_GAMMA
                tex.rgb         = LinearToGammaSpace(tex.rgb);
                mc.rgb          = LinearToGammaSpace(mc.rgb);
                mc              *= 2.0;
                mc              = saturate(tex + mc - 1.0);
                mc.rgb          = LinearToGammaSpace(mc.rgb);
                return mc;
#else
                mc.rgb          = tex.rgb + (mc.rgb * 2.0) - 1.0;
                return mc;
#endif
            }
            ENDCG
        }
    }
}
