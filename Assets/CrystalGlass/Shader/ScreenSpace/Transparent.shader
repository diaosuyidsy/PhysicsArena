Shader "Crystal Glass/Screen Space/Transparent" {
	Properties {
		_BumpMap     ("Bump", 2D) = "Bump" {}
		_Density     ("Refraction", Float) = 0.001
		_IndexR      ("Refraction R Index", Range(0.8, 1)) = 0.8
		_IndexG      ("Refraction G Index", Range(0.8, 1)) = 0.9
		_Reflection  ("Reflection", Range (0.00, 1)) = 0.5
		_FrezPow     ("Fresnel Reflection", Range(0, 1)) = 0.2
		_FrezFalloff ("Fresnal Falloff", Range(0, 10)) = 4
	}
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "Always" }
		UsePass "Hidden/Crystal Glass/Screen Space/Refraction/SSR"

		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "LightMode" = "Always" }
		UsePass "Hidden/Crystal Glass/Screen Space/Reflection/SSR"
	}
}
