// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/// <summary>
/// Off Screen Particle Rendering System
/// ©2015 Disruptor Beam
/// Written by Jason Booth (slipster216@gmail.com)
/// </summary>

Shader "Shaders/Jiffycrew/JiffycrewDepthNormal"
{
	Properties
	{
		_Color("Color",Color) = (1,1,1,1)
		_BumpMap("Normal Map", 2D) = "bump" {}
		_MainTex("Base (RGB)", 2D) = "black" {}
	}
		CGINCLUDE

#include "UnityCG.cginc"

	sampler2D _BumpMap;
	sampler2D _MainTex;
	half4 _Color;
	float4 _BumpMap_ST;
	
	struct v2f {
		float4 pos : SV_POSITION;		
		float2 uv : TEXCOORD0;
		half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
		half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
		half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z
		float depth : TEXCOORD4;
	};
		
	v2f vert( appdata_tan input ) 
	{
		v2f output;
		output.pos = UnityObjectToClipPos(input.vertex);

		output.uv = input.texcoord;

		half3 wNormal = UnityObjectToWorldNormal(input.normal);
		half3 wTangent = UnityObjectToWorldDir(input.tangent.xyz);
		// compute bitangent from cross product of normal and tangent
		half tangentSign = input.tangent.w * unity_WorldTransformParams.w;
		half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
		// output the tangent space matrix
		output.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
		output.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
		output.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

		output.depth = -UnityObjectToViewPos(input.vertex).z;

		return output;
	}
	
	half4 frag(v2f input) : SV_Target 
	{
		float4 color = tex2D(_MainTex, input.uv);
		
		half2 uv_BumpMap = TRANSFORM_TEX(input.uv, _BumpMap);
		half3 tnormal = UnpackNormal(tex2D(_BumpMap, uv_BumpMap));
		half3 worldNormal;
		worldNormal.x = dot(input.tspace0, tnormal);
		worldNormal.y = dot(input.tspace1, tnormal);
		worldNormal.z = dot(input.tspace2, tnormal);

		return float4(worldNormal,input.depth);
	}

	ENDCG
	
Subshader {
	Tags{ "RenderType" = "Opaque" }

	Zwrite On
	ZTest LEqual
		
	Pass{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
	}
}

Fallback Off
	
} // shader