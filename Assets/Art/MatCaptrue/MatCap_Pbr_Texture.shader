Shader "myMatCap/Bumped/PBR"
{
    Properties
    {
		_Color	 ("Main Color", Color ) = ( 1,1,1,1)
        _MainTex ("Albedo", 	2D) = "white" {}
		
		_BumpTex ("BumpTex", 	2D ) = "bump" {}
		_BumpScale("BumpScale", Range(0,10) ) = 1
		
		_MatCapDiffuse("MatCapDiffuse", 2D) = "white" {}
		_MatCapScale  ("MatCapScale", 	Range(0, 5) ) = 1
		
		_MatCapSpec	  ("MatCapSpec", 2D) = "white" {}
		_MatCapSpecScale("MatCapSpecScale", Range(0,5) ) = 0
		
		_SpecTex     ("SpecTex", 2D ) ="white" {}
		_SpecTexScale("SpecTexScale", Range(0,2)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}

        Pass
        {
			Tags { "LightMode"="Always" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			uniform sampler2D  _BumpTex;
			uniform float4 	   _BumpTex_ST;
			uniform float 	   _BumpScale;
			
            struct v2f
            {
                float4 uv       : TEXCOORD0;
                float3 TtoV0	: TEXCCORD1;
				float3 TtoV1    : TEXCOORD2;
                float4 vertex   : SV_POSITION;
            };

            sampler2D           _MainTex;
            float4              _MainTex_ST;

            v2f vert (appdata_tan v)
            {
                v2f o;
                o.vertex            = UnityObjectToClipPos(v.vertex);
				o.uv.xy 			= TRANSFORM_TEX( v.texcoord, _MainTex );
				o.uv.zw				= TRANSFORM_TEX( v.texcoord, _BumpTex );
				
				v.normal			= normalize(v.normal);
				v.tangent			= normalize(v.tangent);
				TANGENT_SPACE_ROTATION;
				o.TtoV0 			= normalize( mul( rotation, UNITY_MATRIX_IT_MV[0].xyz ));
				o.TtoV1 			= normalize( mul( rotation, UNITY_MATRIX_IT_MV[1].xyz ));
                return o;
            }


			uniform fixed4 		_Color;
			uniform sampler2D	_MatCapDiffuse;
			uniform float		_MatCapScale;
			
			uniform sampler2D	_MatCapSpec;
			uniform float		_MatCapSpecScale;
			
			uniform sampler2D	_SpecTex;
			uniform float		_SpecTexScale;
			
			
			fixed lum( fixed3 col )
			{
				return col.r * 0.2 + col.g * 0.7 + col.b * 0.1;
			}
			
			
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col  	= tex2D( _MainTex, i.uv.xy );
				float3 normal 	= UnpackNormal( tex2D( _BumpTex, i.uv.zw ));
				normal.xy 		*= _BumpScale;
				normal.z 		= sqrt( 1.0 - saturate( dot( normal.xy, normal.xy ) ) );
				normal			= normalize( normal );
				
				half2 cap_uv;
				cap_uv.x 		= dot( i.TtoV0, normal );
				cap_uv.y 		= dot( i.TtoV1, normal );
				cap_uv			= cap_uv * 0.5 + 0.5;
				
				fixed4 matcapdif= tex2D( _MatCapDiffuse, cap_uv ) * _MatCapScale;
				fixed4 matspec  = tex2D( _MatCapSpec, cap_uv ) * _MatCapSpecScale;
				
				fixed4 specolor = tex2D( _SpecTex, i.uv.xy ) * _SpecTexScale;
				
				fixed4 diffuse  = col * matcapdif * _Color;
				fixed4 finalcol = diffuse + lerp( 0, matspec, lum( specolor.rgb ) );
				
				return finalcol;
            }
            ENDCG
        }
    }
}
