// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CustomComicShader"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.085
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_ShadowStrength("ShadowStrength", Range( 0 , 1)) = 0.941
		_ShadowColor("ShadowColor", Color) = (0.7607843,0.5921569,0.8862745,1)
		_RimColor("RimColor", Color) = (0.7490196,0.9529412,1,0)
		_ShadowOffset("ShadowOffset", Range( 0 , 50)) = 16.84834
		_RimOffset("RimOffset", Range( 0 , 1)) = 0.682
		_RimPower("RimPower", Range( 0 , 1)) = 0.26
		_SpecTint("SpecTint", Color) = (1,1,1,0)
		_Gloss("Gloss", Range( 0 , 1)) = 0.079
		_SpecIntensity("SpecIntensity", Range( 0 , 1)) = 0
		_SpecTransition("SpecTransition", Range( 0 , 1)) = 0.453
		[Toggle(_USE_SCREEN_SPACE_ON)] _Use_Screen_Space("Use_Screen_Space", Float) = 1
		_LineHalfTone("Line HalfTone", 2D) = "white" {}
		_HalftonePower("Halftone Power", Float) = 2
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
		uniform sampler2D _Sampler0209;
		uniform float4 _HalftoneTexture_TexelSize;
		uniform float _HalftonePower;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform float _RimOffset;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform sampler2D _LineHalfTone;


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1);
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1);
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


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
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldlightDir );
			float NdotL9 = dotResult3;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 temp_output_210_0 = (( ase_screenPosNorm * _ScreenParams * _HalftoneTexture_TexelSize )).xy;
			#ifdef _USE_SCREEN_SPACE_ON
				float2 staticSwitch177 = ( ( 100.0 * temp_output_210_0 * -1000.0 ) / _ScreenParams.y );
			#else
				float2 staticSwitch177 = ( i.uv_texcoord * 100.0 );
			#endif
			float4 temp_cast_0 = ((-1.5 + (NdotL9 - 0.0) * (_HalftonePower - -1.5) / (1.0 - 0.0))).xxxx;
			float4 DotHalftone195 = saturate( pow( tex2D( _HalftoneTexture, staticSwitch177 ) , temp_cast_0 ) );
			float4 temp_output_298_0 = ( step( ( _ShadowOffset * 0.05 ) , NdotL9 ) + ( 1.0 - ( DotHalftone195 * ase_lightAtten ) ) );
			float4 lerpResult167 = lerp( _ShadowColor , color168 , saturate( temp_output_298_0 ));
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
			float4 color244 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 color254 = IsGammaSpace() ? float4(0.6737329,0.5859292,0.7264151,1) : float4(0.411489,0.3023697,0.4865309,1);
			float4 color255 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 temp_cast_3 = (0.11).xxxx;
			float4 temp_cast_4 = (0.76).xxxx;
			float4 break262 = CustomToonLighting99;
			float4 temp_cast_5 = ((-0.9 + (( 1.0 - ( ( 0.299 * break262.r ) + ( 0.587 * break262.g ) + ( break262.b * 0.0 ) ) ) - 0.0) * (1.0 - -0.9) / (1.0 - 0.0))).xxxx;
			float4 smoothstepResult247 = smoothstep( temp_cast_3 , temp_cast_4 , pow( tex2D( _LineHalfTone, ( ( -1000.0 * 188.06 * temp_output_210_0 ) / _ScreenParams.y ) ) , temp_cast_5 ));
			float4 lerpResult256 = lerp( color254 , color255 , smoothstepResult247);
			float4 LineHalftone231 = lerpResult256;
			float temp_output_240_0 = ( 1.2 * ase_lightAtten );
			float4 lerpResult243 = lerp( color244 , LineHalftone231 , ( 1.0 - temp_output_240_0 ));
			float4 blendOpSrc250 = CustomToonLighting99;
			float4 blendOpDest250 = lerpResult243;
			float4 color287 = IsGammaSpace() ? float4(0.4339623,0.4339623,0.4339623,1) : float4(0.1579265,0.1579265,0.1579265,1);
			float4 temp_cast_6 = (-0.1259732).xxxx;
			float4 temp_cast_7 = (( -0.1259732 + 0.5553194 )).xxxx;
			Gradient gradient280 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float ShadowMask267 = temp_output_240_0;
			float4 smoothstepResult140 = smoothstep( temp_cast_6 , temp_cast_7 , SampleGradient( gradient280, ShadowMask267 ));
			float4 Shadow161 = saturate( round( smoothstepResult140 ) );
			float temp_output_155_0 = ( -0.1259732 + 0.2630343 );
			float4 temp_cast_8 = (temp_output_155_0).xxxx;
			float4 temp_cast_9 = (( temp_output_155_0 + 0.5553194 )).xxxx;
			float4 smoothstepResult153 = smoothstep( temp_cast_8 , temp_cast_9 , SampleGradient( gradient280, ShadowMask267 ));
			float4 ShadeOutline268 = ( Shadow161 + -saturate( round( smoothstepResult153 ) ) );
			float4 lerpResult284 = lerp( color287 , color255 , ( 1.0 - ShadeOutline268 ));
			float4 ShadeOutlineColor285 = lerpResult284;
			float4 blendOpSrc270 = ( saturate( ( blendOpSrc250 * blendOpDest250 ) ));
			float4 blendOpDest270 = ShadeOutlineColor285;
			c.rgb = ( saturate( ( blendOpSrc270 * blendOpDest270 ) )).rgb;
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
6.4;180.8;1192;535;9904.898;2238.098;9.442475;True;False
Node;AmplifyShaderEditor.CommentaryNode;90;-2115.073,-201.2556;Float;False;738.8931;280;Comment;2;19;20;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;222;-9587.918,-220.578;Float;False;2760.353;1385.154;Comment;45;255;256;254;249;247;248;229;228;231;227;225;226;195;179;170;178;177;221;176;175;173;210;211;174;208;171;209;198;257;258;260;262;263;259;264;266;283;284;285;286;287;183;290;291;292;HalfTone;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;19;-2065.073,-153.1661;Float;True;Property;_NormalMap;NormalMap;2;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexelSizeNode;209;-9393.415,666.8079;Float;False;178;1;0;SAMPLER2D;_Sampler0209;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenParams;198;-9537.918,442.8544;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenPosInputsNode;171;-9485.017,224.7976;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;11;-4760.716,899.1234;Float;False;853.7776;747.1895;Dot;9;5;3;1;6;7;8;9;10;21;Dot;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1616.18,-119.7497;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-4728.924,1028.588;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-9030.715,535.5078;Float;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-4499.559,958.1584;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;5;-4511.666,1124.744;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;211;-9057.75,196.1448;Float;False;Constant;_Float3;Float 3;17;0;Create;True;0;0;False;0;-1000;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;210;-8892.322,520.0181;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-9116.714,77.09114;Float;False;Constant;_HalftoneScale;Halftone Scale;14;0;Create;True;0;0;False;0;100;10.56;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-9141.513,-151.3158;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;3;-4242.146,1050.723;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-8788.474,137.4855;Float;True;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-8760.568,-170.578;Float;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;221;-8549.566,140.5126;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-4107.128,1057.852;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;177;-8412.769,-4.258065;Float;False;Property;_Use_Screen_Space;Use_Screen_Space;13;0;Create;True;0;0;False;0;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-8076.418,374.3458;Float;False;Property;_HalftonePower;Halftone Power;15;0;Create;True;0;0;False;0;2;1.64;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;183;-8140.956,173.4107;Float;True;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;291;-7801.364,293.3291;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;178;-8104.564,-44.51962;Float;True;Property;_HalftoneTexture;Halftone Texture;16;0;Create;True;0;0;False;0;6e1b83121839a5d48b3a21c79d494184;6e1b83121839a5d48b3a21c79d494184;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;179;-7607.13,75.14452;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;292;-7317.364,131.3291;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;18;-6668.405,-80.02289;Float;False;2717.385;672.9087;Comment;23;191;194;224;186;190;192;189;168;144;167;14;27;28;148;147;91;86;293;295;294;298;299;301;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;-7194.738,55.432;Float;False;DotHalftone;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;293;-6284.417,464.2404;Float;False;195;DotHalftone;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;300;-6488.821,627.4983;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-6098.352,-15.72216;Float;False;Property;_ShadowOffset;ShadowOffset;6;0;Create;True;0;0;False;0;16.84834;19.9;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-6088.666,71.82753;Float;False;Constant;_Float5;Float 5;24;0;Create;True;0;0;False;0;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;-6010.488,584.1661;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;295;-6211.125,260.544;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;6;-4529.147,1314.78;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-4517.535,1473.992;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-5837.16,81.26122;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;294;-5937.549,485.1295;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode;189;-5950.519,238.5784;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;8;-4290.988,1364.518;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-4096.878,1385.055;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;298;-5666.474,427.7736;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;60;-3666.606,1557.014;Float;False;1935.208;548.0753;Comment;17;41;39;40;42;43;44;53;57;47;54;52;45;55;50;48;51;46;Rim;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;89;-2456.28,-732.7886;Float;False;943.9359;447.065;Comment;4;22;23;24;25;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;301;-5396.502,347.836;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;23;-2354.454,-490.7238;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;0.8679245,0.7558982,0.5526876,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;86;-5317.214,-25.75451;Float;False;Property;_ShadowStrength;ShadowStrength;3;0;Create;True;0;0;False;0;0.941;0.941;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;144;-5655.324,-50.53958;Float;False;Property;_ShadowColor;ShadowColor;4;0;Create;True;0;0;False;0;0.7607843,0.5921569,0.8862745,1;0.7411765,0.6117647,0.8784314,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-2406.28,-682.7886;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;ed430e9feb8a29d4e90be7aa48433813;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;168;-5643.168,125.4538;Float;False;Constant;_Color0;Color 0;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;-3604.606,1607.014;Float;False;Property;_RimOffset;RimOffset;7;0;Create;True;0;0;False;0;0.682;0.682;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-3616.606,1679.014;Float;False;10;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;223;-3644.065,2319.537;Float;False;2515.03;897.326;Comment;21;61;66;62;65;67;64;69;76;75;68;80;81;83;70;82;72;84;73;71;74;77;Spec;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;61;-3563.294,2369.537;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;91;-5051.632,67.08507;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-1987.09,-533.4046;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;167;-5238.094,293.754;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;62;-3594.065,2556.725;Float;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-3427.606,1617.014;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-3554.317,2719.663;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-1752.344,-498.3452;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-4768.633,86.89047;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;38;-3669.282,732.6051;Float;False;1645.083;638.9663;Comment;9;37;31;36;29;30;34;33;4;35;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;42;-3244.606,1650.014;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;65;-3289.452,2666.69;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-3216.804,2521.393;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-3107.344,2709.043;Float;False;Property;_Gloss;Gloss;10;0;Create;True;0;0;False;0;0.079;0.079;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;43;-3103.606,1649.014;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-3052.352,2565.843;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-3180.606,1752.014;Float;False;Property;_RimPower;RimPower;8;0;Create;True;0;0;False;0;0.26;0.26;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-2851.737,1772.104;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-3619.282,1009.567;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;28;-4523.239,211.4534;Float;False;25;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;148;-4525.711,104.8526;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;54;-2868.46,1875.144;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-2780.799,2821.938;Float;False;Constant;_Min;Min;10;0;Create;True;0;0;False;0;1.1;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-4373.791,93.73913;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;4;-3286.669,1112.622;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-2661.399,1812.464;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;81;-2489.955,2957.057;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.PowerNode;68;-2823.29,2568.312;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;44;-2884.606,1631.814;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;-2490.955,2777.057;Float;False;Property;_SpecTint;SpecTint;9;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.IndirectDiffuseLighting;33;-3332.684,1031.613;Float;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;83;-2571.023,3102.263;Float;False;Property;_SpecTransition;SpecTransition;12;0;Create;True;0;0;False;0;0.453;0.453;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-2780.799,2915.642;Float;False;Constant;_Max;Max;11;0;Create;True;0;0;False;0;1.12;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;70;-2542.464,2519.524;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;30;-3310.713,853.9331;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-4171.209,-0.9791069;Float;True;Diffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-3038.813,1049.31;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-2596.883,1649.339;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;-2231.955,2850.057;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;48;-2453.244,1791.485;Float;False;Property;_RimColor;RimColor;5;0;Create;True;0;0;False;0;0.7490196,0.9529412,1,0;0.7490196,0.9529411,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;50;-2457.825,1956.314;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-2120.26,2526.025;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2156.933,1869.083;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2030.45,2925.234;Float;True;Property;_SpecIntensity;SpecIntensity;11;0;Create;True;0;0;False;0;0;0.241;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-2931.722,782.6052;Float;False;14;Diffuse;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-2879.274,940.166;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;57;-2404.675,1651.312;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-1829.454,2659.348;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;73;-1830.626,2503.046;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-2207.243,1655.885;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-2644.644,957.736;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-1553.649,2649.087;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-1971.397,1730.984;Float;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-2339.064,955.2833;Float;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;115;-2187.308,131.6443;Float;False;876.5819;338.4216;Comment;6;15;58;78;59;79;99;FinalRender;1,1,1,1;0;0
Node;AmplifyShaderEditor.LightAttenuation;238;-1113.801,542.9979;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;241;-1035.657,434.6209;Float;False;Constant;_Float2;Float 2;18;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-2137.308,355.4659;Float;False;46;Rim;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-2135.925,246.4619;Float;False;37;Lighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-1369.035,2659.036;Float;False;Spec;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;-901.3577,512.4286;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1953.303,181.6443;Float;False;77;Spec;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1902.246,313.6859;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-1745.206,254.1059;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;267;-664.8633,496.5856;Float;False;ShadowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;193;-6093.374,-1100.996;Float;False;1837.188;807.4966;Comment;21;158;160;153;154;141;155;143;149;152;159;142;162;156;140;157;272;139;279;280;161;268;ShadowOutline;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-5934.662,-736.7674;Float;False;Constant;_EdgeOffset;EdgeOffset;18;0;Create;True;0;0;False;0;-0.1259732;0;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;280;-5787.976,-1056.82;Float;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.RangedFloatNode;154;-5915.131,-514.959;Float;False;Constant;_EdgeWidth;EdgeWidth;20;0;Create;True;0;0;False;0;0.2630343;0.031;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-5755.003,-947.7722;Float;False;267;ShadowMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-6016.374,-611.9413;Float;False;Constant;_EdgeBlur;EdgeBlur;18;0;Create;True;0;0;False;0;0.5553194;0.1578419;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-1595.526,268.7623;Float;False;CustomToonLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-5508.583,-608.4062;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientSampleNode;279;-5556.38,-1010.649;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;257;-8989.892,896.3445;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;-5528.356,-740.5503;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;156;-5353.013,-478.3032;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;140;-5209.02,-992.1671;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.05,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;262;-8868.484,979.9798;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;260;-8513.979,952.3801;Float;False;2;2;0;FLOAT;0.587;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-8921.461,736.7415;Float;False;Constant;_LineScale;LineScale;14;0;Create;True;0;0;False;0;188.06;10.56;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;153;-5204.814,-546.0993;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;149;-5019.823,-867.5839;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;258;-8511.137,845.8014;Float;False;2;2;0;FLOAT;0.299;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;-8526.979,1060.38;Float;False;2;2;0;FLOAT;0.114;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;259;-8309.777,938.78;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;152;-4969.151,-547.6547;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;225;-8672.619,613.6182;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;157;-4873.643,-877.8267;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;158;-4849.894,-555.0317;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-4690.301,-884.329;Float;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;227;-8447.53,580.9626;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;264;-8163.786,964.0399;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;228;-8307.189,428.4648;Float;True;Property;_LineHalfTone;Line HalfTone;14;0;Create;True;0;0;False;0;800e6de20ccc0ba45b38523ba2b7c593;800e6de20ccc0ba45b38523ba2b7c593;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NegateNode;159;-4676.146,-552.0876;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;266;-8139.542,644.9966;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.9;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-4697.857,-666.3734;Float;False;161;Shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;248;-7783.529,548.788;Float;False;Constant;_Float4;Float 4;17;0;Create;True;0;0;False;0;0.11;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;-7813.239,724.4283;Float;False;Constant;_Float6;Float 6;17;0;Create;True;0;0;False;0;0.76;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;229;-7967.52,468.1504;Float;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0.5;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;160;-4491.585,-657.18;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;254;-7598.859,607.5126;Float;False;Constant;_Color3;Color 3;5;0;Create;True;0;0;False;0;0.6737329,0.5859292,0.7264151,1;0.2008235,0.1791118,0.2169811,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;268;-4530.464,-404.3545;Float;False;ShadeOutline;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;255;-7595.436,787.4636;Float;False;Constant;_Color4;Color 4;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;247;-7586.331,381.9821;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;256;-7325.258,680.5975;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;283;-7598.619,1119.917;Float;False;268;ShadeOutline;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;286;-7327.605,1071.315;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;231;-7184.618,372.7653;Float;False;LineHalftone;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;287;-7596.512,942.9904;Float;False;Constant;_Color2;Color 2;5;0;Create;True;0;0;False;0;0.4339623,0.4339623,0.4339623,1;0.2008235,0.1791118,0.2169811,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;284;-7086.797,923.6447;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;244;-619.8036,232.758;Float;False;Constant;_Color1;Color 1;18;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;242;-468.0156,546.2818;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;-572.2147,395.8688;Float;False;231;LineHalftone;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;243;-301.513,400.034;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;117;-444.1471,74.87103;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;285;-7058.097,743.8628;Float;False;ShadeOutlineColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;114;-1333.534,-811.6532;Float;False;1428.443;770.9457;Comment;15;101;112;113;111;92;104;102;110;116;118;119;130;131;133;122;Blink;1,1,1,1;0;0
Node;AmplifyShaderEditor.BlendOpsNode;250;-94.41012,228.8761;Float;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;-194.7565,523.2889;Float;False;285;ShadeOutlineColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-1296.697,-744.3424;Float;False;Constant;_IsBlinking;IsBlinking;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;290;-7922.604,243.4863;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;102;-1119.506,-559.5315;Float;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;119;-1120.931,-454.1135;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-114.9454,-254.1939;Float;False;OutlineWidth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;104;-931.6603,-564.3922;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;118;-849.0291,-491.6782;Float;False;Global;_GrabScreen0;Grab Screen 0;16;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-6514.182,230.7499;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;270;221.246,327.664;Float;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-1284.776,-539.0876;Float;False;Constant;_BlinkSpeed;BlinkSpeed;11;0;Create;True;0;0;False;0;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-836.8585,-682.7787;Float;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;130;-342.39,-278.5941;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;299;-5518.502,476.836;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;272;-6025.783,-979.1378;Float;True;Property;_TextureSample0;Texture Sample 0;17;0;Create;True;0;0;False;0;None;1da4d78f723871e4a9ce592bf33745e8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;101;-340.77,-508.8498;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;111;-617.621,-738.8476;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-145.7792,-491.7565;Float;False;FinalRenderWithBlink;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-6572.503,152.2181;Float;False;195;DotHalftone;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-617.4937,-124.6094;Float;False;Constant;_OutlineWidth;Outline Width;11;0;Create;True;0;0;False;0;0.01;0.055;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-820.825,-305.5831;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;194;-6347.798,162.7258;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;131;-605.9423,-230.6196;Float;False;Constant;_Float1;Float 1;18;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;572.7387,71.879;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;CustomComicShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;6;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.085;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;19;0
WireConnection;208;0;171;0
WireConnection;208;1;198;0
WireConnection;208;2;209;0
WireConnection;1;0;21;0
WireConnection;210;0;208;0
WireConnection;3;0;1;0
WireConnection;3;1;5;0
WireConnection;175;0;174;0
WireConnection;175;1;210;0
WireConnection;175;2;211;0
WireConnection;176;0;173;0
WireConnection;176;1;174;0
WireConnection;221;0;175;0
WireConnection;221;1;198;2
WireConnection;9;0;3;0
WireConnection;177;1;176;0
WireConnection;177;0;221;0
WireConnection;291;0;183;0
WireConnection;291;4;170;0
WireConnection;178;1;177;0
WireConnection;179;0;178;0
WireConnection;179;1;291;0
WireConnection;292;0;179;0
WireConnection;195;0;292;0
WireConnection;303;0;293;0
WireConnection;303;1;300;0
WireConnection;6;0;21;0
WireConnection;191;0;190;0
WireConnection;191;1;192;0
WireConnection;294;0;303;0
WireConnection;189;0;191;0
WireConnection;189;1;295;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;10;0;8;0
WireConnection;298;0;189;0
WireConnection;298;1;294;0
WireConnection;301;0;298;0
WireConnection;91;0;86;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;167;0;144;0
WireConnection;167;1;168;0
WireConnection;167;2;301;0
WireConnection;40;0;41;0
WireConnection;40;1;39;0
WireConnection;25;0;24;0
WireConnection;147;0;91;0
WireConnection;147;1;167;0
WireConnection;42;0;40;0
WireConnection;65;0;66;0
WireConnection;67;0;61;0
WireConnection;67;1;62;1
WireConnection;43;0;42;0
WireConnection;64;0;67;0
WireConnection;64;1;65;0
WireConnection;148;0;147;0
WireConnection;27;0;148;0
WireConnection;27;1;28;0
WireConnection;55;0;52;0
WireConnection;55;1;54;0
WireConnection;68;0;64;0
WireConnection;68;1;69;0
WireConnection;44;0;43;0
WireConnection;44;1;45;0
WireConnection;33;0;35;0
WireConnection;70;0;68;0
WireConnection;70;1;75;0
WireConnection;70;2;76;0
WireConnection;14;0;27;0
WireConnection;34;0;33;0
WireConnection;34;1;4;0
WireConnection;53;0;44;0
WireConnection;53;1;55;0
WireConnection;82;0;80;0
WireConnection;82;1;81;0
WireConnection;82;2;83;0
WireConnection;84;0;70;0
WireConnection;84;1;82;0
WireConnection;51;0;48;0
WireConnection;51;1;50;1
WireConnection;36;0;30;0
WireConnection;36;1;34;0
WireConnection;57;0;53;0
WireConnection;71;0;84;0
WireConnection;71;1;72;0
WireConnection;47;0;57;0
WireConnection;47;1;51;0
WireConnection;31;0;29;0
WireConnection;31;1;36;0
WireConnection;74;0;73;0
WireConnection;74;1;71;0
WireConnection;46;0;47;0
WireConnection;37;0;31;0
WireConnection;77;0;74;0
WireConnection;240;0;241;0
WireConnection;240;1;238;0
WireConnection;59;0;15;0
WireConnection;59;1;58;0
WireConnection;79;0;78;0
WireConnection;79;1;59;0
WireConnection;267;0;240;0
WireConnection;99;0;79;0
WireConnection;155;0;142;0
WireConnection;155;1;154;0
WireConnection;279;0;280;0
WireConnection;279;1;139;0
WireConnection;143;0;142;0
WireConnection;143;1;141;0
WireConnection;156;0;155;0
WireConnection;156;1;141;0
WireConnection;140;0;279;0
WireConnection;140;1;142;0
WireConnection;140;2;143;0
WireConnection;262;0;257;0
WireConnection;260;1;262;1
WireConnection;153;0;279;0
WireConnection;153;1;155;0
WireConnection;153;2;156;0
WireConnection;149;0;140;0
WireConnection;258;1;262;0
WireConnection;263;0;262;2
WireConnection;259;0;258;0
WireConnection;259;1;260;0
WireConnection;259;2;263;0
WireConnection;152;0;153;0
WireConnection;225;0;211;0
WireConnection;225;1;226;0
WireConnection;225;2;210;0
WireConnection;157;0;149;0
WireConnection;158;0;152;0
WireConnection;161;0;157;0
WireConnection;227;0;225;0
WireConnection;227;1;198;2
WireConnection;264;0;259;0
WireConnection;228;1;227;0
WireConnection;159;0;158;0
WireConnection;266;0;264;0
WireConnection;229;0;228;0
WireConnection;229;1;266;0
WireConnection;160;0;162;0
WireConnection;160;1;159;0
WireConnection;268;0;160;0
WireConnection;247;0;229;0
WireConnection;247;1;248;0
WireConnection;247;2;249;0
WireConnection;256;0;254;0
WireConnection;256;1;255;0
WireConnection;256;2;247;0
WireConnection;286;0;283;0
WireConnection;231;0;256;0
WireConnection;284;0;287;0
WireConnection;284;1;255;0
WireConnection;284;2;286;0
WireConnection;242;0;240;0
WireConnection;243;0;244;0
WireConnection;243;1;234;0
WireConnection;243;2;242;0
WireConnection;285;0;284;0
WireConnection;250;0;117;0
WireConnection;250;1;243;0
WireConnection;290;0;183;0
WireConnection;102;0;110;0
WireConnection;133;0;130;0
WireConnection;104;0;102;0
WireConnection;118;0;119;0
WireConnection;186;0;194;0
WireConnection;186;1;183;0
WireConnection;270;0;250;0
WireConnection;270;1;271;0
WireConnection;130;0;111;0
WireConnection;130;2;131;0
WireConnection;130;3;122;0
WireConnection;130;4;122;0
WireConnection;299;0;298;0
WireConnection;101;0;111;0
WireConnection;101;2;118;0
WireConnection;101;3;92;0
WireConnection;101;4;92;0
WireConnection;111;0;112;0
WireConnection;111;2;104;0
WireConnection;111;3;113;0
WireConnection;111;4;113;0
WireConnection;116;0;101;0
WireConnection;194;0;224;0
WireConnection;0;13;270;0
ASEEND*/
//CHKSM=75776CD7070092DF33A4C78DA1AF5A9FA21140F7