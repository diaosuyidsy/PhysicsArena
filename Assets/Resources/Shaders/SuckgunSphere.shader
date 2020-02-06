// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SuckgunSphere"
{
	Properties
	{
		_Texture0("Texture 0", 2D) = "white" {}
		_Texture1("Texture 1", 2D) = "white" {}
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		_OffsetPower("OffsetPower", Float) = 0.15
		_ColorPower("ColorPower", Float) = 3
		_Offset("Offset", Float) = 4
		_Color0("Color 0", Color) = (0,0,0,0)
		_TextureSample4("Texture Sample 4", 2D) = "white" {}
		_Progress("Progress", Float) = 1
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
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 screenPos;
			float2 uv_texcoord;
		};

		uniform sampler2D _Texture0;
		uniform float _Offset;
		uniform float _OffsetPower;
		uniform sampler2D _TextureSample2;
		uniform sampler2D _TextureSample4;
		uniform sampler2D _Texture1;
		uniform float _Progress;
		uniform float _ColorPower;
		uniform float4 _Color0;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 uv_TexCoord10 = v.texcoord.xy * float2( 0.5,1 );
			float2 panner11 = ( _Time.y * float2( 0,-0.8 ) + uv_TexCoord10);
			float4 tex2DNode13 = tex2Dlod( _Texture0, float4( panner11, 0, 0.0) );
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			v.vertex.xyz += ( ( tex2DNode13.r + _Offset ) * ase_worldNormal * _OffsetPower );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV22 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode22 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV22, 1.0 ) );
			float temp_output_23_0 = ( 1.0 - fresnelNode22 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float3 worldToView3 = mul( UNITY_MATRIX_V, float4( ase_worldNormal, 1 ) ).xyz;
			float2 normalizeResult5 = normalize( (worldToView3).xy );
			float fresnelNdotV7 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode7 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV7, 5.0 ) );
			float2 uv_TexCoord10 = i.uv_texcoord * float2( 0.5,1 );
			float2 panner11 = ( _Time.y * float2( 0,-0.8 ) + uv_TexCoord10);
			float2 panner36 = ( _Time.y * float2( 0,-1.5 ) + panner11);
			float2 appendResult26 = (float2(( ( ( temp_output_23_0 * temp_output_23_0 ) * saturate( ( tex2D( _TextureSample4, ase_screenPos.xy ).r + tex2D( _Texture0, ( ( normalizeResult5 * fresnelNode7 * 1.0 ) + panner11 ) ).r ) ) * tex2D( _Texture1, panner36 ).r * 4.0 ) + fresnelNode22 ) , 0.0));
			o.Emission = ( tex2D( _TextureSample2, ( appendResult26 + _Progress + -0.4 ) ) * _ColorPower * _Color0 ).rgb;
			float4 tex2DNode13 = tex2D( _Texture0, panner11 );
			o.Alpha = saturate( ( tex2DNode13.r + 1.0 ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
0;1136.8;1524;520;74.24274;-300.8824;1;True;False
Node;AmplifyShaderEditor.WorldNormalVector;1;-1451.421,78.05397;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;3;-1214.421,79.05397;Float;False;World;View;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;4;-973.7096,74.46701;Float;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;37;-1124.849,664.4345;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;7;-979.6222,202.159;Float;False;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-1062.465,522.8147;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.5,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;8;-900.1675,435.4968;Float;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;5;-756.7096,81.46701;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;11;-785.6529,551.2933;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.8;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-523.7096,116.467;Float;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-253.5017,271.8331;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;14;-502.4522,681.8792;Float;True;Property;_Texture0;Texture 0;0;0;Create;True;0;0;False;0;1746d72c8cbcebf4ebe6ab93898e2f61;300baea7454f4464e82411127a4aa6f3;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;38;-329.2307,132.3514;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-73.66679,318.0785;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;22;-294.8236,-68.11446;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;39;-127.2307,136.3514;Float;True;Property;_TextureSample4;Texture Sample 4;7;0;Create;True;0;0;False;0;None;6899e2f8659fc544dbddd620ab88933a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;31;218.6933,238.8652;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;84.1488,-22.70765;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;34;-503.9814,912.3549;Float;True;Property;_Texture1;Texture 1;1;0;Create;True;0;0;False;0;1746d72c8cbcebf4ebe6ab93898e2f61;c4a9b440793e5804eb233a66534a0af8;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.PannerNode;36;-464.6508,563.9417;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-1.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;274.136,-50.51094;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;35;-53.21202,932.377;Float;True;Property;_TextureSample3;Texture Sample 3;8;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;40;349.9014,195.0648;Float;False;Constant;_Float1;Float 1;8;0;Create;True;0;0;False;0;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;20;317.5788,323.1243;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;524.2693,83.89291;Float;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;678.9496,132.5894;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;810.501,455.1192;Float;False;Constant;_Float3;Float 3;10;0;Create;True;0;0;False;0;-0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;26;948.1737,233.1559;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;44;590.3829,374.7634;Float;False;Property;_Progress;Progress;8;0;Create;True;0;0;False;0;1;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;121.264,852.4589;Float;False;Property;_Offset;Offset;5;0;Create;True;0;0;False;0;4;0.42;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;595.1982,627.0863;Float;False;Constant;_OpacityProcess;OpacityProcess;8;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-79.11314,622.3813;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;46;998.6521,326.747;Float;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;18;376.264,968.4589;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;42;850.0353,548.8151;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;1137.565,520.1993;Float;False;Property;_Color0;Color 0;6;0;Create;True;0;0;False;0;0,0,0,0;0.8490566,0.8490566,0.8490566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;29;1120.207,443.8236;Float;False;Property;_ColorPower;ColorPower;4;0;Create;True;0;0;False;0;3;1.63;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;452.264,1132.459;Float;False;Property;_OffsetPower;OffsetPower;3;0;Create;True;0;0;False;0;0.15;0.29;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;27;1150.666,228.8844;Float;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;False;0;None;427fdefda3c71f24899927ca878b2b6a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;15;331.264,735.4589;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;43;1085.853,691.7667;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;1481.256,333.8889;Float;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;659.264,768.4589;Float;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1707.897,290.1802;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;SuckgunSphere;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;1;0
WireConnection;4;0;3;0
WireConnection;5;0;4;0
WireConnection;11;0;10;0
WireConnection;11;1;37;0
WireConnection;6;0;5;0
WireConnection;6;1;7;0
WireConnection;6;2;8;0
WireConnection;9;0;6;0
WireConnection;9;1;11;0
WireConnection;12;0;14;0
WireConnection;12;1;9;0
WireConnection;39;1;38;0
WireConnection;31;0;39;1
WireConnection;31;1;12;1
WireConnection;23;0;22;0
WireConnection;36;0;11;0
WireConnection;36;1;37;0
WireConnection;24;0;23;0
WireConnection;24;1;23;0
WireConnection;35;0;34;0
WireConnection;35;1;36;0
WireConnection;20;0;31;0
WireConnection;21;0;24;0
WireConnection;21;1;20;0
WireConnection;21;2;35;1
WireConnection;21;3;40;0
WireConnection;25;0;21;0
WireConnection;25;1;22;0
WireConnection;26;0;25;0
WireConnection;13;0;14;0
WireConnection;13;1;11;0
WireConnection;46;0;26;0
WireConnection;46;1;44;0
WireConnection;46;2;47;0
WireConnection;42;0;13;1
WireConnection;42;1;41;0
WireConnection;27;1;46;0
WireConnection;15;0;13;1
WireConnection;15;1;16;0
WireConnection;43;0;42;0
WireConnection;28;0;27;0
WireConnection;28;1;29;0
WireConnection;28;2;30;0
WireConnection;17;0;15;0
WireConnection;17;1;18;0
WireConnection;17;2;19;0
WireConnection;0;2;28;0
WireConnection;0;9;43;0
WireConnection;0;11;17;0
ASEEND*/
//CHKSM=010C002534D9EA44215B543AEBADC8B4FCBED8EA