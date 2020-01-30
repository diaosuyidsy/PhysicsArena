Shader "Shaders/Jiffycrew/JiffycrewPPLine"
{
	Properties{
		
		[Header(Common)]
		_MainTex("Base (RGB)", 2D) = "black" {}
		[Toggle]_AugmentSceneColor("Augment Scene Color", float) = 1
		_BGColor("Background Color", color) = (0.5,0.5,0.5,1)
		[Space]

		[Header(Silhouette)]
		[Toggle]_SILineEnable("Silhouette Line Enable", float) = 1
		_SILineThickness("Silhouette Line Width", float) = 1
		_SILineThreshold("Silhouette Line Threshold", float) = 0
		_SILineColor("Silhouette Line Color", color) = (1,0,0,1)
		[Space]

		[Header(Crease)]
		[Toggle]_CRLineEnable("Crease Line Enable", float) = 1
		_CRLineThickness("Crease Line Width", float) = 1
		_CRLineThreshold("Crease Line Threshold", float) = 0
		_CRLineColor("Crease Line Color", color) = (0,0,1,1)
		[Space]

		[Header(Suggestive Contour)]
		[Toggle]_SCLineEnable("Suggestive Contour Line Enable", float) = 1
		_SCLineThickness("Suggestive Contour Line Width", float) = 1
		_SCLineStrength("Suggestive Contour Line Strength", float) = 100
		_SCLineThreshold("Suggestive Contour Line Threshold", float) = 0
		_SCLineColor("Suggestive Contour Line Color", color) = (0,1,0,1)
		[Space]

		[Header(Suggestive Highlight)]
		[Toggle]_SHLineEnable("Suggestive Highlight Line Enable", float) = 1
		_SHLineThickness("Suggestive Highlight Line Width", float) = 1
		_SHLineStrength("Suggestive Highlight Line Strength", float) = 100
		_SHLineThreshold("Suggestive Highlight Line Threshold", float) = 0
		_SHLineColor("Suggestive Highlight Line Color", color) = (1,1,1,1)
	}

	CGINCLUDE

	#include "UnityCG.cginc"
	#include "JiffycrewPPLineCommon.cginc"

	sampler2D _MainTex;
	sampler2D _DepthNormalTexture;

	float4 _MainTex_TexelSize;

	bool _AugmentSceneColor;

	// for fast world space reconstruction
	float4x4 _FrustumCornersWS;
	float4 _CameraWS;

	float4 _BGColor;
	bool _CRLineEnable = true;
	float _CRLineThickness = 1;
	float _CRLineThreshold = 0;
	float4 _CRLineColor;

	bool _SILineEnable = true;
	float _SILineThickness = 1;
	float _SILineThreshold = 0;
	float4 _SILineColor;

	bool _SCLineEnable = true;
	float _SCLineThickness = 1;
	float _SCLineStrength = 1;
	float _SCLineThreshold = 0;
	float4 _SCLineColor;

	bool _SHLineEnable = true;
	float _SHLineThickness = 1;
	float _SHLineStrength = 1;
	float _SHLineThreshold = 0;
	float4 _SHLineColor;

	float2 _SceneTexelSize;

	float _LineIntensityOffset = 0;

	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
	};

	v2f vert(appdata_img v)
	{
		v2f o;
		half index = v.vertex.z;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;

		o.interpolatedRay = _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;

		return o;
	}

	float3 CameraRayFromUV(float2 uv, float4x4 cornerRays) {
		float3 topRay = lerp(cornerRays[0].xyz, cornerRays[1].xyz, uv.x);
		float3 bottomRay = lerp(cornerRays[3].xyz, cornerRays[2].xyz, uv.x);
		return lerp(topRay, bottomRay, 1-uv.y);
	}

	float3 CameraVecFromUV(float2 uv, float4x4 cornerRays) {
		return -normalize(CameraRayFromUV(uv, cornerRays));

	}
	
	float3 WDirUV(float3 wsPos, float3 wsNormal, float3 wsCamera) {
		wsCamera = normalize(wsCamera);
		wsNormal = normalize(wsNormal);
		return wsCamera - dot(wsNormal, wsCamera) * wsNormal;		
	}

	float2 WScreenUV(float3 wsPos, float3 wsNormal, float3 wsCamera) {
		float3 wsWDir = WDirUV(wsPos, wsNormal, wsCamera);

		float4 wsPosA = float4(wsPos, 1);
		float4 wsPosB = float4(wsPos + wsWDir,1);
				
		float4 scrPosA = UnityWorldToClipPos(wsPosA.xyz);
		float4 scrPosB = UnityWorldToClipPos(wsPosB.xyz);

		return scrPosB.xy / scrPosB.w - scrPosA.xy / scrPosB.w;
	}
	
	float Valley(float2 uv, float2 scrW, float3 wsNormal) {
		float2 uvOffset = scrW * _SCLineThickness * _SceneTexelSize;
		float2 uv1 = uv + uvOffset;
		float2 uv2 = uv - uvOffset;

		float3 wsNormal1 = GetNormal(_DepthNormalTexture, uv1);
		float3 wsNormal2 = GetNormal(_DepthNormalTexture, uv2);

		float3 wsCamera = CameraVecFromUV(uv, _FrustumCornersWS);
		float3 wsCamera1 = CameraVecFromUV(uv1, _FrustumCornersWS);
		float3 wsCamera2 = CameraVecFromUV(uv2, _FrustumCornersWS);

		float NdotV = dot(wsNormal, wsCamera);
		float NdotV1 = dot(wsNormal1, wsCamera1);
		float NdotV2 = dot(wsNormal2, wsCamera2);

		float lineVal = min(NdotV1, NdotV2) - (NdotV + _SCLineThreshold);
		return clamp(lineVal * _SCLineStrength, 0,1);
	}


	float Ridge(float2 uv, float2 scrW, float3 wsNormal) {
		float2 uvOffset = scrW * _SCLineThickness * _SceneTexelSize;
		float2 uv1 = uv + uvOffset;
		float2 uv2 = uv - uvOffset;

		float3 wsNormal1 = GetNormal(_DepthNormalTexture, uv1);
		float3 wsNormal2 = GetNormal(_DepthNormalTexture, uv2);

		float3 wsCamera = CameraVecFromUV(uv, _FrustumCornersWS);
		float3 wsCamera1 = CameraVecFromUV(uv1, _FrustumCornersWS);
		float3 wsCamera2 = CameraVecFromUV(uv2, _FrustumCornersWS);

		float NdotV = dot(wsNormal, wsCamera);
		float NdotV1 = dot(wsNormal1, wsCamera1);
		float NdotV2 = dot(wsNormal2, wsCamera2);

		float lineVal = NdotV - (max(NdotV1, NdotV2) + _SHLineThreshold);
		return clamp(lineVal * _SHLineStrength, 0, 1);
	}

	float3 PositionWSFromDepthAndUV2(float4x4 cornerRays, float3 cameraPosWS, float depth, float2 uv) {
		float3 cameraRay = CameraRayFromUV(uv, _FrustumCornersWS);
		return cameraPosWS + depth * cameraRay;
	}

	float SuggestiveContour(float2 uv) {
		float depth = GetDepth(_DepthNormalTexture, uv);
		float3 wsNormal = GetNormal(_DepthNormalTexture, uv);

		float3 cameraRay = CameraRayFromUV(uv, _FrustumCornersWS);
		float3 wsCamera = CameraVecFromUV(uv, _FrustumCornersWS);
		float3 wsPos = PositionWSFromDepthAndUV2(_FrustumCornersWS, _CameraWS.xyz, depth, uv);
		float2 wScr = WScreenUV(wsPos, wsNormal, wsCamera);
		float valley = Valley(uv, wScr, wsNormal);

		return clamp(valley, 0, 1);
	}

	float4 SuggestiveHighlight(float2 uv) {
		float depth = GetDepth(_DepthNormalTexture, uv);
		float3 wsNormal = GetNormal(_DepthNormalTexture, uv);

		float3 cameraRay = CameraRayFromUV(uv, _FrustumCornersWS);
		float3 wsCamera = CameraVecFromUV(uv, _FrustumCornersWS);
		float3 wsPos = PositionWSFromDepthAndUV2(_FrustumCornersWS, _CameraWS.xyz, depth, uv);
		float2 wScr = WScreenUV(wsPos, wsNormal, wsCamera);
		float ridge = Ridge(uv, wScr, wsNormal);

		return clamp(ridge, 0, 1);
	}

	float4 frag(v2f i) : COLOR {
		float4 sceneColor = tex2D(_MainTex, i.uv);

		float si = SilhouetteSobel(_DepthNormalTexture, _SILineThickness*0.5, i.uv, _SceneTexelSize);
		float cr = CreaseSobel(_DepthNormalTexture, _CRLineThickness, i.uv, _SceneTexelSize);
		float sc = SuggestiveContour(i.uv);
		float sh = SuggestiveHighlight(i.uv);

		si = clamp(si - _SILineThreshold*100, 0, 1);
		cr = clamp(cr - _CRLineThreshold, 0, 1);
		sc = clamp(sc, 0, 1);
		sh = clamp(sh, 0, 1);

		float3 white = float3(1, 1, 1);
		float3 black = float3(0, 0, 0);

		float3 bgColor = _AugmentSceneColor ? sceneColor.rgb : _BGColor;

		float3 siColor = _SILineEnable ? white - (si * (white - _SILineColor)) : white;
		float3 crColor = _CRLineEnable ? white - (cr * (white - _CRLineColor)) : white;
		float3 scColor = _SCLineEnable ? white - (sc * (white - _SCLineColor)) : white;
		float3 shColor = _SHLineEnable ? (sh * _SHLineColor) : black;

		float3 lineColor = bgColor * siColor * crColor * scColor + shColor;

		return float4(lineColor,1);
	}

	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		//Blend SrcAlpha OneMinusSrcAlpha

		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}

	Fallback "Diffuse"

}