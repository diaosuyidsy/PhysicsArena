// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SuckGunWave"
{
	Properties
	{
		[HDR]_Color0("Color 0", Color) = (1,1,1,0)
		_Noise01("Noise 01", 2D) = "white" {}
		_Noise02("Noise 02", 2D) = "white" {}
		_DistortionNoise("Distortion Noise", 2D) = "white" {}
		_GradientMask("Gradient Mask", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Ramp("Ramp", 2D) = "white" {}
		_OpacityPower("Opacity Power", Float) = 1
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv_tex4coord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _Ramp;
		uniform sampler2D _GradientMask;
		uniform float4 _GradientMask_ST;
		uniform sampler2D _Noise01;
		uniform sampler2D _DistortionNoise;
		uniform sampler2D _Noise02;
		uniform float4 _Color0;
		uniform float _OpacityPower;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_GradientMask = i.uv_texcoord * _GradientMask_ST.xy + _GradientMask_ST.zw;
			float4 uv0_GradientMask = i.uv_tex4coord;
			uv0_GradientMask.xy = i.uv_tex4coord.xy * _GradientMask_ST.xy + _GradientMask_ST.zw;
			float2 appendResult7 = (float2(i.uv_texcoord.x , ( i.uv_texcoord.y - uv0_GradientMask.z )));
			float2 uv_TexCoord34 = i.uv_texcoord * float2( 2,0.75 );
			float2 panner4 = ( _Time.y * float2( 0,-1 ) + uv_TexCoord34);
			float2 uv_TexCoord35 = i.uv_texcoord * float2( 3,0.5 );
			float2 panner6 = ( _Time.y * float2( 0,-0.25 ) + uv_TexCoord35);
			float temp_output_11_0 = ( tex2D( _DistortionNoise, panner6 ).r * 0.1 );
			float2 panner5 = ( _Time.y * float2( 0,-0.5 ) + uv_TexCoord34);
			float temp_output_21_0 = ( tex2D( _GradientMask, uv_GradientMask ).r * tex2D( _GradientMask, appendResult7 ).r * tex2D( _Noise01, ( panner4 + temp_output_11_0 ) ).r * tex2D( _Noise02, ( panner5 + temp_output_11_0 ) ).r * 2.5 );
			float2 appendResult23 = (float2(temp_output_21_0 , 0.0));
			o.Emission = ( tex2D( _Ramp, ( ( appendResult23 * float2( 0.77,0 ) ) + float2( 0.48,0 ) ) ) * i.vertexColor * _Color0 ).rgb;
			o.Alpha = 1;
			clip( saturate( ( temp_output_21_0 * _OpacityPower * i.vertexColor.a ) ) - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
6.4;1424;1524;459;-1351.716;-440.1603;1.12598;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;17;-628.4282,485.1101;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;35;-882.217,681.0726;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;3,0.5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;6;-307.8143,764.9429;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.25;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-687.291,54.23711;Float;False;0;14;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-883.1075,472.5235;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;2,0.75;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-881.3567,295.726;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;31.91486,990.1191;Float;False;Constant;_DistortionPower;Distortion Power;2;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;10;-49.14558,768.2297;Float;True;Property;_DistortionNoise;Distortion Noise;3;0;Create;True;0;0;False;0;c4a9b440793e5804eb233a66534a0af8;c4a9b440793e5804eb233a66534a0af8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;3;-334.7429,185.7366;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;4;-329.0127,456.0084;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;293.9149,802.1191;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;5;-320.8557,618.8793;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;7;-23.92212,186.1172;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;13;436.5151,613.1585;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;396.0506,420.7728;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;14;135.1812,-49.95417;Float;True;Property;_GradientMask;Gradient Mask;4;0;Create;True;0;0;False;0;f9f0c5e09514cb14aa7c0663a7263f8c;f9f0c5e09514cb14aa7c0663a7263f8c;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;19;533.2746,404.2343;Float;True;Property;_Noise01;Noise 01;1;0;Create;True;0;0;False;0;f65f691a17b47ea43a4c3c3cc3e5ef49;f65f691a17b47ea43a4c3c3cc3e5ef49;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;16;424.6342,192.0809;Float;True;Property;_TextureSample1;Texture Sample 1;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;585.2399,619.2377;Float;True;Property;_Noise02;Noise 02;2;0;Create;True;0;0;False;0;1746d72c8cbcebf4ebe6ab93898e2f61;1746d72c8cbcebf4ebe6ab93898e2f61;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;611.913,867.1272;Float;False;Constant;_MaskPower;Mask Power;5;0;Create;True;0;0;False;0;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;15;428.7729,21.01689;Float;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;956.918,468.6842;Float;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;23;1202.07,446.0645;Float;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;1439.212,463.2894;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.77,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;32;1041.409,927.6902;Float;False;Property;_OpacityPower;Opacity Power;7;0;Create;True;0;0;False;0;1;12;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;26;1540.212,577.2894;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.48,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;36;1086.26,1006.609;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;25;1661.304,458.4265;Float;True;Property;_Ramp;Ramp;6;0;Create;True;0;0;False;0;427fdefda3c71f24899927ca878b2b6a;427fdefda3c71f24899927ca878b2b6a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;29;1826.891,689.5024;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;30;1820.194,866.3173;Float;False;Property;_Color0;Color 0;0;1;[HDR];Create;True;0;0;False;0;1,1,1,0;1.515717,1.515717,1.515717,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;1302.203,879.0469;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;2127.129,520.0057;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;892.9561,191.5191;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;33;1540.153,934.6293;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2349.548,343.815;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SuckGunWave;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;5;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;6;0;35;0
WireConnection;6;1;17;0
WireConnection;10;1;6;0
WireConnection;3;0;2;2
WireConnection;3;1;1;3
WireConnection;4;0;34;0
WireConnection;4;1;17;0
WireConnection;11;0;10;1
WireConnection;11;1;12;0
WireConnection;5;0;34;0
WireConnection;5;1;17;0
WireConnection;7;0;2;1
WireConnection;7;1;3;0
WireConnection;13;0;5;0
WireConnection;13;1;11;0
WireConnection;8;0;4;0
WireConnection;8;1;11;0
WireConnection;19;1;8;0
WireConnection;16;0;14;0
WireConnection;16;1;7;0
WireConnection;20;1;13;0
WireConnection;15;0;14;0
WireConnection;21;0;15;1
WireConnection;21;1;16;1
WireConnection;21;2;19;1
WireConnection;21;3;20;1
WireConnection;21;4;22;0
WireConnection;23;0;21;0
WireConnection;27;0;23;0
WireConnection;26;0;27;0
WireConnection;25;1;26;0
WireConnection;31;0;21;0
WireConnection;31;1;32;0
WireConnection;31;2;36;4
WireConnection;28;0;25;0
WireConnection;28;1;29;0
WireConnection;28;2;30;0
WireConnection;33;0;31;0
WireConnection;0;2;28;0
WireConnection;0;10;33;0
ASEEND*/
//CHKSM=DD6A0B615E9B94C157DD225EAC1FD42CE66AE4FF