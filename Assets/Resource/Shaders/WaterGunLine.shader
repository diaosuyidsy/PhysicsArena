// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "WaterGunLine"
{
	Properties
	{
		_UVStretchBoost("UVStretchBoost", Float) = 2
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_Mask2("Mask2", 2D) = "white" {}
		_UVStretch("UVStretch", Range( 0 , 4)) = 0
		_Noise1ScrollSpeed("Noise1ScrollSpeed", Float) = 0.6
		_Noise2ScrollSpeed("Noise2ScrollSpeed", Float) = 0.7
		_TilingX("TilingX", Float) = 0
		_Position("Position", Vector) = (0,0,0,0)
		_Core("Core", 2D) = "white" {}
		_Progress("Progress", Range( 0 , 1)) = 1
		_Distance("Distance", Float) = 0
		_Noise1("Noise1", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,0)
		_Noise2("Noise2", 2D) = "white" {}
		_OpacityRemapMin("OpacityRemapMin", Float) = 0
		_OpacityRemapMax("OpacityRemapMax", Float) = 1.35
		_OpacityCutOff("OpacityCutOff", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_ColorStrength("ColorStrength", Float) = 1
		_OpacityBoost("OpacityBoost", Range( 0 , 4)) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Noise1;
		uniform float _UVStretch;
		uniform sampler2D _Mask2;
		uniform float4 _Mask2_ST;
		uniform float _UVStretchBoost;
		uniform float _Noise1ScrollSpeed;
		uniform sampler2D _Core;
		uniform float4 _Core_ST;
		uniform float _Progress;
		uniform sampler2D _Noise2;
		uniform float _Noise2ScrollSpeed;
		uniform float _TilingX;
		uniform float4 _Color;
		uniform float _ColorStrength;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float _OpacityBoost;
		uniform sampler2D _OpacityCutOff;
		uniform float _OpacityRemapMin;
		uniform float _OpacityRemapMax;
		uniform float3 _Position;
		uniform float _Distance;
		uniform float _Cutoff = 0.5;


		struct Gradient
		{
			int type;
			int colorsLength;
			int alphasLength;
			float4 colors[8];
			float2 alphas[8];
		};


		Gradient NewGradient(int type, int colorsLength, int alphasLength, 
		float4 colors0, float4 colors1, float4 colors2, float4 colors3, float4 colors4, float4 colors5, float4 colors6, float4 colors7,
		float2 alphas0, float2 alphas1, float2 alphas2, float2 alphas3, float2 alphas4, float2 alphas5, float2 alphas6, float2 alphas7)
		{
			Gradient g;
			g.type = type;
			g.colorsLength = colorsLength;
			g.alphasLength = alphasLength;
			g.colors[ 0 ] = colors0;
			g.colors[ 1 ] = colors1;
			g.colors[ 2 ] = colors2;
			g.colors[ 3 ] = colors3;
			g.colors[ 4 ] = colors4;
			g.colors[ 5 ] = colors5;
			g.colors[ 6 ] = colors6;
			g.colors[ 7 ] = colors7;
			g.alphas[ 0 ] = alphas0;
			g.alphas[ 1 ] = alphas1;
			g.alphas[ 2 ] = alphas2;
			g.alphas[ 3 ] = alphas3;
			g.alphas[ 4 ] = alphas4;
			g.alphas[ 5 ] = alphas5;
			g.alphas[ 6 ] = alphas6;
			g.alphas[ 7 ] = alphas7;
			return g;
		}


		float4 SampleGradient( Gradient gradient, float time )
		{
			float3 color = gradient.colors[0].rgb;
			UNITY_UNROLL
			for (int c = 1; c < 8; c++)
			{
			float colorPos = saturate((time - gradient.colors[c-1].w) / (gradient.colors[c].w - gradient.colors[c-1].w)) * step(c, (float)gradient.colorsLength-1);
			color = lerp(color, gradient.colors[c].rgb, lerp(colorPos, step(0.01, colorPos), gradient.type));
			}
			#ifndef UNITY_COLORSPACE_GAMMA
			color = half3(GammaToLinearSpaceExact(color.r), GammaToLinearSpaceExact(color.g), GammaToLinearSpaceExact(color.b));
			#endif
			float alpha = gradient.alphas[0].x;
			UNITY_UNROLL
			for (int a = 1; a < 8; a++)
			{
			float alphaPos = saturate((time - gradient.alphas[a-1].y) / (gradient.alphas[a].y - gradient.alphas[a-1].y)) * step(a, (float)gradient.alphasLength-1);
			alpha = lerp(alpha, gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), gradient.type));
			}
			return float4(color, alpha);
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			Gradient gradient166 = NewGradient( 0, 5, 2, float4( 1, 1, 1, 0.1579004 ), float4( 0.2783019, 0.8500335, 1, 0.3362631 ), float4( 0.3253653, 0.8598131, 1, 0.7719234 ), float4( 0.1745283, 0.4274954, 1, 0.8362554 ), float4( 0.3460751, 0.6315314, 0.9528302, 0.8713512 ), 0, 0, 0, float2( 1, 0 ), float2( 1, 1 ), 0, 0, 0, 0, 0, 0 );
			float2 uv_Mask2 = i.uv_texcoord * _Mask2_ST.xy + _Mask2_ST.zw;
			float temp_output_135_0 = ( i.uv_texcoord.x + ( (-1.0 + (i.uv_texcoord.y - 0.0) * (1.0 - -1.0) / (1.0 - 0.0)) * _UVStretch * ( tex2D( _Mask2, uv_Mask2 ).r + _UVStretchBoost ) ) );
			float mulTime23 = _Time.y * _Noise1ScrollSpeed;
			float2 appendResult132 = (float2(temp_output_135_0 , ( i.uv_texcoord.y + mulTime23 )));
			float2 uv_Core = i.uv_texcoord * _Core_ST.xy + _Core_ST.zw;
			float clampResult36 = clamp( ( tex2D( _Noise1, ( appendResult132 * float2( 0.2,0.2 ) ) ).r + tex2D( _Core, uv_Core ).r ) , 0.0 , 1.0 );
			float clampResult145 = clamp( _Progress , 0.0 , 1.0 );
			float mulTime9 = _Time.y * _Noise2ScrollSpeed;
			float2 appendResult131 = (float2(temp_output_135_0 , ( i.uv_texcoord.y + mulTime9 )));
			float2 appendResult164 = (float2(_TilingX , 0.2));
			float2 temp_output_159_0 = ( appendResult131 * appendResult164 );
			float clampResult140 = clamp( tex2D( _Noise2, temp_output_159_0 ).r , 0.0 , 1.0 );
			float clampResult40 = clamp( ( ( clampResult36 * clampResult145 ) - clampResult140 ) , 0.0 , 1.0 );
			float2 appendResult125 = (float2(clampResult40 , 0.0));
			o.Emission = ( SampleGradient( gradient166, appendResult125.x ) * _Color * _ColorStrength ).rgb;
			float clampResult146 = clamp( ( clampResult140 + clampResult36 ) , 0.0 , 1.0 );
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode47 = tex2D( _Mask, uv_Mask );
			float clampResult50 = clamp( ( clampResult146 * tex2DNode47.r * _OpacityBoost ) , 0.0 , 1.0 );
			o.Alpha = clampResult50;
			float3 ase_worldPos = i.worldPos;
			float ifLocalVar176 = 0;
			if( distance( ase_worldPos , _Position ) >= _Distance )
				ifLocalVar176 = 0.0;
			else
				ifLocalVar176 = 1.0;
			clip( ( ( (_OpacityRemapMin + (tex2D( _OpacityCutOff, temp_output_159_0 ).r - 0.0) * (_OpacityRemapMax - _OpacityRemapMin) / (1.0 - 0.0)) - ( 1.0 - clampResult145 ) ) * ifLocalVar176 ) - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17000
247.2;1140.8;1524;796;404.626;-1240.262;1;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1589.823,-238.8197;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;138;-1430.44,627.7201;Float;False;Property;_UVStretchBoost;UVStretchBoost;0;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;136;-1432.137,385.2215;Float;True;Property;_Mask2;Mask2;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-1534.497,-336.7775;Float;False;Property;_Noise1ScrollSpeed;Noise1ScrollSpeed;4;0;Create;True;0;0;False;0;0.6;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;137;-1080.488,431.0319;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-1330.961,170.6883;Float;False;Property;_UVStretch;UVStretch;3;0;Create;True;0;0;False;0;0;2;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;133;-1267.5,-79.15564;Float;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;23;-1318.692,-330.5788;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-942.9637,-64.58484;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-1533.454,-488.663;Float;False;Property;_Noise2ScrollSpeed;Noise2ScrollSpeed;5;0;Create;True;0;0;False;0;0.7;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;130;-896.9637,-339.5848;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-730.9637,-56.58484;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;132;-481.9637,-293.5848;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;9;-1316.798,-491.1165;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;160;-808.5761,-153.0526;Float;False;Constant;_TilingY;TilingY;3;0;Create;True;0;0;False;0;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;158;-323.2501,-179.0801;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0.2,0.2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;163;-808.8942,-234.5795;Float;False;Property;_TilingX;TilingX;6;0;Create;True;0;0;False;0;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;129;-903.4704,-578.5871;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;164;-489.3387,-387.4147;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;131;-494.9637,-481.5848;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;26;-123.9458,385.7356;Float;True;Property;_Core;Core;8;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;25;-141.5316,123.9425;Float;True;Property;_Noise1;Noise1;11;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;34;260.9883,236.1062;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;-286.6827,-427.6952;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;144;63.61035,703.5341;Float;False;Property;_Progress;Progress;9;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;36;518.193,250.4129;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-136.5507,-120.7431;Float;True;Property;_Noise2;Noise2;13;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;145;368.2151,701.1095;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;140;257.0618,-103.2729;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;897.9958,130.2661;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;143;1174.32,129.0247;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;63.85364,1192.835;Float;False;Property;_OpacityRemapMin;OpacityRemapMin;14;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;49.85364,1294.835;Float;False;Property;_OpacityRemapMax;OpacityRemapMax;15;0;Create;True;0;0;False;0;1.35;1.35;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;40;1376.327,104.6358;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;149;-6.805298,989.1183;Float;True;Property;_OpacityCutOff;OpacityCutOff;16;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;142;928.5413,363.0305;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;171;127.7627,1516.159;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;175;125.7627,1706.159;Float;False;Property;_Position;Position;7;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;150;366.0024,1083.73;Float;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GradientNode;166;1641.151,-392.7626;Float;False;0;5;2;1,1,1,0.1579004;0.2783019,0.8500335,1,0.3362631;0.3253653,0.8598131,1,0.7719234;0.1745283,0.4274954,1,0.8362554;0.3460751,0.6315314,0.9528302,0.8713512;1,0;1,1;0;1;OBJECT;0
Node;AmplifyShaderEditor.DynamicAppendNode;125;1671.892,-43.24716;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;49;1878.738,594.0971;Float;False;Property;_OpacityBoost;OpacityBoost;19;0;Create;True;0;0;False;0;2;0.93;0;4;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;146;1088.06,358.0435;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;459.7627,1918.159;Float;False;Constant;_Float8;Float 8;9;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;47;1344.383,468.6339;Float;True;Property;_Mask;Mask;17;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;172;435.7627,1579.159;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;173;421.7627,1721.159;Float;False;Property;_Distance;Distance;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;153;510.561,963.6433;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;177;455.7627,1826.159;Float;False;Constant;_Float7;Float 7;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;176;798.7627,1668.159;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;154;819.561,975.6433;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;2316.382,443.2302;Float;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;148;1886.983,98.56443;Float;False;Property;_Color;Color;12;0;Create;True;0;0;False;0;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GradientSampleNode;165;1923.151,-387.7626;Float;True;2;0;OBJECT;0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;127;1942.874,269.8154;Float;False;Property;_ColorStrength;ColorStrength;18;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;50;2503.191,379.6143;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;169;1704.365,707.5496;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;179;1142.709,1200.855;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;2392.611,-15.2318;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;170;2002.365,785.5496;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3092.081,137.4358;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;WaterGunLine;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;False;TransparentCutout;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;137;0;136;1
WireConnection;137;1;138;0
WireConnection;133;0;7;2
WireConnection;23;0;22;0
WireConnection;134;0;133;0
WireConnection;134;1;139;0
WireConnection;134;2;137;0
WireConnection;130;0;7;2
WireConnection;130;1;23;0
WireConnection;135;0;7;1
WireConnection;135;1;134;0
WireConnection;132;0;135;0
WireConnection;132;1;130;0
WireConnection;9;0;4;0
WireConnection;158;0;132;0
WireConnection;129;0;7;2
WireConnection;129;1;9;0
WireConnection;164;0;163;0
WireConnection;164;1;160;0
WireConnection;131;0;135;0
WireConnection;131;1;129;0
WireConnection;25;1;158;0
WireConnection;34;0;25;1
WireConnection;34;1;26;1
WireConnection;159;0;131;0
WireConnection;159;1;164;0
WireConnection;36;0;34;0
WireConnection;8;1;159;0
WireConnection;145;0;144;0
WireConnection;140;0;8;1
WireConnection;141;0;36;0
WireConnection;141;1;145;0
WireConnection;143;0;141;0
WireConnection;143;1;140;0
WireConnection;40;0;143;0
WireConnection;149;1;159;0
WireConnection;142;0;140;0
WireConnection;142;1;36;0
WireConnection;150;0;149;1
WireConnection;150;3;151;0
WireConnection;150;4;152;0
WireConnection;125;0;40;0
WireConnection;146;0;142;0
WireConnection;172;0;171;0
WireConnection;172;1;175;0
WireConnection;153;0;145;0
WireConnection;176;0;172;0
WireConnection;176;1;173;0
WireConnection;176;2;177;0
WireConnection;176;3;177;0
WireConnection;176;4;178;0
WireConnection;154;0;150;0
WireConnection;154;1;153;0
WireConnection;48;0;146;0
WireConnection;48;1;47;1
WireConnection;48;2;49;0
WireConnection;165;0;166;0
WireConnection;165;1;125;0
WireConnection;50;0;48;0
WireConnection;169;0;47;1
WireConnection;179;0;154;0
WireConnection;179;1;176;0
WireConnection;44;0;165;0
WireConnection;44;1;148;0
WireConnection;44;2;127;0
WireConnection;170;0;169;0
WireConnection;0;2;44;0
WireConnection;0;9;50;0
WireConnection;0;10;179;0
ASEEND*/
//CHKSM=9F5992CCEE2B6C8F82CAB4EC4D34F64FD83D0CB0