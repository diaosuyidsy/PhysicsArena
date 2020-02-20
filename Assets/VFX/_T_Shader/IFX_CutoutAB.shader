// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "QFX/IFX/Cutout/CutoutAB"
{
	Properties
	{
		_TexPower("Tex Power", Float) = 1
		_AlphaCutout("Alpha Cutout", Float) = 0
		[HDR]_DissolveColor("Dissolve Color", Color) = (1,1,1,1)
		[HDR]_Color0("Color 0", Color) = (1,1,1,1)
		_MainTilingOffset("Main Tiling Offset", Vector) = (1,1,0,0)
		_NoiseTex("Noise Tex", 2D) = "white" {}
		_DissolveEdgeWidth("Dissolve Edge Width", Range( 0 , 1)) = 0
		_NoiseSpeed("Noise Speed", Vector) = (0,0,0,0)
		_MaskClipValue("Mask Clip Value", Float) = 0.5
		_Texture0("Texture 0", 2D) = "white" {}
		[HideInInspector] _tex3coord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float3 uv_tex3coord;
			float2 uv2_texcoord2;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _DissolveEdgeWidth;
		uniform float _AlphaCutout;
		uniform sampler2D _NoiseTex;
		uniform float2 _NoiseSpeed;
		uniform float4 _NoiseTex_ST;
		uniform float4 _DissolveColor;
		uniform sampler2D _Texture0;
		uniform float4 _MainTilingOffset;
		uniform float _TexPower;
		uniform float4 _Color0;
		uniform float _MaskClipValue;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv1_NoiseTex = i.uv2_texcoord2 * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
			float2 panner71 = ( _Time.y * _NoiseSpeed + uv1_NoiseTex);
			float OpacityMask27 = ( ( 1.0 - ( _AlphaCutout + i.uv_tex3coord.z ) ) - tex2D( _NoiseTex, panner71 ).r );
			float2 uv_TexCoord95 = i.uv_texcoord * (_MainTilingOffset).xy + (_MainTilingOffset).zw;
			float4 tex2DNode82 = tex2D( _Texture0, uv_TexCoord95 );
			float4 Emission22 = ( pow( tex2DNode82.r , _TexPower ) * _Color0 * i.vertexColor );
			float4 temp_cast_0 = (_MaskClipValue).xxxx;
			clip( ( tex2DNode82 * OpacityMask27 ) - temp_cast_0);
			o.Albedo = (( _DissolveEdgeWidth > OpacityMask27 ) ? _DissolveColor :  Emission22 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
212;368;1282;1011;1551.551;876.3112;2.297763;False;False
Node;AmplifyShaderEditor.Vector4Node;92;-1405.881,-373.0578;Float;False;Property;_MainTilingOffset;Main Tiling Offset;4;0;Create;True;0;0;False;0;1,1,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;97;-456.6043,1297.642;Float;False;733.1929;322.7531;Cutout;4;46;15;43;55;;0.04830408,1,0,1;0;0
Node;AmplifyShaderEditor.ComponentMaskNode;94;-1142.838,-325.8071;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;93;-1144.57,-420.5862;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TimeNode;72;-595.2852,1145.245;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-353.2013,1347.642;Float;False;Property;_AlphaCutout;Alpha Cutout;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-406.6043,1441.395;Float;False;0;-1;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;61;-597.7073,852.5314;Float;False;1;12;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;70;-591.2852,1009.245;Float;False;Property;_NoiseSpeed;Noise Speed;7;0;Create;True;0;0;False;0;0,0;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;95;-912.0961,-399.1504;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;101;-912.3255,-627.1432;Float;True;Property;_Texture0;Texture 0;9;0;Create;True;0;0;False;0;None;cbc1d63f141bf78428c91db12ed54e18;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-128.5686,-456.9718;Float;False;Property;_TexPower;Tex Power;0;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;71;-344.2852,991.2446;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;43;-131.3144,1352.586;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;82;-647.9061,-508.0222;Float;True;Property;_TextureSample0;Texture Sample 0;7;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;56;89.47234,-675.9882;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;-132.9651,963.6368;Float;True;Property;_NoiseTex;Noise Tex;5;0;Create;True;0;0;False;0;None;9900a1b5eb786be4d8e13e5e0250308d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;55;89.58849,1353.16;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;102;83.54806,-512.8658;Float;False;Property;_Color0;Color 0;3;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;59;83.34039,-278.1558;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;362.9343,-525.8784;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;19;306.6187,969.66;Float;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;532.8392,-531.2465;Float;True;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;570.7059,967.0314;Float;True;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;30;-459.5061,286.4218;Float;True;22;Emission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-528.0078,21.10897;Float;False;Property;_DissolveEdgeWidth;Dissolve Edge Width;6;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;-572.9563,604.4929;Float;True;27;OpacityMask;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;64;-461.6236,109.2715;Float;False;Property;_DissolveColor;Dissolve Color;2;1;[HDR];Create;True;0;0;False;0;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareGreater;63;-69.56338,240.4235;Float;True;4;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-269.1144,594.6114;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;86;92.66338,542.9013;Float;True;Property;_MaskClipValue;Mask Clip Value;8;0;Create;True;0;0;False;0;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;65;508.4918,-66.39173;Float;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClipNode;78;323.99,240.3333;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;96;221.8528,-68.63082;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;99;548.7306,241.1558;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;QFX/IFX/Cutout/CutoutAB;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;94;0;92;0
WireConnection;93;0;92;0
WireConnection;95;0;93;0
WireConnection;95;1;94;0
WireConnection;71;0;61;0
WireConnection;71;2;70;0
WireConnection;71;1;72;2
WireConnection;43;0;15;0
WireConnection;43;1;46;3
WireConnection;82;0;101;0
WireConnection;82;1;95;0
WireConnection;56;0;82;1
WireConnection;56;1;57;0
WireConnection;12;1;71;0
WireConnection;55;0;43;0
WireConnection;18;0;56;0
WireConnection;18;1;102;0
WireConnection;18;2;59;0
WireConnection;19;0;55;0
WireConnection;19;1;12;1
WireConnection;22;0;18;0
WireConnection;27;0;19;0
WireConnection;63;0;62;0
WireConnection;63;1;26;0
WireConnection;63;2;64;0
WireConnection;63;3;30;0
WireConnection;98;0;82;0
WireConnection;98;1;26;0
WireConnection;65;0;96;0
WireConnection;78;0;63;0
WireConnection;78;1;98;0
WireConnection;78;2;86;0
WireConnection;96;0;82;0
WireConnection;96;1;26;0
WireConnection;99;0;78;0
ASEEND*/
//CHKSM=14654CAEFEB9F3BB82EE3E4A27DF8E0F091BB147