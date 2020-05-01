// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CustomComicShader"
{
	Properties
	{
		_ASEOutlineWidth( "Outline Width", Float ) = 0.0006
		_ASEOutlineColor( "Outline Color", Color ) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_NormalMap("NormalMap", 2D) = "bump" {}
		_RimColor("RimColor", Color) = (0.7490196,0.9529412,1,0)
		_RimOffset("RimOffset", Range( 0 , 1)) = 0.682
		_RimPower("RimPower", Range( 0 , 1)) = 0.26
		_SpecTint("SpecTint", Color) = (1,1,1,0)
		_Gloss("Gloss", Range( 0 , 1)) = 0.079
		_ShadowOffset("ShadowOffset", Range( 0 , 50)) = 8
		_SpecIntensity("SpecIntensity", Range( 0 , 1)) = 0
		_SpecTransition("SpecTransition", Range( 0 , 1)) = 0.453
		_HalftoneRangeMin("Halftone Range Min", Float) = 0.3
		_HalftoneRangeMax("Halftone Range Max", Float) = 2
		_ShadeEdgeOffset("ShadeEdgeOffset", Range( -1 , 5)) = 0
		_ShadeEdgeBlur("ShadeEdgeBlur", Range( 0 , 2)) = 0.6320428
		_ShadeEdgeWidth("ShadeEdgeWidth", Range( 0 , 1)) = 0.03294117
		_Halftone1("Halftone1", 2D) = "black" {}
		_LineHalfTone("Line HalfTone", 2D) = "black" {}
		_HalftoneShadowTiling("HalftoneShadowTiling", Float) = 8
		_LineHalfToneColor("Line HalfTone Color", Color) = (0.6737329,0.5859292,0.7264151,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
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
		Cull Off
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_TEXTURE_PARAMS(textureName) textureName

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
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _Color;
		uniform float _ShadowOffset;
		uniform sampler2D _Halftone1;
		uniform float _HalftoneRangeMin;
		uniform float _HalftoneRangeMax;
		uniform float _ShadeEdgeOffset;
		uniform float _ShadeEdgeBlur;
		uniform float _ShadeEdgeWidth;
		uniform float _RimOffset;
		uniform float _RimPower;
		uniform float4 _RimColor;
		uniform float4 _LineHalfToneColor;
		uniform sampler2D _LineHalfTone;
		uniform float _HalftoneShadowTiling;


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


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
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode22 = tex2D( _Albedo, uv_Albedo );
			float4 Albedo25 = ( tex2DNode22 * _Color );
			float4 color144 = IsGammaSpace() ? float4(0.4352941,0.4352941,0.4352941,1) : float4(0.1589608,0.1589608,0.1589608,1);
			float4 color168 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult3 = dot( (WorldNormalVector( i , NormalMap20 )) , ase_worldlightDir );
			float NdotL9 = dotResult3;
			float4 temp_cast_0 = (0.18).xxxx;
			float4 temp_cast_1 = (0.37).xxxx;
			float2 appendResult359 = (float2(4.0 , 4.0));
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar353 = TriplanarSamplingSF( _Halftone1, ase_worldPos, ase_worldNormal, 1.0, appendResult359, 1.0, 0 );
			float4 temp_cast_2 = ((_HalftoneRangeMin + (( 1.0 - NdotL9 ) - 0.0) * (_HalftoneRangeMax - _HalftoneRangeMin) / (1.0 - 0.0))).xxxx;
			float4 smoothstepResult338 = smoothstep( temp_cast_0 , temp_cast_1 , pow( triplanar353 , temp_cast_2 ));
			float4 DotHalftone195 = saturate( smoothstepResult338 );
			float temp_output_240_0 = ( 1.2 * ase_lightAtten );
			float ShadowMask267 = temp_output_240_0;
			float4 lerpResult167 = lerp( color144 , color168 , saturate( ( step( ( _ShadowOffset * 0.05 ) , NdotL9 ) + ( 1.0 - ( DotHalftone195 * ShadowMask267 ) ) ) ));
			float4 temp_output_148_0 = saturate( ( (1.0 + (1.0 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) + lerpResult167 ) );
			float4 blendOpSrc308 = Albedo25;
			float4 blendOpDest308 = temp_output_148_0;
			float4 color333 = IsGammaSpace() ? float4(0.4352941,0.4352941,0.4352941,1) : float4(0.1589608,0.1589608,0.1589608,1);
			float4 color334 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			Gradient gradient341 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float smoothstepResult319 = smoothstep( _ShadeEdgeOffset , ( _ShadeEdgeOffset + _ShadeEdgeBlur ) , SampleGradient( gradient341, NdotL9 ));
			float temp_output_317_0 = ( _ShadeEdgeOffset + _ShadeEdgeWidth );
			float smoothstepResult321 = smoothstep( temp_output_317_0 , ( temp_output_317_0 + _ShadeEdgeBlur ) , SampleGradient( gradient341, NdotL9 ));
			float4 lerpResult332 = lerp( color333 , color334 , ( 1.0 - ( saturate( round( smoothstepResult319 ) ) + -saturate( round( smoothstepResult321 ) ) ) ));
			float4 ShadeOutline329 = lerpResult332;
			float4 Diffuse14 = ( ( saturate( ( blendOpSrc308 * blendOpDest308 ) )) * ShadeOutline329 );
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
			float4 color255 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float4 temp_cast_6 = (0.11).xxxx;
			float4 temp_cast_7 = (0.76).xxxx;
			float2 appendResult361 = (float2(_HalftoneShadowTiling , _HalftoneShadowTiling));
			float4 triplanar363 = TriplanarSamplingSF( _LineHalfTone, ase_worldPos, ase_worldNormal, 4.0, appendResult361, 1.0, 0 );
			float4 break262 = CustomToonLighting99;
			float4 temp_cast_8 = ((-0.2 + (( 1.0 - ( ( 0.299 * break262.r ) + ( 0.587 * break262.g ) + ( break262.b * 0.0 ) ) ) - 0.0) * (1.0 - -0.2) / (1.0 - 0.0))).xxxx;
			float4 smoothstepResult247 = smoothstep( temp_cast_6 , temp_cast_7 , pow( triplanar363 , temp_cast_8 ));
			float4 lerpResult256 = lerp( _LineHalfToneColor , color255 , smoothstepResult247);
			float4 LineHalftone231 = lerpResult256;
			float4 lerpResult243 = lerp( color244 , LineHalftone231 , ( 1.0 - temp_output_240_0 ));
			float4 blendOpSrc250 = CustomToonLighting99;
			float4 blendOpDest250 = lerpResult243;
			float4 color287 = IsGammaSpace() ? float4(0.4339623,0.4339623,0.4339623,1) : float4(0.1579265,0.1579265,0.1579265,1);
			float4 temp_cast_10 = (-0.1259732).xxxx;
			float4 temp_cast_11 = (( -0.1259732 + 0.5553194 )).xxxx;
			Gradient gradient280 = NewGradient( 0, 2, 2, float4( 0, 0, 0, 0 ), float4( 1, 1, 1, 1 ), 0, 0, 0, 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float4 smoothstepResult140 = smoothstep( temp_cast_10 , temp_cast_11 , SampleGradient( gradient280, ShadowMask267 ));
			float4 Shadow161 = saturate( round( smoothstepResult140 ) );
			float temp_output_155_0 = ( -0.1259732 + 0.26 );
			float4 temp_cast_12 = (temp_output_155_0).xxxx;
			float4 temp_cast_13 = (( temp_output_155_0 + 0.5553194 )).xxxx;
			float4 smoothstepResult153 = smoothstep( temp_cast_12 , temp_cast_13 , SampleGradient( gradient280, ShadowMask267 ));
			float4 ShadowOutline268 = ( Shadow161 + -saturate( round( smoothstepResult153 ) ) );
			float4 lerpResult284 = lerp( color287 , color255 , ( 1.0 - ShadowOutline268 ));
			float4 ShadowOutlineColor285 = lerpResult284;
			float4 blendOpSrc270 = ( saturate( ( blendOpSrc250 * blendOpDest250 ) ));
			float4 blendOpDest270 = ShadowOutlineColor285;
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
169.6;1106.4;1524;796;8584.998;49.62296;1.578698;True;False
Node;AmplifyShaderEditor.CommentaryNode;90;-542.4563,2549.011;Float;False;738.8931;280;Comment;2;19;20;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;19;-492.4563,2597.101;Float;True;Property;_NormalMap;NormalMap;2;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-43.56342,2630.517;Float;False;NormalMap;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;11;-3759.035,1582.899;Float;False;853.7776;747.1895;Dot;9;5;3;1;6;7;8;9;10;21;Dot;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-3727.243,1712.363;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;5;-3509.986,1808.519;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;1;-3497.878,1641.933;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;3;-3240.465,1734.498;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;222;-9587.918,-220.578;Float;False;2760.353;1385.154;Comment;39;255;256;254;249;247;248;229;231;195;179;170;257;258;260;262;263;259;264;266;283;284;285;286;287;291;292;307;338;339;224;194;360;361;362;363;357;359;354;353;HalfTone;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;9;-3105.447,1741.627;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-8444.204,61.5313;Float;True;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;357;-9268.868,89.98046;Float;False;Constant;_DotHalftoneTiling;DotHalftoneTiling;22;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;194;-8237.044,62.8357;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;354;-9130.373,-118.7278;Float;True;Property;_Halftone1;Halftone1;20;0;Create;True;0;0;False;0;6e1b83121839a5d48b3a21c79d494184;None;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;359;-9031.868,100.9805;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;170;-8446.283,338.4122;Float;False;Property;_HalftoneRangeMax;Halftone Range Max;13;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;307;-8453.764,261.166;Float;False;Property;_HalftoneRangeMin;Halftone Range Min;12;0;Create;True;0;0;False;0;0.3;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;353;-8855.225,-13.73192;Float;True;Spherical;World;False;Top Texture 0;_TopTexture0;white;-1;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;False;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;291;-8009.885,165.1327;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.3;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;340;-7534.486,312.2539;Float;False;Constant;_Float9;Float 9;15;0;Create;True;0;0;False;0;0.37;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;179;-7648.73,-9.355478;Float;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;339;-7549.383,230.894;Float;False;Constant;_Float8;Float 8;15;0;Create;True;0;0;False;0;0.18;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;309;-5711.505,-1040.066;Float;False;2586.176;919.6798;Comment;20;328;326;322;320;324;319;323;316;321;318;312;310;317;313;314;333;334;335;341;342;ShadeOutline;1,1,1,1;0;0
Node;AmplifyShaderEditor.LightAttenuation;238;-1113.801,542.9979;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;314;-5570.991,-643.0287;Float;False;Property;_ShadeEdgeOffset;ShadeEdgeOffset;15;0;Create;True;0;0;False;0;0;-0.04;-1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;338;-7347.805,99.89011;Float;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;313;-5543.262,-450.0292;Float;False;Property;_ShadeEdgeWidth;ShadeEdgeWidth;17;0;Create;True;0;0;False;0;0.03294117;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;241;-1035.657,434.6209;Float;False;Constant;_Float2;Float 2;18;0;Create;True;0;0;False;0;1.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;240;-901.3577,512.4286;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;292;-7105.364,127.3291;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;310;-5634.505,-551.0118;Float;False;Property;_ShadeEdgeBlur;ShadeEdgeBlur;16;0;Create;True;0;0;False;0;0.6320428;0.6320428;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;-5373.135,-886.8422;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;341;-5413.908,-996.6473;Float;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;317;-5126.714,-547.4766;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;18;-6668.405,-80.02289;Float;False;3308.871;964.739;Comment;24;14;308;148;28;147;91;167;301;144;168;86;298;294;189;295;191;303;192;293;331;336;337;190;344;Shade;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;318;-4971.146,-417.3732;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;-7076.738,40.432;Float;False;DotHalftone;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GradientSampleNode;342;-5182.312,-950.4763;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;267;-664.8633,496.5856;Float;False;ShadowMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;321;-4822.945,-485.1696;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;293;-6467.558,501.1225;Float;False;195;DotHalftone;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;316;-5146.488,-679.6207;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;190;-6128.013,-7.911599;Float;False;Property;_ShadowOffset;ShadowOffset;8;0;Create;True;0;0;False;0;8;8;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-6088.666,71.82753;Float;False;Constant;_Float5;Float 5;24;0;Create;True;0;0;False;0;0.05;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;344;-6467.631,590.7577;Float;False;267;ShadowMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;295;-6211.125,260.544;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;319;-4827.151,-931.2372;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;323;-4587.284,-486.7249;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;303;-6145.3,537.1099;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;191;-5837.16,81.26122;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;189;-5950.519,238.5784;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;324;-4468.027,-494.1021;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;320;-4555.172,-830.3069;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;294;-5937.549,485.1295;Float;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldNormalVector;6;-3527.466,1998.555;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-3515.855,2157.767;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;298;-5666.474,427.7736;Float;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SaturateNode;322;-4456.297,-827.0338;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;326;-4294.28,-491.1579;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;89;-4763.249,1594.764;Float;False;943.9359;447.065;Comment;6;22;23;24;25;346;347;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;8;-3289.307,2048.293;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;60;-2692.727,2496.563;Float;False;1935.208;548.0753;Comment;17;41;39;40;42;43;44;53;57;47;54;52;45;55;50;48;51;46;Rim;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;144;-5655.324,-50.53958;Float;False;Constant;_ShadowColor;ShadowColor;3;0;Create;True;0;0;False;0;0.4352941,0.4352941,0.4352941,1;0.7411765,0.6117647,0.8784314,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;22;-4742.249,1623.764;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;750a38cad8e5716409f4d0e2830a7b03;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;301;-5396.502,347.836;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;328;-4109.719,-596.2507;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;168;-5643.168,125.4538;Float;False;Constant;_Color0;Color 0;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;10;-3095.197,2068.83;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-5317.214,-25.75451;Float;False;Constant;_ShadowStrength;ShadowStrength;3;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;23;-4402.517,1874.331;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;223;-2685.222,3245.56;Float;False;2515.03;897.326;Comment;21;61;66;62;65;67;64;69;76;75;68;80;81;83;70;82;72;84;73;71;74;77;Spec;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;167;-5238.094,293.754;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;91;-5226.924,91.04569;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;335;-3558.972,-360.2718;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2630.727,2546.563;Float;False;Property;_RimOffset;RimOffset;4;0;Create;True;0;0;False;0;0.682;0.682;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;334;-3830.972,-648.2719;Float;False;Constant;_Color6;Color 6;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-4141.703,1771.182;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;333;-3830.972,-488.2718;Float;False;Constant;_Color5;Color 5;5;0;Create;True;0;0;False;0;0.4352941,0.4352941,0.4352941,1;0.2008235,0.1791118,0.2169811,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;39;-2642.727,2618.563;Float;False;10;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;147;-4848.208,-48.27898;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;332;-3318.972,-504.2718;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;-2453.727,2556.563;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;66;-2595.474,3645.686;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightPos;62;-2635.222,3482.748;Float;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-4024.869,1895.101;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;61;-2604.451,3295.56;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;148;-4895.361,335.0611;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;329;-2983.968,-526.7447;Float;False;ShadeOutline;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;-2257.962,3447.416;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;38;-2689.843,1599.87;Float;False;1645.083;638.9663;Comment;9;37;31;36;29;30;34;33;4;35;Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;42;-2270.728,2589.563;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;65;-2330.609,3592.712;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;28;-4566.741,275.362;Float;False;25;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;43;-2129.728,2588.563;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;54;-1894.581,2814.693;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;52;-1877.858,2711.653;Float;False;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2148.502,3635.065;Float;False;Property;_Gloss;Gloss;7;0;Create;True;0;0;False;0;0.079;0.079;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-2206.728,2691.563;Float;False;Property;_RimPower;RimPower;5;0;Create;True;0;0;False;0;0.26;0.26;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;308;-4351.541,352.2624;Float;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;64;-2093.51,3491.865;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;-2639.843,1876.832;Float;False;20;NormalMap;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;331;-4208.877,513.2101;Float;False;329;ShadeOutline;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;80;-1532.112,3703.079;Float;False;Property;_SpecTint;SpecTint;6;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;76;-1821.956,3841.665;Float;False;Constant;_Max;Max;11;0;Create;True;0;0;False;0;1.12;1.12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;4;-2307.23,1979.887;Float;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;33;-2353.245,1898.878;Float;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;68;-1864.447,3494.334;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;44;-1910.727,2571.363;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;343;-3969.299,303.0036;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;81;-1531.112,3883.08;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;83;-1612.18,4028.287;Float;False;Property;_SpecTransition;SpecTransition;10;0;Create;True;0;0;False;0;0.453;0.453;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-1687.52,2752.013;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-1821.956,3747.96;Float;False;Constant;_Min;Min;10;0;Create;True;0;0;False;0;1.1;1.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;50;-1483.946,2895.863;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightColorNode;30;-2331.274,1721.198;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;48;-1479.365,2731.034;Float;False;Property;_RimColor;RimColor;3;0;Create;True;0;0;False;0;0.7490196,0.9529412,1,0;0.7490196,0.9529411,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;14;-3683.176,196.8282;Float;True;Diffuse;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;70;-1583.621,3445.546;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-2059.374,1916.575;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-1623.004,2588.888;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;-1273.112,3776.079;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-1899.835,1807.432;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-1183.054,2808.632;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-1161.417,3452.047;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-1952.283,1649.87;Float;False;14;Diffuse;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;57;-1430.796,2590.861;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1071.607,3851.256;Float;True;Property;_SpecIntensity;SpecIntensity;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-870.6113,3585.37;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1665.205,1825.001;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-1233.364,2595.434;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;73;-871.7832,3429.068;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-1359.625,1822.549;Float;False;Lighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-594.8064,3575.109;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;115;-2187.308,131.6443;Float;False;876.5819;338.4216;Comment;6;15;58;78;59;79;99;FinalRender;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;46;-997.5182,2670.533;Float;False;Rim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-410.1924,3585.058;Float;False;Spec;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-2135.925,246.4619;Float;False;37;Lighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;-2137.308,355.4659;Float;False;46;Rim;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;59;-1902.246,313.6859;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;78;-1953.303,181.6443;Float;False;77;Spec;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;193;-1632.957,-1188.455;Float;False;1837.188;807.4966;Comment;20;158;160;153;154;141;155;143;149;152;159;142;162;156;140;157;139;279;280;161;268;ShadowOutline;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-1745.206,254.1059;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;154;-1454.714,-602.4184;Float;False;Constant;_EdgeWidth;EdgeWidth;20;0;Create;True;0;0;False;0;0.26;0.031;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-1555.957,-699.4008;Float;False;Constant;_EdgeBlur;EdgeBlur;18;0;Create;True;0;0;False;0;0.5553194;0.1578419;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;142;-1474.245,-824.2268;Float;False;Constant;_EdgeOffset;EdgeOffset;18;0;Create;True;0;0;False;0;-0.1259732;0;-1;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;280;-1327.559,-1144.279;Float;False;0;2;2;0,0,0,0;1,1,1,1;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;99;-1595.526,268.7623;Float;False;CustomToonLighting;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-1294.586,-1035.231;Float;False;267;ShadowMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-1048.166,-695.8656;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;143;-1067.939,-828.0096;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;257;-9530.765,970.8745;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GradientSampleNode;279;-1095.963,-1098.108;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;262;-9172.357,965.5098;Float;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;156;-892.5961,-565.7625;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;140;-748.6025,-1079.626;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0.05,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;263;-8830.852,1045.91;Float;False;2;2;0;FLOAT;0.114;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;260;-8817.852,937.9102;Float;False;2;2;0;FLOAT;0.587;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;258;-8815.01,831.3314;Float;False;2;2;0;FLOAT;0.299;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;153;-744.3964,-633.5587;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;149;-559.4059,-955.0432;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;259;-8613.65,924.3101;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;360;-9300.738,661.9172;Float;False;Property;_HalftoneShadowTiling;HalftoneShadowTiling;22;0;Create;True;0;0;False;0;8;6.37;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;157;-413.2258,-965.2859;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RoundOpNode;152;-508.7336,-635.1141;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;361;-9029.738,649.9172;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;158;-389.4768,-642.4911;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TexturePropertyNode;362;-9084.243,452.2091;Float;True;Property;_LineHalfTone;Line HalfTone;21;0;Create;True;0;0;False;0;800e6de20ccc0ba45b38523ba2b7c593;800e6de20ccc0ba45b38523ba2b7c593;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.OneMinusNode;264;-8467.659,949.5699;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-229.8837,-971.7881;Float;False;Shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NegateNode;159;-215.7289,-639.547;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;363;-8842.096,545.2049;Float;True;Spherical;World;False;Top Texture 1;_TopTexture1;white;-1;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;False;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;4;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;266;-8202.908,675.3611;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.2;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;162;-237.4398,-753.8328;Float;False;161;Shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;160;-31.16787,-744.6395;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;229;-7967.52,468.1504;Float;True;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0.5;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;249;-7813.239,724.4283;Float;False;Constant;_Float6;Float 6;17;0;Create;True;0;0;False;0;0.76;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;248;-7783.529,548.788;Float;False;Constant;_Float4;Float 4;17;0;Create;True;0;0;False;0;0.11;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;255;-7600,784;Float;False;Constant;_Color4;Color 4;19;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;268;-9.046775,-495.814;Float;False;ShadowOutline;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;247;-7586.331,413.9883;Float;True;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;1,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ColorNode;254;-7599.859,607.5126;Float;False;Property;_LineHalfToneColor;Line HalfTone Color;23;0;Create;True;0;0;False;0;0.6737329,0.5859292,0.7264151,1;0.6737329,0.5859292,0.7264151,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;283;-7598.619,1119.917;Float;False;268;ShadowOutline;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;256;-7325.258,680.5975;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;231;-7056.618,581.7653;Float;False;LineHalftone;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;287;-7600,944;Float;False;Constant;_Color2;Color 2;5;0;Create;True;0;0;False;0;0.4339623,0.4339623,0.4339623,1;0.2008235,0.1791118,0.2169811,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;286;-7328,1072;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;284;-7088,928;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;242;-468.0156,546.2818;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;234;-572.2147,395.8688;Float;False;231;LineHalftone;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;244;-619.8036,232.758;Float;False;Constant;_Color1;Color 1;18;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;117;-444.1471,74.87103;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;285;-7058.097,743.8628;Float;False;ShadowOutlineColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;243;-301.513,400.034;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;114;-839.4231,1607.358;Float;False;1428.443;770.9457;Comment;15;101;112;113;111;92;104;102;110;116;118;119;130;131;133;122;Blink;1,1,1,1;0;0
Node;AmplifyShaderEditor.BlendOpsNode;250;-94.41012,228.8761;Float;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;271;-194.7565,523.2889;Float;False;285;ShadowOutlineColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;364;-11808.7,-297.129;Float;False;1883.126;1528.773;Comment;16;178;177;221;227;208;171;225;175;211;210;198;209;226;176;174;173;Old Function;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;348;-6164.252,1593.525;Float;True;9;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;111;-123.51,1680.164;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;336;-4658.616,373.9656;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;351;-5068.249,1837.99;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;174;-11331.55,236.4553;Float;False;Constant;_HalftoneScale;Halftone Scale;14;0;Create;True;0;0;False;0;100;10.56;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;176;-10975.4,-11.21383;Float;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;173;-11356.35,8.048372;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GrabScreenPosition;119;-626.8201,1964.898;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScaleAndOffsetNode;350;-5337.291,1591.463;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;122;-123.3828,2294.402;Float;False;Constant;_OutlineWidth;Outline Width;11;0;Create;True;0;0;False;0;0.01;0.055;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;211;-11272.58,355.509;Float;False;Constant;_Float3;Float 3;17;0;Create;True;0;0;False;0;-1000;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;225;-10887.45,772.9824;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-11245.55,694.8721;Float;False;3;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;226;-11136.3,896.1057;Float;False;Constant;_LineScale;LineScale;14;0;Create;True;0;0;False;0;200;10.56;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;346;-4430.494,1698.548;Float;False;Property;_UseRampAsAlbedo;UseRampAsAlbedo?;18;0;Create;True;0;0;False;0;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenParams;198;-11752.75,602.2186;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;131;-111.8315,2188.392;Float;False;Constant;_Float1;Float 1;18;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;227;-10528.92,690.1989;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;210;-11107.16,679.3823;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;171;-11699.85,384.1618;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;348.3323,1927.255;Float;False;FinalRenderWithBlink;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;177;-10606.35,22.31291;Float;False;Property;_Use_Screen_Space;Use_Screen_Space;11;0;Create;True;0;0;False;0;0;1;1;True;;Toggle;2;Key0;Key1;Create;False;9;1;FLOAT2;0,0;False;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0;False;6;FLOAT2;0,0;False;7;FLOAT2;0,0;False;8;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;110;-790.665,1879.924;Float;False;Constant;_BlinkSpeed;BlinkSpeed;11;0;Create;True;0;0;False;0;0;20;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;-326.7138,2113.428;Float;False;99;CustomToonLighting;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SinOpNode;104;-437.549,1854.619;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;379.166,2164.817;Float;False;OutlineWidth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;270;221.246,327.664;Float;False;Multiply;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;347;-4739.494,1813.548;Float;True;Property;_Ramp;Ramp;19;0;Create;True;0;0;False;0;None;f7768b9fcad442b4a8a5e4a0de3d4769;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexelSizeNode;209;-11608.25,826.1721;Float;False;178;1;0;SAMPLER2D;_Sampler0209;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;-11003.31,296.8497;Float;True;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;112;-802.5862,1674.669;Float;False;Constant;_IsBlinking;IsBlinking;12;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;102;-625.395,1859.48;Float;False;1;0;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;118;-354.9179,1927.333;Float;False;Global;_GrabScreen0;Grab Screen 0;16;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;352;-5738.844,1703.054;Float;True;3;0;FLOAT;0;False;1;FLOAT;-0.03;False;2;FLOAT;1.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;178;-10262.72,-14.99631;Float;True;Property;_HalftoneTexture;Halftone Texture;14;0;Create;True;0;0;False;0;6e1b83121839a5d48b3a21c79d494184;6e1b83121839a5d48b3a21c79d494184;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;113;-342.7473,1736.233;Float;False;Constant;_Float0;Float 0;16;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;130;151.7215,2140.417;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;337;-4908.133,424.0829;Float;False;Constant;_Float7;Float 7;15;0;Create;True;0;0;False;0;9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;101;153.3416,1910.162;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;221;-10524.67,320.6048;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;572.7387,71.879;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;CustomComicShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;1;False;-1;255;False;-1;255;False;-1;6;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;True;0.0006;0,0,0,0;VertexOffset;False;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;19;0
WireConnection;1;0;21;0
WireConnection;3;0;1;0
WireConnection;3;1;5;0
WireConnection;9;0;3;0
WireConnection;194;0;224;0
WireConnection;359;0;357;0
WireConnection;359;1;357;0
WireConnection;353;0;354;0
WireConnection;353;3;359;0
WireConnection;291;0;194;0
WireConnection;291;3;307;0
WireConnection;291;4;170;0
WireConnection;179;0;353;0
WireConnection;179;1;291;0
WireConnection;338;0;179;0
WireConnection;338;1;339;0
WireConnection;338;2;340;0
WireConnection;240;0;241;0
WireConnection;240;1;238;0
WireConnection;292;0;338;0
WireConnection;317;0;314;0
WireConnection;317;1;313;0
WireConnection;318;0;317;0
WireConnection;318;1;310;0
WireConnection;195;0;292;0
WireConnection;342;0;341;0
WireConnection;342;1;312;0
WireConnection;267;0;240;0
WireConnection;321;0;342;1
WireConnection;321;1;317;0
WireConnection;321;2;318;0
WireConnection;316;0;314;0
WireConnection;316;1;310;0
WireConnection;319;0;342;1
WireConnection;319;1;314;0
WireConnection;319;2;316;0
WireConnection;323;0;321;0
WireConnection;303;0;293;0
WireConnection;303;1;344;0
WireConnection;191;0;190;0
WireConnection;191;1;192;0
WireConnection;189;0;191;0
WireConnection;189;1;295;0
WireConnection;324;0;323;0
WireConnection;320;0;319;0
WireConnection;294;0;303;0
WireConnection;6;0;21;0
WireConnection;298;0;189;0
WireConnection;298;1;294;0
WireConnection;322;0;320;0
WireConnection;326;0;324;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;301;0;298;0
WireConnection;328;0;322;0
WireConnection;328;1;326;0
WireConnection;10;0;8;0
WireConnection;167;0;144;0
WireConnection;167;1;168;0
WireConnection;167;2;301;0
WireConnection;91;0;86;0
WireConnection;335;0;328;0
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;147;0;91;0
WireConnection;147;1;167;0
WireConnection;332;0;333;0
WireConnection;332;1;334;0
WireConnection;332;2;335;0
WireConnection;40;0;41;0
WireConnection;40;1;39;0
WireConnection;25;0;24;0
WireConnection;148;0;147;0
WireConnection;329;0;332;0
WireConnection;67;0;61;0
WireConnection;67;1;62;1
WireConnection;42;0;40;0
WireConnection;65;0;66;0
WireConnection;43;0;42;0
WireConnection;308;0;28;0
WireConnection;308;1;148;0
WireConnection;64;0;67;0
WireConnection;64;1;65;0
WireConnection;33;0;35;0
WireConnection;68;0;64;0
WireConnection;68;1;69;0
WireConnection;44;0;43;0
WireConnection;44;1;45;0
WireConnection;343;0;308;0
WireConnection;343;1;331;0
WireConnection;55;0;52;0
WireConnection;55;1;54;0
WireConnection;14;0;343;0
WireConnection;70;0;68;0
WireConnection;70;1;75;0
WireConnection;70;2;76;0
WireConnection;34;0;33;0
WireConnection;34;1;4;0
WireConnection;53;0;44;0
WireConnection;53;1;55;0
WireConnection;82;0;80;0
WireConnection;82;1;81;0
WireConnection;82;2;83;0
WireConnection;36;0;30;0
WireConnection;36;1;34;0
WireConnection;51;0;48;0
WireConnection;51;1;50;1
WireConnection;84;0;70;0
WireConnection;84;1;82;0
WireConnection;57;0;53;0
WireConnection;71;0;84;0
WireConnection;71;1;72;0
WireConnection;31;0;29;0
WireConnection;31;1;36;0
WireConnection;47;0;57;0
WireConnection;47;1;51;0
WireConnection;37;0;31;0
WireConnection;74;0;73;0
WireConnection;74;1;71;0
WireConnection;46;0;47;0
WireConnection;77;0;74;0
WireConnection;59;0;15;0
WireConnection;59;1;58;0
WireConnection;79;0;78;0
WireConnection;79;1;59;0
WireConnection;99;0;79;0
WireConnection;155;0;142;0
WireConnection;155;1;154;0
WireConnection;143;0;142;0
WireConnection;143;1;141;0
WireConnection;279;0;280;0
WireConnection;279;1;139;0
WireConnection;262;0;257;0
WireConnection;156;0;155;0
WireConnection;156;1;141;0
WireConnection;140;0;279;0
WireConnection;140;1;142;0
WireConnection;140;2;143;0
WireConnection;263;0;262;2
WireConnection;260;1;262;1
WireConnection;258;1;262;0
WireConnection;153;0;279;0
WireConnection;153;1;155;0
WireConnection;153;2;156;0
WireConnection;149;0;140;0
WireConnection;259;0;258;0
WireConnection;259;1;260;0
WireConnection;259;2;263;0
WireConnection;157;0;149;0
WireConnection;152;0;153;0
WireConnection;361;0;360;0
WireConnection;361;1;360;0
WireConnection;158;0;152;0
WireConnection;264;0;259;0
WireConnection;161;0;157;0
WireConnection;159;0;158;0
WireConnection;363;0;362;0
WireConnection;363;3;361;0
WireConnection;266;0;264;0
WireConnection;160;0;162;0
WireConnection;160;1;159;0
WireConnection;229;0;363;0
WireConnection;229;1;266;0
WireConnection;268;0;160;0
WireConnection;247;0;229;0
WireConnection;247;1;248;0
WireConnection;247;2;249;0
WireConnection;256;0;254;0
WireConnection;256;1;255;0
WireConnection;256;2;247;0
WireConnection;231;0;256;0
WireConnection;286;0;283;0
WireConnection;284;0;287;0
WireConnection;284;1;255;0
WireConnection;284;2;286;0
WireConnection;242;0;240;0
WireConnection;285;0;284;0
WireConnection;243;0;244;0
WireConnection;243;1;234;0
WireConnection;243;2;242;0
WireConnection;250;0;117;0
WireConnection;250;1;243;0
WireConnection;111;0;112;0
WireConnection;111;2;104;0
WireConnection;111;3;113;0
WireConnection;111;4;113;0
WireConnection;336;0;148;0
WireConnection;336;1;337;0
WireConnection;351;0;350;0
WireConnection;176;0;173;0
WireConnection;176;1;174;0
WireConnection;350;0;352;0
WireConnection;225;0;211;0
WireConnection;225;1;226;0
WireConnection;225;2;210;0
WireConnection;208;0;171;0
WireConnection;208;1;198;0
WireConnection;208;2;209;0
WireConnection;346;1;22;0
WireConnection;346;0;347;0
WireConnection;227;0;225;0
WireConnection;227;1;198;2
WireConnection;210;0;208;0
WireConnection;116;0;101;0
WireConnection;177;1;176;0
WireConnection;177;0;221;0
WireConnection;104;0;102;0
WireConnection;133;0;130;0
WireConnection;270;0;250;0
WireConnection;270;1;271;0
WireConnection;347;1;351;0
WireConnection;175;0;174;0
WireConnection;175;1;210;0
WireConnection;175;2;211;0
WireConnection;102;0;110;0
WireConnection;118;0;119;0
WireConnection;352;0;348;0
WireConnection;178;1;177;0
WireConnection;130;0;111;0
WireConnection;130;2;131;0
WireConnection;130;3;122;0
WireConnection;130;4;122;0
WireConnection;101;0;111;0
WireConnection;101;2;118;0
WireConnection;101;3;92;0
WireConnection;101;4;92;0
WireConnection;221;0;175;0
WireConnection;221;1;198;2
WireConnection;0;13;270;0
ASEEND*/
//CHKSM=0CA355015559BCA202EB6857A1E0C8A77EB43692