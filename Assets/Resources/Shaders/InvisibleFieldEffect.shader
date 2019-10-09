// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "InvisibleFieldEffect"
{
	Properties
	{
		_FieldColor("FieldColor", Color) = (0.003838672,0.5220588,0.243292,0)
		_Distortion("Distortion", Range( 0 , 1)) = 0.292
		_Texture0("Texture 0", 2D) = "white" {}
		_BrushedMetalNormal("BrushedMetalNormal", 2D) = "bump" {}
		_DissolveTexture("DissolveTexture", 2D) = "white" {}
		_TimeScale("Time Scale", Float) = 0.98
		_DissolveAmount("Dissolve Amount", Range( 0 , 1)) = 0
		[HDR]_RimColor("RimColor", Color) = (0,0,0,0)
		_RimScale("RimScale", Float) = 0
		_RimPower("RimPower", Float) = 0
		_IntersectIntensity("Intersect Intensity", Range( 0 , 1)) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform sampler2D _Texture0;
		uniform float _DissolveAmount;
		uniform sampler2D _DissolveTexture;
		uniform float4 _DissolveTexture_ST;
		uniform float4 _RimColor;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _BrushedMetalNormal;
		uniform float _TimeScale;
		uniform float _Distortion;
		uniform float4 _FieldColor;
		uniform float _RimScale;
		uniform float _RimPower;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _IntersectIntensity;


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_DissolveTexture = i.uv_texcoord * _DissolveTexture_ST.xy + _DissolveTexture_ST.zw;
			float temp_output_81_0 = ( (-0.5 + (( 1.0 - _DissolveAmount ) - 0.0) * (0.9 - -0.5) / (1.0 - 0.0)) + tex2D( _DissolveTexture, uv_DissolveTexture ).r );
			float clampResult83 = clamp( (-5.0 + (temp_output_81_0 - 0.0) * (5.0 - -5.0) / (1.0 - 0.0)) , 0.0 , 1.0 );
			float temp_output_84_0 = ( 1.0 - clampResult83 );
			float2 appendResult86 = (float2(temp_output_84_0 , 0.0));
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 normalizedClip20 = ase_grabScreenPosNorm;
			float mulTime25 = _Time.y * _TimeScale;
			float cos15 = cos( mulTime25 );
			float sin15 = sin( mulTime25 );
			float2 rotator15 = mul( i.uv_texcoord - float2( 0.5,0.5 ) , float2x2( cos15 , -sin15 , sin15 , cos15 )) + float2( 0.5,0.5 );
			float4 screenColor22 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( normalizedClip20 + float4( ( UnpackNormal( tex2D( _BrushedMetalNormal, rotator15 ) ) * _Distortion ) , 0.0 ) ).xy/( normalizedClip20 + float4( ( UnpackNormal( tex2D( _BrushedMetalNormal, rotator15 ) ) * _Distortion ) , 0.0 ) ).w);
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV27 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode27 = ( 0.0 + _RimScale * pow( 1.0 - fresnelNdotV27, _RimPower ) );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth60 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth60 = abs( ( screenDepth60 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _IntersectIntensity ) );
			float clampResult64 = clamp( distanceDepth60 , 0.0 , 1.0 );
			float4 lerpResult67 = lerp( _RimColor , ( ( screenColor22 * _FieldColor ) + ( _RimColor * fresnelNode27 ) ) , clampResult64);
			o.Emission = ( ( tex2D( _Texture0, appendResult86 ) * temp_output_84_0 ) + lerpResult67 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
6.4;469.6;1524;275;4610.913;991.2927;5.70647;True;False
Node;AmplifyShaderEditor.RangedFloatNode;26;-2363.022,124.4659;Float;False;Property;_TimeScale;Time Scale;5;0;Create;True;0;0;False;0;0.98;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-2080.919,-1092.073;Float;False;Property;_DissolveAmount;Dissolve Amount;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-2193.131,-199.2768;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;12;-2193.131,-71.27679;Float;False;Constant;_Vector0;Vector 0;-1;0;Create;True;0;0;False;0;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleTimeNode;25;-2087.81,121.4249;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;78;-1785.275,-1073.021;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;80;-1610.82,-1108.424;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;79;-1920.911,-917.249;Float;True;Property;_DissolveTexture;DissolveTexture;4;0;Create;True;0;0;False;0;e28dc97a9541e3642a48c0e3886688c5;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RotatorNode;15;-1872.71,-115.8788;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-1312.023,-898.6692;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-1583.711,-179.8778;Float;True;Property;_BrushedMetalNormal;BrushedMetalNormal;3;0;Create;True;0;0;False;0;None;302951faffe230848aa0d3df7bb70faa;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;17;-1585.131,24.72321;Float;False;Property;_Distortion;Distortion;1;0;Create;True;0;0;False;0;0.292;0.008;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;16;-1562.467,-403.2462;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;82;-1122.678,-1085.044;Float;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-5;False;4;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-1169.905,-114.178;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1297.131,-391.2768;Float;False;normalizedClip;-1;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1053.008,293.453;Float;False;Property;_RimScale;RimScale;8;0;Create;True;0;0;False;0;0;0.08;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;83;-785.4869,-1043.615;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-1004.506,-237.9773;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-1038.43,383.569;Float;False;Property;_RimPower;RimPower;9;0;Create;True;0;0;False;0;0;5.94;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;85;-1661.913,-1398.836;Float;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;22;-817.1306,-289.3771;Float;False;Global;_ScreenGrab0;Screen Grab 0;-1;0;Create;True;0;0;False;0;Object;-1;False;True;1;0;FLOAT4;0,0,0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;84;-1389.914,-1278.562;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-1268.305,576.3815;Float;False;Property;_IntersectIntensity;Intersect Intensity;10;0;Create;True;0;0;False;0;0.2;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;27;-853.3347,307.0632;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-871.3061,140.131;Float;False;Property;_RimColor;RimColor;7;1;[HDR];Create;True;0;0;False;0;0,0,0,0;0,3.575151,5.340313,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-809.9133,-104.3767;Float;False;Property;_FieldColor;FieldColor;0;0;Create;True;0;0;False;0;0.003838672,0.5220588,0.243292,0;1,1,1,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;60;-876.4318,565.2746;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;86;-1423.887,-1669.477;Float;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;87;-1105.597,-1636.961;Float;True;Property;_Texture0;Texture 0;2;0;Create;True;0;0;False;0;64e7766099ad46747a07014e44d0aea1;427fdefda3c71f24899927ca878b2b6a;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-536.9134,-257.7784;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-606.259,207.718;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;64;-599.6006,523.5428;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-367.7699,-77.37729;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;88;-853.0651,-1546.285;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;-319.4954,-1105.636;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;67;-234.4916,220.4347;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;91;-2923.387,-1782.311;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;92;-1890.198,-1375.76;Float;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;False;0;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;94;-2474.442,-1690.683;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ATan2OpNode;95;-2194.672,-1712.755;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;96;-1930.133,-1665.989;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;97;-1695.637,-1663.259;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-388.2188,367.5953;Float;False;Property;_Opacity;Opacity;11;0;Create;True;0;0;False;0;0;0.844;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TauNode;90;-2133.515,-1483.436;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;119.3867,-8.187469;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;93;-2683.835,-1744.58;Float;False;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;383.4429,83.8224;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;InvisibleFieldEffect;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Translucent;0.5;True;False;0;True;Opaque;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;1;False;-1;255;False;-1;255;False;-1;6;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;26;0
WireConnection;78;0;77;0
WireConnection;80;0;78;0
WireConnection;15;0;13;0
WireConnection;15;1;12;0
WireConnection;15;2;25;0
WireConnection;81;0;80;0
WireConnection;81;1;79;1
WireConnection;18;1;15;0
WireConnection;82;0;81;0
WireConnection;19;0;18;0
WireConnection;19;1;17;0
WireConnection;20;0;16;0
WireConnection;83;0;82;0
WireConnection;21;0;20;0
WireConnection;21;1;19;0
WireConnection;22;0;21;0
WireConnection;84;0;83;0
WireConnection;27;2;31;0
WireConnection;27;3;32;0
WireConnection;60;0;59;0
WireConnection;86;0;84;0
WireConnection;86;1;85;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;29;0;30;0
WireConnection;29;1;27;0
WireConnection;64;0;60;0
WireConnection;28;0;24;0
WireConnection;28;1;29;0
WireConnection;88;0;87;0
WireConnection;88;1;86;0
WireConnection;89;0;88;0
WireConnection;89;1;84;0
WireConnection;67;0;30;0
WireConnection;67;1;28;0
WireConnection;67;2;64;0
WireConnection;94;0;93;0
WireConnection;95;0;94;1
WireConnection;95;1;94;0
WireConnection;96;0;95;0
WireConnection;96;1;90;0
WireConnection;97;0;96;0
WireConnection;97;1;92;0
WireConnection;99;0;89;0
WireConnection;99;1;67;0
WireConnection;93;0;91;0
WireConnection;0;2;99;0
WireConnection;0;10;81;0
ASEEND*/
//CHKSM=C714337486FF73DF50CF7A42B337810ED3D11133