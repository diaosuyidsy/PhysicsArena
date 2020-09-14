// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FresnelDistortion"
{
	Properties
	{
		_FresnelScale("Fresnel Scale", Range( 0 , 1)) = 0.510905
		_MainTilingOffset("Main Tiling Offset", Vector) = (1,1,0,0)
		_FresnelPower("Fresnel Power", Range( 0 , 5)) = 2
		_MainSpeed("Main Speed", Vector) = (0.1,0.1,0,0)
		_MainMultiply("Main Multiply", Range( 0 , 1)) = 0
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_Color0("Color 0", Color) = (1,1,1,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
			float4 vertexColor : COLOR;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float4 _Color0;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _MainMultiply;
		uniform sampler2D _TextureSample1;
		uniform float2 _MainSpeed;
		uniform float4 _MainTilingOffset;
		uniform float _FresnelScale;
		uniform float _FresnelPower;


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


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TexCoord67 = i.uv_texcoord * (_MainTilingOffset).xy + (_MainTilingOffset).zw;
			float2 panner96 = ( 1.0 * _Time.y * ( _Time.x * _MainSpeed ) + uv_TexCoord67);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor80 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( ( _MainMultiply * tex2D( _TextureSample1, panner96 ) ) + ase_grabScreenPosNorm ).rg);
			float4 temp_output_98_0 = ( ( _Color0 + screenColor80 ) * i.vertexColor );
			o.Emission = (temp_output_98_0).rgb;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV62 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode62 = ( 0.0 + _FresnelScale * pow( 1.0 - fresnelNdotV62, _FresnelPower ) );
			o.Alpha = saturate( ( (temp_output_98_0).a * fresnelNode62 ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
-249.6;1560;1524;416;743.1917;-104.1756;1;True;False
Node;AmplifyShaderEditor.Vector4Node;89;-2882.69,-30.24262;Float;False;Property;_MainTilingOffset;Main Tiling Offset;1;0;Create;True;0;0;False;0;1,1,0,0;3,3,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;91;-2621.274,10.49515;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;90;-2621.377,-77.771;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;73;-2702.875,285.2549;Float;False;Property;_MainSpeed;Main Speed;3;0;Create;True;0;0;False;0;0.1,0.1;0,-0.01;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TimeNode;71;-2711.93,145.3965;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-2472.46,226.8966;Float;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;67;-2328.761,-23.49689;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;96;-2074.246,64.36356;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;70;-1829.417,-126.1645;Float;False;Property;_MainMultiply;Main Multiply;4;0;Create;True;0;0;False;0;0;0.028;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;75;-1848.023,-22.60529;Float;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;None;bdb9b29dafca0434f852959cb6202afc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-1543.138,-75.50249;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GrabScreenPosition;76;-1334.079,20.25392;Float;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;79;-1007.47,-66.45084;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ScreenColorNode;80;-852.9886,26.14617;Float;False;Global;_GrabScreen1;Grab Screen 1;0;0;Create;True;0;0;False;0;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;105;-878.9346,-277.0995;Float;False;Property;_Color0;Color 0;6;0;Create;True;0;0;False;0;1,1,1,1;0,0,0,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;103;-1072.144,472.8016;Float;False;861.978;314.1815;Fresnel;4;62;65;66;95;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-626.6963,-86.83135;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VertexColorNode;97;-636.4104,76.74129;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;65;-1015.958,565.6268;Float;False;Property;_FresnelScale;Fresnel Scale;0;0;Create;True;0;0;False;0;0.510905;0.708;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-1022.144,671.983;Float;False;Property;_FresnelPower;Fresnel Power;2;0;Create;True;0;0;False;0;2;3.06;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;98;-438.5561,-70.63257;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;62;-705.1653,522.8016;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;94;-226.7055,-35.26563;Float;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-383.1698,501.2682;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;93;-219.7932,-141.1499;Float;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SaturateNode;63;177.757,166.5219;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;104;559.0933,29.08446;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;FresnelDistortion;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;91;0;89;0
WireConnection;90;0;89;0
WireConnection;68;0;71;1
WireConnection;68;1;73;0
WireConnection;67;0;90;0
WireConnection;67;1;91;0
WireConnection;96;0;67;0
WireConnection;96;2;68;0
WireConnection;75;1;96;0
WireConnection;72;0;70;0
WireConnection;72;1;75;0
WireConnection;79;0;72;0
WireConnection;79;1;76;0
WireConnection;80;0;79;0
WireConnection;87;0;105;0
WireConnection;87;1;80;0
WireConnection;98;0;87;0
WireConnection;98;1;97;0
WireConnection;62;2;65;0
WireConnection;62;3;66;0
WireConnection;94;0;98;0
WireConnection;95;0;94;0
WireConnection;95;1;62;0
WireConnection;93;0;98;0
WireConnection;63;0;95;0
WireConnection;104;2;93;0
WireConnection;104;9;63;0
ASEEND*/
//CHKSM=034BD8662FCF8FD1F0B673E5F0F607DA701D5A58