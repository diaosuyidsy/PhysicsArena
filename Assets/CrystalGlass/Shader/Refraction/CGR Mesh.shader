// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Refraction/Mesh" {
	Properties {
		_BumpTex    ("Bump", 2D) = "black" {}
		_BumpScale  ("Bump Scale", Range(0, 0.1)) = 0.15
		_TintTex    ("Tint", 2D) = "black" {}
		_TintColor  ("Tint Color", Color) = (1, 1, 1, 1)
		_TintAmount ("Tint Amount", Range(0, 1)) = 0.1
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		sampler2D _Global_ScreenTex, _BumpTex, _TintTex, _Global_ScreenBlurTex;
		float4 _BumpTex_ST, _TintColor;
		float _BumpScale, _TintAmount;
		struct v2f
		{
			float4 pos : POSITION;
			float2 tex : TEXCOORD0;
			float4 scrpos : TEXCOORD1;
			float3 norm : TEXCOORD2;
		};
		v2f vert (appdata_base v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.tex = TRANSFORM_TEX(v.texcoord, _BumpTex);
			o.scrpos = ComputeScreenPos(o.pos);
			o.norm = mul((float3x3)UNITY_MATRIX_IT_MV, SCALED_NORMAL);
			return o;
       	}
       	float4 frag (v2f i) : COLOR
		{
			float2 uv = i.scrpos.xy / i.scrpos.w;
			float2 bump = 2 * tex2D(_BumpTex, i.tex).xy - 1;
			float4 tint = tex2D(_TintTex, i.tex);
#if CRYSTAL_GLASS_BLUR
			float4 refrA = tex2D(_Global_ScreenBlurTex, uv + bump * _BumpScale);
			float4 refrB = tex2D(_Global_ScreenBlurTex, uv);
#else
			float4 refrA = tex2D(_Global_ScreenTex, uv + bump * _BumpScale);
			float4 refrB = tex2D(_Global_ScreenTex, uv);
#endif
			refrA = lerp(refrA, tint, _TintAmount);
			
			return refrB * refrB.a + refrA * (1 - refrB.a) * _TintColor;
		}
	ENDCG
	SubShader {
		Tags { "RenderType" = "Opaque" }
		Pass {
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ CRYSTAL_GLASS_BLUR
			ENDCG
		}
	}
	FallBack Off
}