// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PostProcessComic"
{
	Properties
	{
		_RenderTexture("RenderTexture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float4 ColorsArray[10];
		uniform sampler2D _RenderTexture;
		uniform float4 _RenderTexture_ST;


		float3 MyCustomExpression89( float4 Colors , float4 ScreenColor )
		{
			float4 bestColor = float4(0, 0, 0, 0);
			float3 bestDiff = float3(1000, 1000, 1000);
			for (float i = 0; i < 10 ; i++) {
			    float4 palCol = ColorsArray[i];
			    float3 diff = abs(ScreenColor.rgb -  palCol.rgb);
			    if (length(diff) < length(bestDiff)) {
			        bestDiff = diff;
			        bestColor = palCol;
			    }
			}
			return bestColor;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 Colors89 = ColorsArray[clamp(3,0,(10 - 1))];
			float2 uv_RenderTexture = i.uv_texcoord * _RenderTexture_ST.xy + _RenderTexture_ST.zw;
			float4 ScreenColor15 = tex2D( _RenderTexture, uv_RenderTexture );
			float4 ScreenColor89 = ScreenColor15;
			float3 localMyCustomExpression89 = MyCustomExpression89( Colors89 , ScreenColor89 );
			o.Emission = localMyCustomExpression89;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
6.4;1105.6;1524;753;272.2142;385.9395;1.517966;True;False
Node;AmplifyShaderEditor.SamplerNode;95;-2370.342,-764.3101;Float;True;Property;_RenderTexture;RenderTexture;3;0;Create;True;0;0;False;0;None;280dcd80881f38d4c8800b3942247899;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;15;-1695.007,-604.9055;Float;False;ScreenColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GlobalArrayNode;90;914.356,283.8618;Float;False;ColorsArray;3;10;1;True;False;0;1;False;Object;90;4;0;INT;0;False;2;INT;0;False;1;INT;0;False;3;INT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;91;972.007,462.9622;Float;False;15;ScreenColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;610.0445,-164.5777;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenColorNode;6;-2364.703,-475.9968;Float;False;Global;_GrabScreen0;Grab Screen 0;0;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-646.2321,-25.91884;Float;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;False;0;0.1140975,0.1018156,0.1509434,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;66;786.5942,-209.8713;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;37.19648,-267.5979;Float;False;15;ScreenColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.HSVToRGBNode;61;1165.83,-247.0246;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;69;1001.111,-32.63577;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-189.0451,-205.7332;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GradientSampleNode;9;-1266.113,160.5392;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;73;307.5942,-425.8713;Float;False;Property;_HueStep;HueStep;1;0;Create;True;0;0;False;0;20;16.32;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;70;971.1108,-321.6359;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-1514.731,-336.8736;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.299;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;78;-1535.114,151.3914;Float;False;1;2;2;0,0,0,0.1295949;1,1,1,0.5120012;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.CustomExpressionNode;89;1400.455,270.4698;Float;False;float4 bestColor = float4(0, 0, 0, 0)@$float3 bestDiff = float3(1000, 1000, 1000)@$for (float i = 0@ i < 10 @ i++) {$    float4 palCol = ColorsArray[i]@$    float3 diff = abs(ScreenColor.rgb -  palCol.rgb)@$    if (length(diff) < length(bestDiff)) {$        bestDiff = diff@$        bestColor = palCol@$    }$}$return bestColor@;3;False;2;True;Colors;FLOAT4;0,0,0,0;In;;Float;False;True;ScreenColor;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;True;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;67;645.5611,-31.34216;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-1274.585,-180.1833;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-1494.731,-195.8736;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.587;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;68;798.1108,-15.63577;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;65;965.5942,-165.8713;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;16;1087.225,40.62267;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-1561.297,-71.91978;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.114;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;60;299.1968,-261.5979;Float;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RoundOpNode;72;801.1108,-374.6359;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;320.806,-111.5197;Float;False;Property;_SaturationStep;SaturationStep;2;0;Create;True;0;0;False;0;20;7.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;615.5611,-320.3423;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-1054.998,-133.635;Float;False;GreyScaleScreen;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;63;330.5902,33.0484;Float;False;Property;_ValueStep;ValueStep;0;0;Create;True;0;0;False;0;2.5;2.62;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;93;1800.37,-29.37505;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;PostProcessComic;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;15;0;95;0
WireConnection;64;0;60;2
WireConnection;64;1;77;0
WireConnection;66;0;64;0
WireConnection;61;0;70;0
WireConnection;61;1;65;0
WireConnection;61;2;69;0
WireConnection;69;0;68;0
WireConnection;69;1;63;0
WireConnection;7;0;6;0
WireConnection;9;0;78;0
WireConnection;9;1;14;0
WireConnection;70;0;72;0
WireConnection;70;1;73;0
WireConnection;11;0;6;1
WireConnection;89;0;90;0
WireConnection;89;1;91;0
WireConnection;67;0;60;3
WireConnection;67;1;63;0
WireConnection;14;0;11;0
WireConnection;14;1;12;0
WireConnection;14;2;13;0
WireConnection;12;0;6;2
WireConnection;68;0;67;0
WireConnection;65;0;66;0
WireConnection;65;1;77;0
WireConnection;16;0;19;0
WireConnection;16;1;61;0
WireConnection;16;2;9;1
WireConnection;13;0;6;3
WireConnection;60;0;59;0
WireConnection;72;0;71;0
WireConnection;71;0;60;1
WireConnection;71;1;73;0
WireConnection;17;0;14;0
WireConnection;93;2;89;0
ASEEND*/
//CHKSM=809067E4FF2D9D4BAF8CB1AFCAD3D2F4ADFB23B8