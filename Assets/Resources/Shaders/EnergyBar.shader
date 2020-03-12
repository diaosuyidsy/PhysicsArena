// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "EnergyBar"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Energy("Energy", Range( 0 , 1)) = 0
		[Toggle]_IsDissove("IsDissove", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustomLighting keepalpha noshadow 
		struct Input
		{
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

		uniform float _Energy;
		uniform float _IsDissove;
		uniform float _Cutoff = 0.5;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			c.rgb = 0;
			c.a = 1;
			clip( lerp(1.0,step( ( 1.0 - ( i.uv_texcoord.y * 1.0 ) ) , (0.15 + (_Energy - 0.0) * (1.0 - 0.15) / (1.0 - 0.0)) ),_IsDissove) - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float4 color5 = IsGammaSpace() ? float4(1,0.8798681,0,0) : float4(1,0.7481608,0,0);
			float4 color1 = IsGammaSpace() ? float4(0,1,0.01685119,0) : float4(0,1,0.001304272,0);
			float4 lerpResult2 = lerp( color5 , color1 , _Energy);
			float4 color17 = IsGammaSpace() ? float4(1,0.1559573,0.1084906,0) : float4(1,0.02100203,0.01139139,0);
			float4 ifLocalVar16 = 0;
			if( _Energy <= 0.0 )
				ifLocalVar16 = color17;
			else
				ifLocalVar16 = lerpResult2;
			o.Emission = ( ifLocalVar16 * 2.0 ).rgb;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
-244.8;1233.6;1524;505;1501.376;273.1579;1.246841;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-987.0338,373.2138;Float;True;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;1;-967.7583,-54.56194;Float;False;Constant;_Color0;Color 0;0;0;Create;True;0;0;False;0;0,1,0.01685119,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;5;-963.2606,-241.0546;Float;False;Constant;_Color1;Color 1;1;0;Create;True;0;0;False;0;1,0.8798681,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-1094.536,178.9216;Float;False;Property;_Energy;Energy;1;0;Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;9;-744.0036,399.8294;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;2;-634.271,-208.5023;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;20;-458.1117,278.3764;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.15;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;12;-486.6558,479.7668;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;17;-579.261,60.31036;Float;False;Constant;_Color2;Color 2;3;0;Create;True;0;0;False;0;1,0.1559573,0.1084906,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;10;-122.521,400.9414;Float;True;2;0;FLOAT;0;False;1;FLOAT;-0.06;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-182.5022,75.6591;Float;False;Constant;_EmissionPower;EmissionPower;3;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-95.18225,222.6595;Float;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;16;-269.1543,-122.4076;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;125.9612,-47.72627;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;18;137.6609,241.7176;Float;False;Property;_IsDissove;IsDissove;2;0;Create;True;0;0;False;0;1;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;705.0205,18.35711;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;EnergyBar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;8;2
WireConnection;2;0;5;0
WireConnection;2;1;1;0
WireConnection;2;2;3;0
WireConnection;20;0;3;0
WireConnection;12;0;9;0
WireConnection;10;0;12;0
WireConnection;10;1;20;0
WireConnection;16;0;3;0
WireConnection;16;2;2;0
WireConnection;16;3;17;0
WireConnection;16;4;17;0
WireConnection;15;0;16;0
WireConnection;15;1;14;0
WireConnection;18;0;19;0
WireConnection;18;1;10;0
WireConnection;0;2;15;0
WireConnection;0;10;18;0
ASEEND*/
//CHKSM=8672F7F8A055146BDAA1D364FB414A1BC9CE910B