// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SuckGunCone"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_TextureSample2("Texture Sample 2", 2D) = "white" {}
		_OpacityPower("Opacity Power", Float) = 2
		[HDR]_Color0("Color 0", Color) = (0.5754717,0.5754717,0.5754717,0)
		_OffsetPower("Offset Power", Float) = 0.2
		_FresnelPower("Fresnel Power", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform sampler2D _TextureSample0;
		uniform float _OffsetPower;
		uniform sampler2D _TextureSample1;
		uniform sampler2D _TextureSample2;
		uniform float4 _TextureSample2_ST;
		uniform float4 _Color0;
		uniform float _FresnelPower;
		uniform float _OpacityPower;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 panner1 = ( _Time.y * float2( 0,1 ) + v.texcoord.xy);
			float Noise5 = tex2Dlod( _TextureSample0, float4( panner1, 0, 0.0) ).r;
			float3 ase_worldNormal = UnityObjectToWorldNormal( v.normal );
			v.vertex.xyz += ( Noise5 * ase_worldNormal * _OffsetPower );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner1 = ( _Time.y * float2( 0,1 ) + i.uv_texcoord);
			float Noise5 = tex2D( _TextureSample0, panner1 ).r;
			float2 appendResult6 = (float2(( Noise5 * 0.7 ) , 0.0));
			float2 uv_TextureSample2 = i.uv_texcoord * _TextureSample2_ST.xy + _TextureSample2_ST.zw;
			float4 tex2DNode11 = tex2D( _TextureSample2, uv_TextureSample2 );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV14 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode14 = ( 0.0 + 1.0 * pow( 1.0 - fresnelNdotV14, _FresnelPower ) );
			float temp_output_15_0 = ( 1.0 - fresnelNode14 );
			o.Emission = ( tex2D( _TextureSample1, ( appendResult6 + float2( 0.2,0 ) ) ) * tex2DNode11.r * _Color0 * temp_output_15_0 ).rgb;
			o.Alpha = saturate( ( ( Noise5 + 0.2 ) * _OpacityPower * tex2DNode11.r * temp_output_15_0 ) );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
-47.2;1414.4;1524;487;2940.483;343.0558;1.3;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;3;-2934.94,-70.79682;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;-2934.94,-210.7968;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;1;-2674.081,-146.0502;Float;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,1;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-2459.94,-167.7968;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;f65f691a17b47ea43a4c3c3cc3e5ef49;f65f691a17b47ea43a4c3c3cc3e5ef49;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;5;-2115.631,-185.2259;Float;False;Noise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;7;-1927.389,-243.3155;Float;False;5;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;-1891.167,-133.4265;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-1706.167,-272.4265;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1992.763,635.2604;Float;False;Property;_FresnelPower;Fresnel Power;6;0;Create;True;0;0;False;0;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;6;-1808.57,-17.51219;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;17;-1379.526,443.256;Float;False;5;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;14;-1733.024,521.4587;Float;True;Standard;WorldNormal;ViewDir;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1087.13,540.6472;Float;False;Property;_OpacityPower;Opacity Power;3;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;35;-1582.029,-35.86879;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.2,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;-1344.677,100.1402;Float;True;Property;_TextureSample2;Texture Sample 2;2;0;Create;True;0;0;False;0;f9f0c5e09514cb14aa7c0663a7263f8c;f9f0c5e09514cb14aa7c0663a7263f8c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-1111.746,472.2356;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;15;-1265.306,635.9818;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;22;-1199.389,833.7912;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;12;-1325.081,292.9101;Float;False;Property;_Color0;Color 0;4;1;[HDR];Create;True;0;0;False;0;0.5754717,0.5754717,0.5754717,0;3.891908,5.124749,4.590518,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;-1046.19,952.8612;Float;False;Property;_OffsetPower;Offset Power;5;0;Create;True;0;0;False;0;0.2;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1367.264,-136.9633;Float;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;False;0;427fdefda3c71f24899927ca878b2b6a;427fdefda3c71f24899927ca878b2b6a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-782.1191,455.23;Float;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;20;-1056.201,744.3506;Float;False;5;Noise;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;19;-568.8049,440.6962;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-930.4537,-30.31617;Float;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-789.3887,735.7912;Float;False;3;3;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;SuckGunCone;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;1;0;2;0
WireConnection;1;1;3;0
WireConnection;4;1;1;0
WireConnection;5;0;4;1
WireConnection;37;0;7;0
WireConnection;37;1;38;0
WireConnection;6;0;37;0
WireConnection;14;3;30;0
WireConnection;35;0;6;0
WireConnection;39;0;17;0
WireConnection;15;0;14;0
WireConnection;9;1;35;0
WireConnection;16;0;39;0
WireConnection;16;1;18;0
WireConnection;16;2;11;1
WireConnection;16;3;15;0
WireConnection;19;0;16;0
WireConnection;10;0;9;0
WireConnection;10;1;11;1
WireConnection;10;2;12;0
WireConnection;10;3;15;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;21;2;24;0
WireConnection;0;2;10;0
WireConnection;0;9;19;0
WireConnection;0;11;21;0
ASEEND*/
//CHKSM=5E54A2F075EAC75700E22DC01C11CD40B4828E82