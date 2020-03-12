// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Add_Base"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Flow("Flow", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_DistortionSpeedXYPowerZ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)
		_Emission("Emission", Float) = 2
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		[Toggle]_Usecenterglow("Use center glow?", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _tex3coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite Off
		Blend One One
		
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustomLighting keepalpha addshadow fullforwardshadows 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float3 uv_tex3coord;
			float4 vertexColor : COLOR;
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

		uniform float _Usecenterglow;
		uniform sampler2D _Texture;
		uniform float4 _SpeedMainTexUVNoiseZW;
		uniform float4 _Texture_ST;
		uniform sampler2D _Flow;
		uniform float4 _DistortionSpeedXYPowerZ;
		uniform float4 _Flow_ST;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform sampler2D _Noise;
		uniform float4 _Noise_ST;
		uniform float4 _Color;
		uniform float _Emission;

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			c.rgb = 0;
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
			float2 appendResult21 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
			float2 uv0_Texture = i.uv_texcoord * _Texture_ST.xy + _Texture_ST.zw;
			float2 panner107 = ( 1.0 * _Time.y * appendResult21 + uv0_Texture);
			float2 appendResult100 = (float2(_DistortionSpeedXYPowerZ.x , _DistortionSpeedXYPowerZ.y));
			float3 uv0_Flow = i.uv_tex3coord;
			uv0_Flow.xy = i.uv_tex3coord.xy * _Flow_ST.xy + _Flow_ST.zw;
			float2 panner110 = ( 1.0 * _Time.y * appendResult100 + (uv0_Flow).xy);
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode33 = tex2D( _Mask, uv_Mask );
			float Flowpower102 = _DistortionSpeedXYPowerZ.z;
			float4 tex2DNode13 = tex2D( _Texture, ( panner107 - ( (( tex2D( _Flow, panner110 ) * tex2DNode33 )).rg * Flowpower102 ) ) );
			float2 appendResult22 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
			float2 uv0_Noise = i.uv_texcoord * _Noise_ST.xy + _Noise_ST.zw;
			float2 panner108 = ( 1.0 * _Time.y * appendResult22 + uv0_Noise);
			float4 tex2DNode14 = tex2D( _Noise, panner108 );
			float4 temp_output_30_0 = ( tex2DNode13 * tex2DNode14 * _Color * i.vertexColor * tex2DNode13.a * tex2DNode14.a * _Color.a * i.vertexColor.a );
			float4 temp_cast_0 = ((1.0 + (uv0_Flow.z - 0.0) * (0.0 - 1.0) / (1.0 - 0.0))).xxxx;
			float4 clampResult38 = clamp( ( tex2DNode33 - temp_cast_0 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			float4 clampResult40 = clamp( ( tex2DNode33 * clampResult38 ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
			o.Emission = ( lerp(temp_output_30_0,( temp_output_30_0 * clampResult40 ),_Usecenterglow) * _Emission ).rgb;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
6.4;1080.8;1524;796;1988.576;341.6604;1.564995;True;False
Node;AmplifyShaderEditor.CommentaryNode;104;-4130.993,490.5418;Float;False;1910.996;537.6462;Texture distortion;12;91;33;100;102;99;94;95;103;92;59;98;110;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector4Node;99;-3968.293,619.481;Float;False;Property;_DistortionSpeedXYPowerZ;Distortion Speed XY Power Z;6;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;98;-3920.299,848.9976;Float;False;0;91;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;100;-3535.482,654.5021;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;59;-3583.603,566.496;Float;False;True;True;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;110;-3339.196,596.5295;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;33;-3146.373,763.0061;Float;True;Property;_Mask;Mask;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;91;-3152.937,567.9764;Float;True;Property;_Flow;Flow;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;109;-3401.27,-330.4436;Float;False;1037.896;533.6285;Textures movement;7;107;108;29;21;89;22;15;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;102;-3556.945,748.0421;Float;False;Flowpower;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;-2762.212,550.0183;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node;15;-3351.27,-101.4007;Float;False;Property;_SpeedMainTexUVNoiseZW;Speed MainTex U/V + Noise Z/W;5;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;94;-2609.926,543.6367;Float;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-2856.788,-280.4436;Float;False;0;13;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;21;-2778.501,-153.1786;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCRemapNode;36;-2530.289,1355.094;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;103;-2605.07,630.9626;Float;False;102;Flowpower;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2388.997,542.6455;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;37;-2289.906,1280.763;Float;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;-2766.722,70.18491;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;107;-2570.374,-239.5098;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;89;-2861.858,-55.04038;Float;False;0;14;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;38;-2130.64,1280.587;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;108;-2577.237,-21.63752;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;96;-1989.684,-41.77601;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-1937.593,1156.593;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;13;-1803.192,-66.2159;Float;True;Property;_Texture;Texture;1;0;Create;True;0;0;False;0;None;60d224affeaecf84ead5a8b24c6c9995;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;32;-1670.612,486.0577;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;14;-1804.579,119.2214;Float;True;Property;_Noise;Noise;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;31;-1728.612,316.0578;Float;False;Property;_Color;Color;8;0;Create;True;0;0;False;0;0.5,0.5,0.5,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-1187.357,127.2037;Float;False;8;8;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;40;-1764.275,1143.857;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-896.1669,248.9071;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-621.877,229.5624;Float;False;Property;_Emission;Emission;7;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;90;-697.314,128.5203;Float;False;Property;_Usecenterglow;Use center glow?;9;0;Create;True;0;0;False;0;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;72;-1580.242,1135.946;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-461.6268,132.2673;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;111;-275.6836,109.062;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;Add_Base;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Custom;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;4;1;False;-1;1;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;100;0;99;1
WireConnection;100;1;99;2
WireConnection;59;0;98;0
WireConnection;110;0;59;0
WireConnection;110;2;100;0
WireConnection;91;1;110;0
WireConnection;102;0;99;3
WireConnection;92;0;91;0
WireConnection;92;1;33;0
WireConnection;94;0;92;0
WireConnection;21;0;15;1
WireConnection;21;1;15;2
WireConnection;36;0;98;3
WireConnection;95;0;94;0
WireConnection;95;1;103;0
WireConnection;37;0;33;0
WireConnection;37;1;36;0
WireConnection;22;0;15;3
WireConnection;22;1;15;4
WireConnection;107;0;29;0
WireConnection;107;2;21;0
WireConnection;38;0;37;0
WireConnection;108;0;89;0
WireConnection;108;2;22;0
WireConnection;96;0;107;0
WireConnection;96;1;95;0
WireConnection;39;0;33;0
WireConnection;39;1;38;0
WireConnection;13;1;96;0
WireConnection;14;1;108;0
WireConnection;30;0;13;0
WireConnection;30;1;14;0
WireConnection;30;2;31;0
WireConnection;30;3;32;0
WireConnection;30;4;13;4
WireConnection;30;5;14;4
WireConnection;30;6;31;4
WireConnection;30;7;32;4
WireConnection;40;0;39;0
WireConnection;41;0;30;0
WireConnection;41;1;40;0
WireConnection;90;0;30;0
WireConnection;90;1;41;0
WireConnection;72;0;40;0
WireConnection;51;0;90;0
WireConnection;51;1;52;0
WireConnection;111;2;51;0
ASEEND*/
//CHKSM=A7321E30A0376AA78C0647C71049FE09C42C3EC4