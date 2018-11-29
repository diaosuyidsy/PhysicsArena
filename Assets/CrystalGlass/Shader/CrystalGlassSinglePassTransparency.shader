Shader "Crystal Glass/Single Pass Transparency" {
	Properties {
		_EnvTex        ("Environment", CUBE) = "black" {}
		_NormalTex     ("Normal", 2D) = "black" {}
		_IOR           ("Indices Of Refract", Range(-0.95, -0.8)) = -0.9
		_IOROffset     ("Indices Of Refract Offset", Range(-0.05, 0.05)) = 0.02
		_FresnelPower  ("Fresnel Power", Range(0, 8)) = 1.55
		_FresnelAlpha  ("Fresnel Alpha Intensity", Range(0, 2)) = 1
		_FresnelColor1 ("Fresnel Color 1", Color) = (1, 1, 1, 1)
		_FresnelColor2 ("Fresnel Color 2", Color) = (1, 1, 1, 1)
		_Transparency  ("Transparency", Float) = 1
		_Lod           ("Cube Lod", Float) = 0
		[Header(Sparkle)]
		_SparkleNoiseTex  ("Sparkle Noise", 2D) = "black" {}
		_SparklePower     ("Sparkle Power", Float) = 1024
		_SparkleAnimSpeed ("Sparkle Anim Speed", Float) = 0
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ CRYSTAL_GLASS_BUMP
			#pragma multi_compile _ CRYSTAL_GLASS_LOD
			#pragma multi_compile _ CRYSTAL_GLASS_SPARKLE
			#include "UnityCG.cginc"
			#include "CrystalGlass.cginc"
			ENDCG
		}
	}
	FallBack "Diffuse"
}
