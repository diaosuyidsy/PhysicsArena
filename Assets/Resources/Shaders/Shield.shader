// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shield"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_Color0("Color 0", Color) = (0,0,0,0)
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_CrackPower("CrackPower", Float) = 0
		[HDR]_CrackColor("CrackColor", Color) = (0.1650943,0.6460097,1,0)
		_Range("Range", Float) = 0.82
		_Distance("Distance", Range( 0 , 1)) = 10
		_BreakDirection("BreakDirection", Vector) = (0,0,0,0)
		_RotateStrength("RotateStrength", Float) = 1
		_Energy("Energy", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float4 screenPos;
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

		uniform float _RotateStrength;
		uniform float _Range;
		uniform float _Distance;
		uniform float3 _BreakDirection;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float4 _Color0;
		uniform float _Energy;
		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _CrackColor;
		uniform sampler2D _TextureSample1;
		uniform float4 _TextureSample1_ST;
		uniform float _CrackPower;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


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


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 appendResult34 = (float3(( ( 1.0 - v.color.r ) - 0.5 ) , ( v.color.g - 0.5 ) , ( v.color.b - 0.5 )));
			float3 _BreakOrigin = float3(0,0,0);
			float temp_output_40_0 = ( distance( appendResult34 , _BreakOrigin ) - _Range );
			float clampResult53 = clamp( temp_output_40_0 , 0.1 , 1.0 );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 rotatedValue48 = RotateAroundAxis( appendResult34, ase_vertex3Pos, v.color.rgb, ( ( _RotateStrength * clampResult53 * _Distance ) * UNITY_PI ) );
			float clampResult43 = clamp( ( 1.0 - temp_output_40_0 ) , 0.0 , (0.0 + (_Distance - 0.0) * (0.25 - 0.0) / (1.0 - 0.0)) );
			float3 normalizeResult39 = normalize( ( appendResult34 - _BreakOrigin ) );
			v.vertex.xyz += ( ( rotatedValue48 - ase_vertex3Pos ) + ( clampResult43 * ( normalizeResult39 * _BreakDirection ) ) );
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float Distance81 = _Distance;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 screenColor14 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ase_grabScreenPos.xy/ase_grabScreenPos.w);
			float temp_output_94_0 = (-0.7 + (_Energy - 0.4) * (-1.3 - -0.7) / (1.0 - 0.4));
			float ifLocalVar93 = 0;
			if( _Energy >= 0.41 )
				ifLocalVar93 = temp_output_94_0;
			else
				ifLocalVar93 = (0.2 + (_Energy - 0.0) * (-0.7 - 0.2) / (0.4 - 0.0));
			float temp_output_90_0 = (1.3 + (_Energy - 0.4) * (1.0 - 1.3) / (1.0 - 0.4));
			float ifLocalVar91 = 0;
			if( _Energy >= 0.41 )
				ifLocalVar91 = temp_output_90_0;
			else
				ifLocalVar91 = (2.0 + (_Energy - 0.0) * (1.3 - 2.0) / (0.4 - 0.0));
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float smoothstepResult28 = smoothstep( ifLocalVar93 , ifLocalVar91 , tex2D( _TextureSample0, uv_TextureSample0 ).r);
			float clampResult88 = clamp( smoothstepResult28 , 0.0 , 1.0 );
			float4 lerpResult13 = lerp( screenColor14 , ( _Color0 * 2.0 ) , clampResult88);
			float2 temp_cast_0 = (0.0).xx;
			float2 temp_cast_1 = (1.0).xx;
			float2 temp_cast_2 = (-1.0).xx;
			float2 temp_cast_3 = (1.0).xx;
			float2 uv_TextureSample1 = i.uv_texcoord * _TextureSample1_ST.xy + _TextureSample1_ST.zw;
			float smoothstepResult18 = smoothstep( 0.0 , 0.35 , ( tex2D( _TextureSample1, uv_TextureSample1 ).r * _CrackPower ));
			c.rgb = ( lerpResult13 + ( ( 1.0 - step( length( (temp_cast_2 + (i.uv_texcoord - temp_cast_0) * (temp_cast_3 - temp_cast_2) / (temp_cast_1 - temp_cast_0)) ) , (0.15 + (_Energy - 0.0) * (1.0 - 0.15) / (0.5 - 0.0)) ) ) * ( _CrackColor * smoothstepResult18 ) ) ).rgb;
			c.a = ( 1.0 - Distance81 );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting alpha:fade keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
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
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
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
-212;610;1522;766;1907.193;68.26964;1.988369;True;False
Node;AmplifyShaderEditor.VertexColorNode;29;-184.3444,1017.006;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;30;98.11227,1051.146;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;31;283.637,1036.352;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;32;281.364,1125;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;33;292.729,1218.194;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;34;638.4996,1072.252;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;37;602.5961,1304.857;Float;False;Constant;_BreakOrigin;BreakOrigin;6;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;41;1000.94,1234.948;Float;False;Property;_Range;Range;5;0;Create;True;0;0;False;0;0.82;0.09;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;36;926.5352,1112.067;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-1065.046,439.3809;Float;False;Constant;_Float3;Float 3;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-1360.817,1111.939;Float;False;Constant;_Float2;Float 2;10;0;Create;True;0;0;False;0;-0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-1340.674,913.1359;Float;False;Property;_Energy;Energy;9;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;67;-1077.609,513.9458;Float;False;Constant;_Float4;Float 4;10;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;40;1417.571,1059.755;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-1108.379,623.6911;Float;False;Constant;_Float5;Float 5;10;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;64;-1060.547,214.5797;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;97;-1177.448,1373.884;Float;False;Constant;_Float6;Float 6;10;0;Create;True;0;0;False;0;1.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;15;-301.2099,479.1501;Float;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;False;0;None;5047f7bc739bcbb4c9e978828c2cab4d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;92;-907.0117,1402.87;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.4;False;3;FLOAT;2;False;4;FLOAT;1.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;90;-852.1473,1185.884;Float;False;5;0;FLOAT;0.4;False;1;FLOAT;0.4;False;2;FLOAT;1;False;3;FLOAT;1.3;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;45;1232.692,1236.466;Float;False;Property;_Distance;Distance;6;0;Create;True;0;0;False;0;10;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;95;-1124.434,1191.812;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.4;False;3;FLOAT;0.2;False;4;FLOAT;-0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;65;-874.0941,437.4184;Float;True;5;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT2;1,1;False;3;FLOAT2;-1,-1;False;4;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;50;1244.137,663.1059;Float;False;Property;_RotateStrength;RotateStrength;8;0;Create;True;0;0;False;0;1;5.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;94;-1043.01,996.1844;Float;False;5;0;FLOAT;0.4;False;1;FLOAT;0.4;False;2;FLOAT;1;False;3;FLOAT;0.2;False;4;FLOAT;-1.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;53;1361.306,825.0809;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-200.0932,676.8507;Float;False;Property;_CrackPower;CrackPower;3;0;Create;True;0;0;False;0;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LengthOpNode;70;-506.0824,310.8868;Float;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;73.80922,714.6833;Float;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;False;0;0.35;0.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-661.5032,39.15735;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;0a0ba8636ef5a7344a9e319bde3bde32;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;38;919.5962,1355.857;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ConditionalIfNode;93;-784.785,859.9924;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.41;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;54;1496.812,665.8305;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;46.7215,496.716;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;77;40.38025,294.4237;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;3;FLOAT;0.15;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;91;-546.7373,856.6045;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0.41;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;51;1846.78,854.6691;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;22;452.5797,392.1387;Float;False;Property;_CrackColor;CrackColor;4;1;[HDR];Create;True;0;0;False;0;0.1650943,0.6460097,1,0;0.6701568,2.884321,4,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;18;473.7511,578.4033;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;39;1312.504,1332.983;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PiNode;49;1654.621,693.2482;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;6;-872.9268,-243.221;Float;False;Property;_Color0;Color 0;1;0;Create;True;0;0;False;0;0,0,0,0;0.4103765,0.833957,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;78;1570.51,1228.021;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;74;523.1947,159.2278;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;28;-50.89327,-26.58282;Float;True;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-785.3498,-73.55084;Float;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;42;1525.271,1172.638;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;47;1290.131,1497.213;Float;False;Property;_BreakDirection;BreakDirection;7;0;Create;True;0;0;False;0;0,0,0;3,8,3;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;81;1598.641,1010.95;Float;False;Distance;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;715.6591,571.9115;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-591.5298,-80.43201;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;14;-437.8499,-222.6431;Float;False;Global;_GrabScreen0;Grab Screen 0;2;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;88;229.478,65.64518;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;1589.131,1409.213;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;43;1786.251,1203.066;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;48;1881.421,723.4025;Float;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;75;838.9529,253.1196;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;2067.018,433.1291;Float;False;81;Distance;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;13;427.9839,-137.4487;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;52;2123.91,918.7538;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;1982.993,1283.948;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;926.3275,338.5226;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;794.918,-102.2544;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;2272.371,1101.156;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;87;2297.018,485.1291;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2752.929,292.146;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;Shield;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;30;0;29;1
WireConnection;31;0;30;0
WireConnection;32;0;29;2
WireConnection;33;0;29;3
WireConnection;34;0;31;0
WireConnection;34;1;32;0
WireConnection;34;2;33;0
WireConnection;36;0;34;0
WireConnection;36;1;37;0
WireConnection;40;0;36;0
WireConnection;40;1;41;0
WireConnection;92;0;61;0
WireConnection;92;4;97;0
WireConnection;90;0;61;0
WireConnection;90;3;97;0
WireConnection;95;0;61;0
WireConnection;95;4;96;0
WireConnection;65;0;64;0
WireConnection;65;1;66;0
WireConnection;65;2;67;0
WireConnection;65;3;68;0
WireConnection;65;4;67;0
WireConnection;94;0;61;0
WireConnection;94;3;96;0
WireConnection;53;0;40;0
WireConnection;70;0;65;0
WireConnection;38;0;34;0
WireConnection;38;1;37;0
WireConnection;93;0;61;0
WireConnection;93;2;94;0
WireConnection;93;3;94;0
WireConnection;93;4;95;0
WireConnection;54;0;50;0
WireConnection;54;1;53;0
WireConnection;54;2;45;0
WireConnection;16;0;15;1
WireConnection;16;1;17;0
WireConnection;77;0;61;0
WireConnection;91;0;61;0
WireConnection;91;2;90;0
WireConnection;91;3;90;0
WireConnection;91;4;92;0
WireConnection;18;0;16;0
WireConnection;18;2;19;0
WireConnection;39;0;38;0
WireConnection;49;0;54;0
WireConnection;78;0;45;0
WireConnection;74;0;70;0
WireConnection;74;1;77;0
WireConnection;28;0;10;1
WireConnection;28;1;93;0
WireConnection;28;2;91;0
WireConnection;42;0;40;0
WireConnection;81;0;45;0
WireConnection;20;0;22;0
WireConnection;20;1;18;0
WireConnection;12;0;6;0
WireConnection;12;1;11;0
WireConnection;88;0;28;0
WireConnection;46;0;39;0
WireConnection;46;1;47;0
WireConnection;43;0;42;0
WireConnection;43;2;78;0
WireConnection;48;0;29;0
WireConnection;48;1;49;0
WireConnection;48;2;34;0
WireConnection;48;3;51;0
WireConnection;75;0;74;0
WireConnection;13;0;14;0
WireConnection;13;1;12;0
WireConnection;13;2;88;0
WireConnection;52;0;48;0
WireConnection;52;1;51;0
WireConnection;44;0;43;0
WireConnection;44;1;46;0
WireConnection;23;0;75;0
WireConnection;23;1;20;0
WireConnection;24;0;13;0
WireConnection;24;1;23;0
WireConnection;55;0;52;0
WireConnection;55;1;44;0
WireConnection;87;0;86;0
WireConnection;0;9;87;0
WireConnection;0;13;24;0
WireConnection;0;11;55;0
ASEEND*/
//CHKSM=1FA52E17F4C2C01E4FC1B839A81F2717822FD3E6