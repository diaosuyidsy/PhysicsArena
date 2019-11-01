// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CustomComicShader"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.055
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_ShadowStrength("ShadowStrength", Range( 0 , 1)) = 0.4
		_ShadowColor("ShadowColor", Color) = (0.7411765,0.6117647,0.8784314,1)
		_ShadowOffset("ShadowOffset", Range( 0 , 10)) = 7.3
		_RimColor("RimColor", Color) = (0,0,0,0)
		_RimOffset("RimOffset", Range( 0 , 1)) = 0
		_RimPower("RimPower", Range( 0 , 1)) = 0
		_SpecTint("SpecTint", Color) = (0,0,0,0)
		_Gloss("Gloss", Range( 0 , 1)) = 0
		_SpecIntensity("SpecIntensity", Range( 0 , 1)) = 0
		_SpecTransition("SpecTransition", Range( 0 , 1)) = 0.364086
		_HalftoneScale("Halftone Scale", Float) = 18
		_HalftonePower("Halftone Power", Float) = 1
		[Toggle(_USE_SCREEN_SPACE_ON)] _Use_Screen_Space("Use_Screen_Space", Float) = 0
		_HalftoneTexture("Halftone Texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
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
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
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
		#pragma shader_feature _USE_SCREEN_SPACE_ON
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
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
			float4 screenPos;
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

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _Gloss;
		uniform float4 _SpecTint;
		uniform float _SpecTransition;
		uniform float _SpecIntensity;
		uniform float _ShadowStrength;
		uniform float4 _ShadowColor;
		uniform float _ShadowOffset;
		uniform sampler2D _HalftoneTexture;
		uniform float _HalftoneScale;
		uniform float _HalftonePower;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
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
			float4 color168 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 temp_cast_0 = (( _ShadowOffset * 0.05 )).xxxx;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float2 appendResult172 = (float2(ase_screenPos.x , ase_screenPos.y));
			#ifdef _USE_SCREEN_SPACE_ON
				float2 staticSwitch177 = ( _HalftoneScale * appendResult172 );
			#else
				float2 staticSwitch177 = ( i.uv_texcoord * _HalftoneScale );
			#endif
			float4 temp_cast_1 = (_HalftonePower).xxxx;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldlightDir );
			float NdotL9 = dotResult3;
			float4 lerpResult167 = lerp( _ShadowColor , color168 , step( temp_cast_0 , ( ( 1.0 - pow( tex2D( _HalftoneTexture, staticSwitch177 ) , temp_cast_1 ) ) * NdotL9 ) ));
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 Albedo25 = ( tex2D( _Albedo, uv_Albedo ) * _Color );
			float4 Diffuse14 = ( saturate( ( (1.0 + (_ShadowStrength - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) + lerpResult167 ) ) * Albedo25 );
			UnityGI gi33 = gi;
			float3 diffNorm33 = WorldNormalVector( i , NormalMap20 );
			gi33 = UnityGI_Base( data, 1, diffNorm33 );
			float3 indirectDiffuse33 = gi33.indirect.diffuse + diffNorm33 * 0.0001;
			float4 Lighting37 = ( Diffuse14 * ( ase_lightColor * float4( ( indirectDiffuse33 + ase_lightAtten ) , 0.0 ) ) );
			float dotResult8 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldViewDir );
			float NdotV10 = dotResult8;
			float4 Rim46 = ( saturate( ( pow( ( 1.0 - saturate( ( _RimOffset + NdotV10 ) ) ) , _RimPower ) * ( NdotL9 * ase_lightAtten ) ) ) * ( _RimColor * float4( ase_lightColor.rgb , 0.0 ) ) );
			float4 CustomToonLighting99 = ( Spec77 + ( Lighting37 + Rim46 ) );
			c.rgb = CustomToonLighting99.rgb;
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
			Stencil
		    {
			Ref 1
			Comp NotEqual
			Pass Keep
		    }
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
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				o.screenPos = ComputeScreenPos( o.pos );
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
				surfIN.screenPos = IN.screenPos;
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
4.133333;720.8;1535;802;8743.12;1214.873;2.953764;True;False
Node;AmplifyShaderEditor.CommentaryNode;90;-2115.073,-201.2556;Float;False;738.8931;280;Comment;2;19;20;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;19;-2065.073,-153.1661;Float;True;Property;_NormalMap;NormalMap;2;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;171;-9119.916,348.4099;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1616.18,-119.7497;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;11;-3523.753,-800.5223;Float;False;853.7776;747.1895;Dot;9;5;3;1;6;7;8;9;10;21;Dot;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;172;-8866.711,377.1238;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-8942.412,-32.70355;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;174;-8917.613,195.7034;Float;False;Property;_HalftoneScale;Halftone Scale;13;0;Create;True;0;0;False;0;18;16.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-8561.468,-51.96574;Float;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-8593.929,268.7936;Float;True;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-3491.961,-671.0579;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-3262.595,-741.4872;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;5;-3274.702,-574.9018;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;177;-8213.668,114.3542;Float;False;Property;_Use_Screen_Space;Use_Screen_Space;15;0;Create;True;0;0;False;0;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;3;-3005.183,-648.9232;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;178;-7780.466,95.09265;Float;True;Property;_HalftoneTexture;Halftone Texture;16;0;Create;True;0;0;False;0;None;1defb449ad9e75b48bee9bee67bd1dd7;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;170;-7685.667,421.1539;Float;False;Property;_HalftonePower;Halftone Power;14;0;Create;True;0;0;False;0;1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-2870.165,-641.794;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;179;-7215.265,263.8544;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-3280.571,-225.6539;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;6;-3292.184,-384.866;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;183;-6774.204,539.9971;Float;True;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;194;-6708.596,376.8511;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-6237.316,181.9217;Float;False;Constant;_Float5;Float 5;24;0;Create;True;0;0;False;0;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;8;-3054.025,-335.1273;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-6252.251,92.27237;Float;False;Property;_ShadowOffset;ShadowOffset;5;0;Create;True;0;0;False;0;7.3;3.11;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-5808.81,203.3554;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-2456.28,-732.7886;Float;False;943.9359;447.065;Comment;4;22;23;24;25;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-2859.914,-314.5912;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;18;-3696.693,79.45742;Float;False;1375.879;400.7242;Comment;7;86;91;147;14;27;148;28;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;60;-3626.12,1131.915;Float;False;1935.208;548.0753;Comment;17;41;39;40;42;43;44;53;57;47;54;52;45;55;50;48;51;46;Rim;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-6220.966,315.6368;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-3687.008,133.7258;Float;False;Property;_ShadowStrength;ShadowStrength;3;0;Create;True;0;0;False;0;0.4;0.95;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;144;-5451.198,121.7605;Float;False;Property;_ShadowColor;ShadowColor;4;0;Create;True;0;0;False;0;0.7411765,0.6117647,0.8784314,1;0.7590013,0.5939835,0.8867924,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;189;-5869.913,372.0824;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;23;-2354.454,-490.7238;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;0.8018868,0.6312547,0.3252937,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;-3564.12,1181.915;Float;False;Property;_RimOffset;RimOffset;7;0;Create;True;0;0;False;0;0;0.838;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;168;-5442.83,258.3789;Float;False;Constant;_Color0;Color 0;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-2406.28,-682.7886;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;ed430e9feb8a29d4e90be7aa48433813;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;39;-3576.12,1253.915;Float;False;10;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;61;-3138.195,1995.045;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-3387.12,1191.915;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1987.09,-533.4046;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;167;-4656.152,309.2627;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;91;-3421.426,226.5654;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-3129.218,2345.171;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;62;-3168.966,2182.233;Float;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2791.705,2146.901;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;38;-3586.311,557.7729;Float;False;1412.762;489.6167;Comment;9;35;33;4;34;30;29;36;31;37;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;65;-2864.353,2292.198;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1752.344,-498.3452;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;42;-3204.12,1224.915;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-3138.427,246.3708;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;54;-2827.974,1450.045;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-3140.12,1326.915;Float;False;Property;_RimPower;RimPower;8;0;Create;True;0;0;False;0;0;0.92;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;148;-2895.505,264.3329;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-2627.253,2191.351;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2682.245,2334.551;Float;False;Property;_Gloss;Gloss;10;0;Create;True;0;0;False;0;0;0.079;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;43;-3063.12,1223.915;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-2893.033,370.9337;Float;False;25;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-2811.251,1347.005;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-3536.311,834.7341;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-2355.7,2447.446;Float;False;Constant;_Min;Min;10;0;Create;True;0;0;False;0;1.1;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;33;-3249.713,856.7802;Float;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2355.7,2541.15;Float;False;Constant;_Max;Max;11;0;Create;True;0;0;False;0;1.12;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;44;-2844.12,1206.715;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;4;-3203.698,937.7897;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-2737.205,270.2331;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-2145.924,2727.771;Float;False;Property;_SpecTransition;SpecTransition;12;0;Create;True;0;0;False;0;0.364086;0.453;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;68;-2398.191,2193.82;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2620.913,1387.365;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;-2065.856,2402.565;Float;False;Property;_SpecTint;SpecTint;9;0;Create;True;0;0;False;0;0,0,0,0;0.8207547,0.2816643,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;81;-2064.856,2582.565;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SmoothstepOpNode;70;-2117.365,2145.032;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;50;-2349.562,1525.19;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightColorNode;30;-3227.742,679.1009;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-2955.842,874.4774;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;48;-2412.758,1366.386;Float;False;Property;_RimColor;RimColor;6;0;Create;True;0;0;False;0;0,0,0,0;1,0.7773697,0.3349048,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-2541.003,158.5012;Float;True;Diffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-2556.397,1224.24;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;-1806.856,2475.565;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-1695.16,2151.533;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2116.447,1443.984;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1605.35,2550.742;Float;True;Property;_SpecIntensity;SpecIntensity;11;0;Create;True;0;0;False;0;0;0.273;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-2848.751,607.7729;Float;False;14;Diffuse;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;57;-2364.189,1226.213;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2796.303,765.3336;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1404.354,2284.856;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-2623.329,650.4549;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;73;-1405.526,2128.554;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2166.757,1230.786;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1128.549,2274.595;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-1930.912,1305.885;Float;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;115;-2187.308,131.6443;Float;False;876.5819;338.4216;Comment;6;15;58;78;59;79;99;FinalRender;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-2413.55,679.2524;Float;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-2137.308,355.4659;Float;False;46;Rim;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-943.9359,2284.544;Float;False;Spec;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-2135.925,246.4619;Float;False;37;Lighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1902.246,313.6859;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1953.303,181.6443;Float;False;77;Spec;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-1745.206,254.1059;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;193;-6093.374,-1100.996;Float;False;1837.188;807.4966;Comment;17;158;160;153;154;141;155;143;149;152;159;161;142;139;162;156;140;157;ShadowOutline;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;114;-1333.534,-811.6532;Float;False;1428.443;770.9457;Comment;15;101;112;113;111;92;104;102;110;116;118;119;130;131;133;122;Blink;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-1595.526,268.7623;Float;False;CustomToonLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;160;-4491.585,-657.18;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;152;-5014.497,-572.8467;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;159;-4676.146,-552.0876;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-114.9454,-254.1939;Float;False;OutlineWidth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-4632.165,-881.879;Float;False;Shadow;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-5508.583,-608.4062;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;153;-5204.814,-546.0993;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-6043.374,-613.9413;Float;False;Constant;_EdgeBlur;EdgeBlur;18;0;Create;True;0;0;False;0;0.3195542;0.1578419;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-617.4937,-124.6094;Float;False;Constant;_OutlineWidth;Outline Width;11;0;Create;True;0;0;False;0;0.01;0.055;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;102;-1119.506,-559.5315;Float;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-605.9423,-230.6196;Float;False;Constant;_Float1;Float 1;18;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;156;-5353.013,-478.3032;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-1296.697,-744.3424;Float;False;Constant;_IsBlinking;IsBlinking;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;140;-5248.962,-899.2352;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-5623.28,-906.0644;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;101;-340.77,-508.8498;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;149;-5021.907,-1050.996;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-253.3228,82.50399;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1284.776,-539.0876;Float;False;Constant;_BlinkSpeed;BlinkSpeed;11;0;Create;True;0;0;False;0;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-836.8585,-682.7787;Float;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;104;-931.6603,-564.3922;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;158;-4849.894,-555.0317;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;130;-342.39,-278.5941;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-820.825,-305.5831;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;157;-4873.643,-877.8267;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-145.7792,-491.7565;Float;False;FinalRenderWithBlink;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-5934.662,-736.7674;Float;False;Constant;_EdgeOffset;EdgeOffset;18;0;Create;True;0;0;False;0;0;0;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;-5528.356,-740.5503;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;119;-1120.931,-454.1135;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;111;-617.621,-738.8476;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-4730.208,-653.1165;Float;False;161;Shadow;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;118;-849.0291,-491.6782;Float;False;Global;_GrabScreen0;Grab Screen 0;16;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;154;-5915.131,-514.959;Float;False;Constant;_EdgeWidth;EdgeWidth;20;0;Create;True;0;0;False;0;0.05;0.031;0;0.2;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;240.7045,56.61306;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;CustomComicShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;6;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.055;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;19;0
WireConnection;172;0;171;1
WireConnection;172;1;171;2
WireConnection;176;0;173;0
WireConnection;176;1;174;0
WireConnection;175;0;174;0
WireConnection;175;1;172;0
WireConnection;1;0;21;0
WireConnection;177;1;176;0
WireConnection;177;0;175;0
WireConnection;3;0;1;0
WireConnection;3;1;5;0
WireConnection;178;1;177;0
WireConnection;9;0;3;0
WireConnection;179;0;178;0
WireConnection;179;1;170;0
WireConnection;6;0;21;0
WireConnection;194;0;179;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;191;0;190;0
WireConnection;191;1;192;0
WireConnection;10;0;8;0
WireConnection;186;0;194;0
WireConnection;186;1;183;0
WireConnection;189;0;191;0
WireConnection;189;1;186;0
WireConnection;40;0;41;0
WireConnection;40;1;39;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;167;0;144;0
WireConnection;167;1;168;0
WireConnection;167;2;189;0
WireConnection;91;0;86;0
WireConnection;67;0;61;0
WireConnection;67;1;62;1
WireConnection;65;0;66;0
WireConnection;25;0;24;0
WireConnection;42;0;40;0
WireConnection;147;0;91;0
WireConnection;147;1;167;0
WireConnection;148;0;147;0
WireConnection;64;0;67;0
WireConnection;64;1;65;0
WireConnection;43;0;42;0
WireConnection;33;0;35;0
WireConnection;44;0;43;0
WireConnection;44;1;45;0
WireConnection;27;0;148;0
WireConnection;27;1;28;0
WireConnection;68;0;64;0
WireConnection;68;1;69;0
WireConnection;55;0;52;0
WireConnection;55;1;54;0
WireConnection;70;0;68;0
WireConnection;70;1;75;0
WireConnection;70;2;76;0
WireConnection;34;0;33;0
WireConnection;34;1;4;0
WireConnection;14;0;27;0
WireConnection;53;0;44;0
WireConnection;53;1;55;0
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
WireConnection;71;0;84;0
WireConnection;71;1;72;0
WireConnection;31;0;29;0
WireConnection;31;1;36;0
WireConnection;47;0;57;0
WireConnection;47;1;51;0
WireConnection;74;0;73;0
WireConnection;74;1;71;0
WireConnection;46;0;47;0
WireConnection;37;0;31;0
WireConnection;77;0;74;0
WireConnection;59;0;15;0
WireConnection;59;1;58;0
WireConnection;79;0;78;0
WireConnection;79;1;59;0
WireConnection;99;0;79;0
WireConnection;160;0;162;0
WireConnection;160;1;159;0
WireConnection;152;0;153;0
WireConnection;159;0;158;0
WireConnection;133;0;130;0
WireConnection;161;0;157;0
WireConnection;155;0;142;0
WireConnection;155;1;154;0
WireConnection;153;0;139;0
WireConnection;153;1;155;0
WireConnection;153;2;156;0
WireConnection;102;0;110;0
WireConnection;156;0;155;0
WireConnection;156;1;141;0
WireConnection;140;0;139;0
WireConnection;140;1;142;0
WireConnection;140;2;143;0
WireConnection;101;0;111;0
WireConnection;101;2;118;0
WireConnection;101;3;92;0
WireConnection;101;4;92;0
WireConnection;149;0;140;0
WireConnection;104;0;102;0
WireConnection;158;0;152;0
WireConnection;130;0;111;0
WireConnection;130;2;131;0
WireConnection;130;3;122;0
WireConnection;130;4;122;0
WireConnection;157;0;149;0
WireConnection;116;0;101;0
WireConnection;143;0;142;0
WireConnection;143;1;141;0
WireConnection;111;0;112;0
WireConnection;111;2;104;0
WireConnection;111;3;113;0
WireConnection;111;4;113;0
WireConnection;118;0;119;0
WireConnection;0;13;117;0
ASEEND*/
//CHKSM=86534BD12823292A708D8F79F3502E2305F75CFD