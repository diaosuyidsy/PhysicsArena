// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Rainbow" {
	Properties {
		_EnvTex        ("Environment", CUBE) = "black" {}
		_NormalTex     ("Normal", 2D) = "black" {}
		_RainbowTex    ("Rainbow", 2D) = "black" {}
		_RainbowInten  ("Rainbow Intensity", Range(0, 1)) = 0.5
		_Lod           ("Cube Lod", Float) = 0
		_Transparency  ("Transparency", Float) = 1
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		samplerCUBE _EnvTex;
		sampler2D _RainbowTex;
		float _RainbowInten, _Transparency;
#ifdef CRYSTAL_GLASS_BUMP
		sampler2D _NormalTex;
		float4 _NormalTex_ST;
#endif
#ifdef CRYSTAL_GLASS_LOD
		float _Lod;
#endif
		struct v2f
		{
			float4 pos : POSITION;
			float3 view : TEXCOORD0;  // world space view
			float3 norm : TEXCOORD1;  // world space normal
#ifdef CRYSTAL_GLASS_BUMP
			float2 tex : TEXCOORD4;
			float3 tan : TEXCOORD5;   // world space tangent
			float3 bin : TEXCOORD6;   // world space binormal
#endif
		};
		v2f vert (appdata_tan v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.norm = mul((float3x3)unity_ObjectToWorld, v.normal);
			o.view = WorldSpaceViewDir(v.vertex);
#ifdef CRYSTAL_GLASS_BUMP
			o.tex = TRANSFORM_TEX(v.texcoord, _NormalTex);
			TANGENT_SPACE_ROTATION;
			o.tan = mul((float3x3)unity_ObjectToWorld, v.tangent.xyz);
			o.bin = mul((float3x3)unity_ObjectToWorld, binormal);
#endif
			return o;
       	}
       	float4 frag (v2f i) : COLOR
		{
			float3 N = normalize(i.norm);
#ifdef CRYSTAL_GLASS_BUMP
			float3 bump = tex2D(_NormalTex, i.tex).rgb;
			bump = normalize(bump * 2.0 - 1.0);
			float3 T = normalize(i.tan);
			float3 B = normalize(i.bin);
			N = normalize(N + T * bump.x - B * bump.y);
#endif
			float3 V = normalize(i.view);

			float vdn = dot(V, N);
			float3 rainbow = tex2D(_RainbowTex, float2(vdn, 0.0)).rgb;
			
			float3 r = reflect(-V, N);
#ifdef CRYSTAL_GLASS_LOD
			float3 refl = texCUBElod(_EnvTex, float4(r, _Lod)).rgb;
#else
			float3 refl = texCUBE(_EnvTex, r).rgb;
#endif
			float3 c = lerp(refl, rainbow, _RainbowInten * vdn);
			return float4(c, (1.0 - vdn) * _Transparency);
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		
		Stencil{
		    Ref 1
		    Comp Notequal
		    Pass keep
		    
		}
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ CRYSTAL_GLASS_BUMP
			#pragma multi_compile _ CRYSTAL_GLASS_LOD
			ENDCG
		}
	}
	FallBack "Diffuse"
}