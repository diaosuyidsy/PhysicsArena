// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WaterWave"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainTex("MainTex", 2D) = "white" {}
		_DistortionValue("DistortionValue", Float) = 1
		_WaterColor("WaterColor", Color) = (0.514151,0.8074254,1,0)
		_Progress("Progress", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 _WaterColor;
		uniform sampler2D _MainTex;
		uniform float _DistortionValue;
		uniform float _Progress;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 color29 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
			float2 panner4 = ( _Time.y * float2( 0.1,1.5 ) + i.uv_texcoord);
			float2 appendResult18 = (float2(i.uv_texcoord.x , ( ( tex2D( _MainTex, panner4 ).g * ( ( ( i.uv_texcoord.y * i.uv_texcoord.y ) + 0.25 ) * _DistortionValue ) ) + ( _Time.y + i.uv_texcoord.y ) )));
			float temp_output_19_0 = ( tex2D( _MainTex, appendResult18 ).r + ( ( i.uv_texcoord.y * i.uv_texcoord.y ) * 1.2 ) );
			float clampResult25 = clamp( ( ( temp_output_19_0 - 0.5 ) * 2.09 ) , 0.0 , 1.0 );
			float4 lerpResult26 = lerp( _WaterColor , color29 , clampResult25);
			o.Emission = saturate( ( lerpResult26 + ( i.uv_texcoord.y * 0.5 ) ) ).rgb;
			o.Alpha = 1;
			float clampResult38 = clamp( _Progress , 0.0 , 1.0 );
			clip( ( temp_output_19_0 * clampResult38 ) - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
7;259;1522;760;-562.4568;251.689;1.612929;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;9;-1753.76,130.3158;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;5;-773.1538,-357.0845;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;3;-1012.668,-239.6703;Float;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;False;0;0.1,1.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-855.0608,84.1748;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;1;-1092.916,-42.6998;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;4;-460.1538,-264.0845;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;-535.3586,-515.4327;Float;True;Property;_MainTex;MainTex;1;0;Create;True;0;0;False;0;99337227e3f045d4f851089f515ec204;99337227e3f045d4f851089f515ec204;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;12;-593.0608,122.1748;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-645.6934,248.2624;Float;False;Property;_DistortionValue;DistortionValue;2;0;Create;True;0;0;False;0;1;0.94;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-258.8413,-295.9936;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;99337227e3f045d4f851089f515ec204;99337227e3f045d4f851089f515ec204;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-415.6934,172.2624;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-2.149204,127.0372;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-754.8348,380.1727;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;16;289.3741,296.9128;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;20;450.2956,393.8418;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;909.2956,403.8418;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;572.7632,252.3112;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;1077.296,402.8418;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;760.8374,97.9562;Float;True;Property;_TextureSample1;Texture Sample 1;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;19;1253.578,485.9269;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;23;1506.505,339.3909;Float;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;1724.424,287.6135;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;2.09;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;31;2136.236,513.6072;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;25;2063.497,385.5412;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;27;1969.789,13.20426;Float;False;Property;_WaterColor;WaterColor;3;0;Create;True;0;0;False;0;0.514151,0.8074254,1,0;0,0.6651967,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;29;1968.766,186.0908;Float;False;Constant;_Color0;Color 0;4;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;37;961.4371,713.7232;Float;False;Property;_Progress;Progress;4;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;2243.682,241.0716;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;2440.874,510.5095;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;30;2638.879,208.3846;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;38;1238.145,699.9393;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;34;2889.064,336.5928;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;1628.902,607.7644;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3202.26,194.4683;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;WaterWave;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;11;0;9;2
WireConnection;11;1;9;2
WireConnection;4;0;5;0
WireConnection;4;2;3;0
WireConnection;4;1;1;0
WireConnection;12;0;11;0
WireConnection;6;0;7;0
WireConnection;6;1;4;0
WireConnection;13;0;12;0
WireConnection;13;1;14;0
WireConnection;15;0;6;2
WireConnection;15;1;13;0
WireConnection;17;0;1;0
WireConnection;17;1;9;2
WireConnection;16;0;15;0
WireConnection;16;1;17;0
WireConnection;21;0;20;2
WireConnection;21;1;20;2
WireConnection;18;0;9;1
WireConnection;18;1;16;0
WireConnection;22;0;21;0
WireConnection;8;0;7;0
WireConnection;8;1;18;0
WireConnection;19;0;8;1
WireConnection;19;1;22;0
WireConnection;23;0;19;0
WireConnection;24;0;23;0
WireConnection;25;0;24;0
WireConnection;26;0;27;0
WireConnection;26;1;29;0
WireConnection;26;2;25;0
WireConnection;35;0;31;2
WireConnection;30;0;26;0
WireConnection;30;1;35;0
WireConnection;38;0;37;0
WireConnection;34;0;30;0
WireConnection;36;0;19;0
WireConnection;36;1;38;0
WireConnection;0;2;34;0
WireConnection;0;10;36;0
ASEEND*/
//CHKSM=75BE46C88E386234C511AD019D3F32034B252F59