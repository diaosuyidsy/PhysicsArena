// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SuckGunLine"
{
	Properties
	{
		_Noise2("Noise2", 2D) = "white" {}
		_Noise1("Noise1", 2D) = "white" {}
		_Noise3("Noise3", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Ramp("Ramp", 2D) = "white" {}
		_ColorStrength("ColorStrength", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Ramp;
		uniform sampler2D _Noise2;
		uniform sampler2D _Noise1;
		uniform sampler2D _Noise3;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float _ColorStrength;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float mulTime9 = _Time.y * 0.7;
			float2 panner6 = ( mulTime9 * float2( -1,0 ) + i.uv_texcoord);
			float mulTime23 = _Time.y * 0.6;
			float2 panner24 = ( mulTime23 * float2( -1,0 ) + i.uv_texcoord);
			float temp_output_95_0 = step( tex2D( _Noise1, panner24 ).r , 0.2 );
			float mulTime28 = _Time.y * 2.0;
			float2 uv_TexCoord99 = i.uv_texcoord * float2( 0.5,0.5 );
			float2 panner29 = ( mulTime28 * float2( -1,0 ) + uv_TexCoord99);
			float clampResult36 = clamp( ( step( tex2D( _Noise3, panner29 ).r , 0.18 ) + 0.5 ) , 0.0 , 1.0 );
			float clampResult40 = clamp( ( ( ( 1.0 - step( tex2D( _Noise2, panner6 ).r , 0.67 ) ) * temp_output_95_0 * clampResult36 ) + ( ( 1.0 - temp_output_95_0 ) * clampResult36 ) ) , 0.0 , 1.0 );
			float2 appendResult125 = (float2(clampResult40 , 0.0));
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode47 = tex2D( _Mask, uv_Mask );
			o.Emission = ( tex2D( _Ramp, ( appendResult125 * float2( 0.7,0 ) ) ) * tex2DNode47.r * clampResult40 * _ColorStrength ).rgb;
			float clampResult50 = clamp( ( clampResult40 * ( 1.0 - step( tex2DNode47.r , 0.49 ) ) * 2.0 ) , 0.0 , 1.0 );
			o.Alpha = clampResult50;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
47;486;1523;425;-1302.78;216.7094;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;27;-907.2698,341.8753;Float;False;Constant;_Noise3ScrollSpeed;Noise3ScrollSpeed;0;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-876.2194,-15.92616;Float;False;Constant;_Noise2ScrollSpeed;Noise2ScrollSpeed;0;0;Create;True;0;0;False;0;0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;28;-685.7747,366.9171;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;99;-874.5095,458.2465;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.5,0.5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-877.2622,135.9594;Float;False;Constant;_Noise1ScrollSpeed;Noise1ScrollSpeed;0;0;Create;True;0;0;False;0;0.6;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-889.7716,-173.0738;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;23;-661.4574,142.1581;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;29;-422.852,371.5025;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;9;-659.5637,-18.37963;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;6;-431.554,-89.61771;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;24;-412.4317,132.5221;Float;True;3;0;FLOAT2;0,0;False;2;FLOAT2;-1,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;26;-123.9458,385.7356;Float;True;Property;_Noise3;Noise3;2;0;Create;True;0;0;False;0;b3d2d0e01f599534f882e5ccf78883f0;b3d2d0e01f599534f882e5ccf78883f0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;93;183.0663,388.9526;Float;True;2;0;FLOAT;0;False;1;FLOAT;0.18;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;25;-141.5316,123.9425;Float;True;Property;_Noise1;Noise1;1;0;Create;True;0;0;False;0;c4a9b440793e5804eb233a66534a0af8;c4a9b440793e5804eb233a66534a0af8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;8;-136.5507,-120.7431;Float;True;Property;_Noise2;Noise2;0;0;Create;True;0;0;False;0;52dd7ccd1af1f134d8ece592c38ff312;52dd7ccd1af1f134d8ece592c38ff312;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;122.8397,671.0856;Float;False;Constant;_Float2;Float 2;3;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;95;247.4189,128.3971;Float;True;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;371.0039,521.5251;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;97;224.954,-135.9376;Float;True;2;0;FLOAT;0;False;1;FLOAT;0.67;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;96;653.8107,191.9827;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;539.1099,417.7473;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;98;586.2796,-121.0976;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;843.9982,217.3844;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;825.9255,-141.1261;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;1069.447,106.3868;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;40;1373.327,104.6358;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;1391.479,419.7786;Float;True;Property;_Mask;Mask;3;0;Create;True;0;0;False;0;f9f0c5e09514cb14aa7c0663a7263f8c;f9f0c5e09514cb14aa7c0663a7263f8c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;125;1635.719,137.9504;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.StepOpNode;89;1764.454,775.6565;Float;True;2;0;FLOAT;0;False;1;FLOAT;0.49;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;2203.561,624.8965;Float;False;Constant;_Float3;Float 3;7;0;Create;True;0;0;False;0;2;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;1813.274,184.9222;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.7,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;91;1931.399,527.6656;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;2339.048,521.1459;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;124;1895.377,-90.5866;Float;True;Property;_Ramp;Ramp;4;0;Create;True;0;0;False;0;427fdefda3c71f24899927ca878b2b6a;427fdefda3c71f24899927ca878b2b6a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;127;2073.406,172.1272;Float;False;Property;_ColorStrength;ColorStrength;5;0;Create;True;0;0;False;0;1;2.07;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;2759.655,516.0564;Float;False;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;2392.611,-15.2318;Float;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;50;2504.191,491.6143;Float;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;110;2504.943,847.7628;Float;True;3;0;FLOAT;0;False;1;FLOAT;0.17;False;2;FLOAT;0.38;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;111;2512.929,1104.063;Float;True;3;0;FLOAT;0;False;1;FLOAT;0.6;False;2;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;116;3029.161,656.9482;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;117;2964.663,938.6644;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;118;2946.663,1105.664;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;115;3135.217,1030.109;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;120;3238.802,618.8234;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;113;2816.975,873.5054;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;121;3280.23,343.1322;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;114;2754.776,1104.888;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2837.081,105.4358;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;SuckGunLine;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;28;0;27;0
WireConnection;23;0;22;0
WireConnection;29;0;99;0
WireConnection;29;1;28;0
WireConnection;9;0;4;0
WireConnection;6;0;7;0
WireConnection;6;1;9;0
WireConnection;24;0;7;0
WireConnection;24;1;23;0
WireConnection;26;1;29;0
WireConnection;93;0;26;1
WireConnection;25;1;24;0
WireConnection;8;1;6;0
WireConnection;95;0;25;1
WireConnection;34;0;93;0
WireConnection;34;1;35;0
WireConnection;97;0;8;1
WireConnection;96;0;95;0
WireConnection;36;0;34;0
WireConnection;98;0;97;0
WireConnection;37;0;96;0
WireConnection;37;1;36;0
WireConnection;33;0;98;0
WireConnection;33;1;95;0
WireConnection;33;2;36;0
WireConnection;38;0;33;0
WireConnection;38;1;37;0
WireConnection;40;0;38;0
WireConnection;125;0;40;0
WireConnection;89;0;47;1
WireConnection;126;0;125;0
WireConnection;91;0;89;0
WireConnection;48;0;40;0
WireConnection;48;1;91;0
WireConnection;48;2;49;0
WireConnection;124;1;126;0
WireConnection;101;0;50;0
WireConnection;44;0;124;0
WireConnection;44;1;47;1
WireConnection;44;2;40;0
WireConnection;44;3;127;0
WireConnection;50;0;48;0
WireConnection;110;0;47;1
WireConnection;111;0;47;1
WireConnection;116;0;117;0
WireConnection;116;1;115;0
WireConnection;117;0;113;0
WireConnection;118;0;114;0
WireConnection;115;0;118;0
WireConnection;120;0;116;0
WireConnection;113;0;110;0
WireConnection;121;1;120;0
WireConnection;114;0;111;0
WireConnection;0;2;44;0
WireConnection;0;9;50;0
ASEEND*/
//CHKSM=7F4323DB190DAA1083365AC88B50866F1D7D5BA6