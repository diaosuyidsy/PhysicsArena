// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CustomComicShader"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.055
		_ASEOutlineColor( "Outline Color", Color ) = (1,1,1,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_ToonRamp("Toon Ramp", 2D) = "white" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_ShadowOffset("ShadowOffset", Float) = 1
		_ShadowStrength("ShadowStrength", Range( 0 , 1)) = 0.4
		_RimColor("RimColor", Color) = (0,0,0,0)
		_RimOffset("RimOffset", Range( 0 , 1)) = 0
		_RimPower("RimPower", Range( 0 , 1)) = 0
		_SpecTint("SpecTint", Color) = (0,0,0,0)
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_SpecIntensity("SpecIntensity", Range( 0 , 1)) = 0
		_SpecTransition("SpecTransition", Range( 0 , 1)) = 0.364086
		_BlinkPower("BlinkPower", Float) = 1.3
		_BlinkSpeed("BlinkSpeed", Float) = 0
		_IsBlinking("IsBlinking", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		ZTest NotEqual
		Stencil
		{
			Ref 1
			Comp NotEqual
			Pass Keep
		}
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		uniform half4 _ASEOutlineColor;
		uniform half _ASEOutlineWidth;
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += ( v.normal * _ASEOutlineWidth );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _ASEOutlineColor.rgb;
			o.Alpha = 1;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		ZTest LEqual
		Stencil
		{
			Ref 1
			Comp NotEqual
			Pass Keep
		}
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform float _IsBlinking;
		uniform float _BlinkSpeed;
		uniform float _BlinkPower;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Gloss;
		uniform float4 _SpecTint;
		uniform float _SpecTransition;
		uniform float _SpecIntensity;
		uniform sampler2D _ToonRamp;
		uniform float _ShadowOffset;
		uniform float _ShadowStrength;
		uniform float _RimOffset;
		uniform float _RimPower;
		uniform float4 _RimColor;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float mulTime102 = _Time.y * _BlinkSpeed;
			float temp_output_97_0 = ( sin( mulTime102 ) * _BlinkPower );
			float ifLocalVar111 = 0;
			if( _IsBlinking <= 0.5 )
				ifLocalVar111 = 0.0;
			else
				ifLocalVar111 = temp_output_97_0;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 Albedo25 = ( tex2D( _Albedo, uv_Albedo ) * _Color );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 NormalMap20 = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float dotResult64 = dot( ( ase_worldViewDir + _WorldSpaceLightPos0.xyz ) , (WorldNormalVector( i , NormalMap20 )) );
			float smoothstepResult70 = smoothstep( 1.1 , 1.12 , pow( dotResult64 , _Gloss ));
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float4 lerpResult82 = lerp( _SpecTint , ase_lightColor , _SpecTransition);
			float4 Spec77 = ( ase_lightAtten * ( ( smoothstepResult70 * lerpResult82 ) * _SpecIntensity ) );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldlightDir );
			float NdotL9 = dotResult3;
			float2 temp_cast_0 = ((NdotL9*_ShadowOffset + _ShadowOffset)).xx;
			float4 Diffuse14 = ( saturate( ( tex2D( _ToonRamp, temp_cast_0 ) + (1.0 + (_ShadowStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) ) ) * Albedo25 );
			UnityGI gi33 = gi;
			float3 diffNorm33 = WorldNormalVector( i , NormalMap20 );
			gi33 = UnityGI_Base( data, 1, diffNorm33 );
			float3 indirectDiffuse33 = gi33.indirect.diffuse + diffNorm33 * 0.0001;
			float4 Lighting37 = ( Diffuse14 * ( ase_lightColor * float4( ( indirectDiffuse33 + ase_lightAtten ) , 0.0 ) ) );
			float dotResult8 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldViewDir );
			float NdotV10 = dotResult8;
			float4 Rim46 = ( saturate( ( pow( ( 1.0 - saturate( ( _RimOffset + NdotV10 ) ) ) , _RimPower ) * ( NdotL9 * ase_lightAtten ) ) ) * ( _RimColor * float4( ase_lightColor.rgb , 0.0 ) ) );
			float4 CustomToonLighting99 = ( Spec77 + ( Lighting37 + Rim46 ) );
			float4 ifLocalVar101 = 0;
			if( ifLocalVar111 <= 0.0 )
				ifLocalVar101 = CustomToonLighting99;
			else
				ifLocalVar101 = ( ( temp_output_97_0 * Albedo25 ) + CustomToonLighting99 );
			float4 FinalRenderWithBlink116 = ifLocalVar101;
			c.rgb = FinalRenderWithBlink116.rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
49.6;360;1524;545;5114.517;675.701;5.267092;True;False
Node;AmplifyShaderEditor.CommentaryNode;90;-2115.073,-201.2556;Float;False;738.8931;280;Comment;2;19;20;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;19;-2065.073,-153.1661;Float;True;Property;_NormalMap;NormalMap;3;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1616.18,-119.7497;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;11;-3523.753,-800.5223;Float;False;853.7776;747.1895;Dot;9;5;3;1;6;7;8;9;10;21;Dot;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-3491.961,-671.0579;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-3262.595,-741.4872;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;5;-3274.702,-574.9018;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;6;-3292.184,-384.866;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-3280.571,-225.6539;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;3;-3005.183,-648.9232;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;18;-3498.411,79.45742;Float;False;1177.597;388.549;Comment;11;17;12;16;13;14;27;28;85;86;88;91;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-2870.165,-641.794;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;8;-3054.025,-335.1273;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-3448.411,353.4064;Float;False;Property;_ShadowOffset;ShadowOffset;4;0;Create;True;0;0;False;0;1;0.44;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-2456.28,-732.7886;Float;False;943.9359;447.065;Comment;4;22;23;24;25;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;60;-3626.12,1131.915;Float;False;1935.208;548.0753;Comment;17;41;39;40;42;43;44;53;57;47;54;52;45;55;50;48;51;46;Rim;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-2859.914,-314.5912;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;12;-3386.704,225.5922;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-3576.12,1253.915;Float;False;10;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-3564.12,1181.915;Float;False;Property;_RimOffset;RimOffset;7;0;Create;True;0;0;False;0;0;0.838;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-3240.394,355.2768;Float;False;Property;_ShadowStrength;ShadowStrength;5;0;Create;True;0;0;False;0;0.4;0.941;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;16;-3156.502,181.2436;Float;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;22;-2406.28,-682.7886;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;030ecdaa96f03604bb15debd34b12d5e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;23;-2354.454,-490.7238;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;0.8509804,0.4588233,0.6745098,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;91;-2892.031,314.2704;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;62;-3168.966,2182.233;Float;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;66;-3129.218,2345.171;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-3387.12,1191.915;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1987.09,-533.4046;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;-2972.168,132.4575;Float;True;Property;_ToonRamp;Toon Ramp;2;0;Create;True;0;0;False;0;52e66a9243cdfed44b5e906f5910d35b;4d3414517f9e99a4a95ae23b831c0861;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;61;-3138.195,1995.045;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;38;-3586.311,557.7729;Float;False;1412.762;489.6167;Comment;9;35;33;4;34;30;29;36;31;37;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;85;-2676.849,208.9086;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1752.344,-498.3452;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;65;-2864.353,2292.198;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;42;-3204.12,1224.915;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2791.705,2146.901;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;88;-2598.133,330.0515;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;54;-2827.974,1450.045;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2682.245,2334.551;Float;False;Property;_Gloss;Gloss;10;0;Create;True;0;0;False;0;0;0.079;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-3536.311,834.7341;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;43;-3063.12,1223.915;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-2811.251,1347.005;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-2627.253,2191.351;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-3140.12,1326.915;Float;False;Property;_RimPower;RimPower;8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-2795.336,398.5444;Float;False;25;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-2355.7,2447.446;Float;False;Constant;_Min;Min;10;0;Create;True;0;0;False;0;1.1;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;44;-2844.12,1206.715;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;81;-2064.856,2582.565;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2620.913,1387.365;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;-2065.856,2402.565;Float;False;Property;_SpecTint;SpecTint;9;0;Create;True;0;0;False;0;0,0,0,0;0.8207547,0.281665,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightAttenuation;4;-3203.698,937.7897;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;68;-2398.191,2193.82;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;33;-3249.713,856.7802;Float;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-2145.924,2727.771;Float;False;Property;_SpecTransition;SpecTransition;12;0;Create;True;0;0;False;0;0.364086;0.453;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2452.899,336.51;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2355.7,2541.15;Float;False;Constant;_Max;Max;11;0;Create;True;0;0;False;0;1.12;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-2955.842,874.4774;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-2500.394,189.2815;Float;False;Diffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;48;-2412.758,1366.386;Float;False;Property;_RimColor;RimColor;6;0;Create;True;0;0;False;0;0,0,0,0;1,0.7773697,0.3349056,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;50;-2349.562,1525.19;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-2556.397,1224.24;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;70;-2117.365,2145.032;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;-1806.856,2475.565;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;30;-3227.742,679.1009;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-1695.16,2151.533;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1605.35,2550.742;Float;True;Property;_SpecIntensity;SpecIntensity;11;0;Create;True;0;0;False;0;0;0.273;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-2848.751,607.7729;Float;False;14;Diffuse;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2116.447,1443.984;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;57;-2364.189,1226.213;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2796.303,765.3336;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;73;-1405.526,2128.554;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2166.757,1230.786;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-2623.329,650.4549;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1404.354,2284.856;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-2413.55,679.2524;Float;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;114;-1315.623,-820.6085;Float;False;1428.443;770.9457;Comment;15;101;112;113;106;111;92;105;107;109;97;94;104;102;110;116;Blink;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;115;-2187.308,131.6443;Float;False;876.5819;338.4216;Comment;6;15;58;78;59;79;99;FinalRender;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-1930.912,1305.885;Float;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1128.549,2274.595;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-943.9359,2284.544;Float;False;Spec;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1265.623,-574.1292;Float;False;Property;_BlinkSpeed;BlinkSpeed;14;0;Create;True;0;0;False;0;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-2137.308,355.4659;Float;False;46;Rim;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-2135.925,246.4619;Float;False;37;Lighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleTimeNode;102;-1101.595,-568.4868;Float;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1953.303,181.6443;Float;False;77;Spec;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1902.246,313.6859;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-1745.206,254.1059;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;104;-913.7497,-573.3475;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-928.5735,-503.2858;Float;False;Property;_BlinkPower;BlinkPower;13;0;Create;True;0;0;False;0;1.3;1.8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-891.1945,-322.4591;Float;False;25;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-1595.526,268.7623;Float;False;CustomToonLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;97;-748.7658,-566.9873;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-826.0099,-770.6085;Float;False;Property;_IsBlinking;IsBlinking;15;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-941.3268,-246.4987;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;107;-622.1537,-462.6913;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-818.9479,-691.7341;Float;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;106;-497.0929,-376.0874;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;111;-599.7103,-747.8029;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-681.9411,-158.2815;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;101;-322.8593,-517.8051;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-127.8686,-500.7118;Float;False;FinalRenderWithBlink;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-255.582,224.8393;Float;False;116;FinalRenderWithBlink;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;CustomComicShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;6;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.055;1,1,1,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;19;0
WireConnection;1;0;21;0
WireConnection;6;0;21;0
WireConnection;3;0;1;0
WireConnection;3;1;5;0
WireConnection;9;0;3;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;10;0;8;0
WireConnection;16;0;12;0
WireConnection;16;1;17;0
WireConnection;16;2;17;0
WireConnection;91;0;86;0
WireConnection;40;0;41;0
WireConnection;40;1;39;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;13;1;16;0
WireConnection;85;0;13;0
WireConnection;85;1;91;0
WireConnection;25;0;24;0
WireConnection;65;0;66;0
WireConnection;42;0;40;0
WireConnection;67;0;61;0
WireConnection;67;1;62;1
WireConnection;88;0;85;0
WireConnection;43;0;42;0
WireConnection;64;0;67;0
WireConnection;64;1;65;0
WireConnection;44;0;43;0
WireConnection;44;1;45;0
WireConnection;55;0;52;0
WireConnection;55;1;54;0
WireConnection;68;0;64;0
WireConnection;68;1;69;0
WireConnection;33;0;35;0
WireConnection;27;0;88;0
WireConnection;27;1;28;0
WireConnection;34;0;33;0
WireConnection;34;1;4;0
WireConnection;14;0;27;0
WireConnection;53;0;44;0
WireConnection;53;1;55;0
WireConnection;70;0;68;0
WireConnection;70;1;75;0
WireConnection;70;2;76;0
WireConnection;82;0;80;0
WireConnection;82;1;81;0
WireConnection;82;2;83;0
WireConnection;84;0;70;0
WireConnection;84;1;82;0
WireConnection;51;0;48;0
WireConnection;51;1;50;1
WireConnection;57;0;53;0
WireConnection;36;0;30;0
WireConnection;36;1;34;0
WireConnection;47;0;57;0
WireConnection;47;1;51;0
WireConnection;31;0;29;0
WireConnection;31;1;36;0
WireConnection;71;0;84;0
WireConnection;71;1;72;0
WireConnection;37;0;31;0
WireConnection;46;0;47;0
WireConnection;74;0;73;0
WireConnection;74;1;71;0
WireConnection;77;0;74;0
WireConnection;102;0;110;0
WireConnection;59;0;15;0
WireConnection;59;1;58;0
WireConnection;79;0;78;0
WireConnection;79;1;59;0
WireConnection;104;0;102;0
WireConnection;99;0;79;0
WireConnection;97;0;104;0
WireConnection;97;1;94;0
WireConnection;107;0;97;0
WireConnection;107;1;109;0
WireConnection;106;0;107;0
WireConnection;106;1;105;0
WireConnection;111;0;112;0
WireConnection;111;2;97;0
WireConnection;111;3;113;0
WireConnection;111;4;113;0
WireConnection;101;0;111;0
WireConnection;101;2;106;0
WireConnection;101;3;92;0
WireConnection;101;4;92;0
WireConnection;116;0;101;0
WireConnection;0;13;117;0
ASEEND*/
//CHKSM=4D62D558DB398D9E1D733B5B069009457B6549FB