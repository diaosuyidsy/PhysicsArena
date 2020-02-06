// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CurveParticles"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_TextureSample3("Texture Sample 3", 2D) = "white" {}
		[HDR]_Color0("_Color0", Color) = (0.5,0.5,0.5,0)
		_OpacityProcess("OpacityProcess", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow nofog nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform sampler2D _TextureSample3;
		uniform sampler2D _TextureSample0;
		uniform sampler2D _TextureSample1;
		uniform float4 _Color0;
		uniform float _OpacityProcess;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 panner6 = ( _Time.y * float2( 0,0.05 ) + i.uv_texcoord);
			float2 appendResult5 = (float2(i.uv_texcoord.x , ( i.uv_texcoord.y + (-1.0 + (i.vertexColor.a - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) )));
			float temp_output_12_0 = ( saturate( ( ( tex2D( _TextureSample0, panner6 ).r + 0.2 ) * tex2D( _TextureSample1, appendResult5 ).r * 1.5 ) ) * i.vertexColor.r );
			float2 appendResult18 = (float2(temp_output_12_0 , 0.0));
			o.Emission = ( tex2D( _TextureSample3, ( appendResult18 + float2( 0.1,0 ) ) ) * 5.0 * _Color0 ).rgb;
			o.Alpha = saturate( ( temp_output_12_0 - ( 1.0 - _OpacityProcess ) ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
-34.4;1356.8;1524;443;329.4921;81.36948;1;True;False
Node;AmplifyShaderEditor.VertexColorNode;1;-1587.114,70.93407;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1338.197,-95.11218;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;2;-1323.491,67.64758;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;27;-1089.174,-190.4079;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-1087.197,37.88782;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;6;-899.5562,-225.275;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0.05;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;5;-924.2974,-51.20609;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;7;-652.2456,-266.2189;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;52dd7ccd1af1f134d8ece592c38ff312;52dd7ccd1af1f134d8ece592c38ff312;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;29;-286.0291,-207.7388;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;10;-457.2051,177.7016;Float;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;1.5;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-663.4532,-43.77361;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;66b45ce6123b5754b8f15a5568c0f279;66b45ce6123b5754b8f15a5568c0f279;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-221.8364,-97.39307;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;11;-41.04468,-63.79172;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;152.3914,16.56305;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;337.1915,162.1342;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;31;220.7709,95.3599;Float;False;Property;_OpacityProcess;OpacityProcess;7;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;483.7739,247.3444;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.1,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;32;445.5079,109.6305;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;761.1447,404.4931;Float;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;False;0;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;23;695.1447,493.4931;Float;False;Property;_Color0;_Color0;5;1;[HDR];Create;True;0;0;False;0;0.5,0.5,0.5,0;0.5377358,0.5377358,0.5377358,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;626.8521,209.9096;Float;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;False;0;None;427fdefda3c71f24899927ca878b2b6a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;30;598.7709,13.35988;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;25;-594.1113,457.1086;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;967.0299,256.2782;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;15;-139.3204,445.5926;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-211.772,199.472;Float;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;False;0;None;57b778c779837a4498fef9f48b25b511;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;-839.3545,549.579;Float;False;Property;_FresnelPower;FresnelPower;6;0;Create;True;0;0;False;0;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;16;768.1694,-13.79054;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1405.452,31.34541;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;CurveParticles;False;False;False;False;False;False;False;False;False;True;True;True;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;1;4
WireConnection;4;0;3;2
WireConnection;4;1;2;0
WireConnection;6;0;3;0
WireConnection;6;1;27;0
WireConnection;5;0;3;1
WireConnection;5;1;4;0
WireConnection;7;1;6;0
WireConnection;29;0;7;1
WireConnection;8;1;5;0
WireConnection;9;0;29;0
WireConnection;9;1;8;1
WireConnection;9;2;10;0
WireConnection;11;0;9;0
WireConnection;12;0;11;0
WireConnection;12;1;1;1
WireConnection;18;0;12;0
WireConnection;28;0;18;0
WireConnection;32;0;31;0
WireConnection;20;1;28;0
WireConnection;30;0;12;0
WireConnection;30;1;32;0
WireConnection;25;3;26;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;21;2;23;0
WireConnection;15;0;25;0
WireConnection;16;0;30;0
WireConnection;0;2;21;0
WireConnection;0;9;16;0
ASEEND*/
//CHKSM=94D229FFD747783864E0251F5375E902E11F4EEE