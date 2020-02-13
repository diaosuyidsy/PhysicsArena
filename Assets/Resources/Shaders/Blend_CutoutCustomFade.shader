// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Blend_CutoutFade"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_TexPower("Tex Power", Float) = 1
		_AlphaCutout("Alpha Cutout", Float) = 0
		[HDR]_DissolveColor("Dissolve Color", Color) = (1,1,1,1)
		_MainTilingOffset("Main Tiling Offset", Vector) = (1,1,0,0)
		_Texture("Texture", 2D) = "white" {}
		_NoiseTex("NoiseTex", 2D) = "white" {}
		_DissolveEdgeWidth("Dissolve Edge Width", Range( 0 , 1)) = 0
		_NoiseSpeed("Noise Speed", Vector) = (0,0,0,0)
		[HDR]_Color("Color", Color) = (0,0,0,0)
		[HideInInspector] _tex3coord( "", 2D ) = "white" {}
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
		#pragma surface surf Standard keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 uv_tex3coord;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _DissolveEdgeWidth;
		uniform float _AlphaCutout;
		uniform sampler2D _NoiseTex;
		uniform float2 _NoiseSpeed;
		uniform float4 _DissolveColor;
		uniform sampler2D _Texture;
		uniform float4 _MainTilingOffset;
		uniform float _TexPower;
		uniform float4 _Color;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner33 = ( _Time.y * _NoiseSpeed + i.uv_texcoord);
			float OpacityMask47 = ( ( 1.0 - ( _AlphaCutout + i.uv_tex3coord.z ) ) - tex2D( _NoiseTex, panner33 ).r );
			float2 uv_TexCoord45 = i.uv_texcoord * (_MainTilingOffset).xy + (_MainTilingOffset).zw;
			float4 tex2DNode34 = tex2D( _Texture, uv_TexCoord45 );
			float4 Emission41 = ( pow( tex2DNode34.r , _TexPower ) * _Color * i.vertexColor );
			o.Emission = (( _DissolveEdgeWidth > OpacityMask47 ) ? _DissolveColor :  Emission41 ).rgb;
			o.Alpha = 1;
			clip( ( tex2DNode34.a * OpacityMask47 * i.vertexColor.a ) - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
-2.4;420.8;1524;516;2649.238;484.0349;1.947574;True;False
Node;AmplifyShaderEditor.Vector4Node;28;-1435.554,-703.2037;Float;False;Property;_MainTilingOffset;Main Tiling Offset;4;0;Create;True;0;0;False;0;1,1,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;53;-1172.51,-655.953;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;52;-1174.242,-750.7321;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;49;-834.3998,1308.023;Float;False;0;-1;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;51;-1025.503,719.1589;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;31;-1023.081,1011.873;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;30;-780.9968,1214.27;Float;False;Property;_AlphaCutout;Alpha Cutout;2;0;Create;True;0;0;False;0;0;-1.04;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;29;-1019.081,875.8727;Float;False;Property;_NoiseSpeed;Noise Speed;8;0;Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;45;-941.7681,-729.2965;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;34;-677.5782,-838.1681;Float;True;Property;_Texture;Texture;5;0;Create;True;0;0;False;0;None;177d8f53a38d0984089f0b9aa95f0ae0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-556.364,-590.3441;Float;False;Property;_TexPower;Tex Power;1;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-559.1099,1219.214;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;33;-772.0806,857.8723;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;46;-382.5282,-644.5521;Float;False;Property;_Color;Color;9;1;[HDR];Create;True;0;0;False;0;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;36;-344.455,-411.5282;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;50;-560.7605,830.2644;Float;True;Property;_NoiseTex;NoiseTex;6;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;38;-338.2069,1219.788;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;37;-338.3231,-809.3605;Float;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;39;-121.1767,836.2876;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-64.86107,-659.2507;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;41;105.0437,-664.6189;Float;True;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;141.9105,833.6591;Float;True;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-950.5289,292.551;Float;False;47;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;48;-887.3015,153.0495;Float;False;41;Emission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;43;-955.8032,-112.2635;Float;False;Property;_DissolveEdgeWidth;Dissolve Edge Width;7;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-889.4189,-24.1009;Float;False;Property;_DissolveColor;Dissolve Color;3;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareGreater;44;-40.91954,9.809755;Float;False;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-48.16023,216.1562;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;214.4561,15.88564;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;Blend_CutoutFade;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;53;0;28;0
WireConnection;52;0;28;0
WireConnection;45;0;52;0
WireConnection;45;1;53;0
WireConnection;34;1;45;0
WireConnection;32;0;30;0
WireConnection;32;1;49;3
WireConnection;33;0;51;0
WireConnection;33;2;29;0
WireConnection;33;1;31;2
WireConnection;50;1;33;0
WireConnection;38;0;32;0
WireConnection;37;0;34;1
WireConnection;37;1;35;0
WireConnection;39;0;38;0
WireConnection;39;1;50;1
WireConnection;40;0;37;0
WireConnection;40;1;46;0
WireConnection;40;2;36;0
WireConnection;41;0;40;0
WireConnection;47;0;39;0
WireConnection;44;0;43;0
WireConnection;44;1;54;0
WireConnection;44;2;42;0
WireConnection;44;3;48;0
WireConnection;55;0;34;4
WireConnection;55;1;54;0
WireConnection;55;2;36;4
WireConnection;0;2;44;0
WireConnection;0;10;55;0
ASEEND*/
//CHKSM=D17399A15007A318DED5EAA1BD8939A00E8E9A28