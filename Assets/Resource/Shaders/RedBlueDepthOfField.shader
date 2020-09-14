// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/RedBlueDepthOfField"
{
    Properties
	{
		_FocusDistance("FocusDistance", Float) = 0
		_FocusRange("FocusRange", Float) = 0
		_MainTex("MainTex", 2D) = "white" {}
		_BokehRadius("BokehRadius", Float) = 0
		_CoCTex("_CoCTex", 2D) = "white" {}
		_DoFTex("DoFTex", 2D) = "white" {}
		_GreenChannelMove("GreenChannelMove", Float) = 0
		_RedChannelMove("RedChannelMove", Float) = 0.4
		_BlueChannelMove("BlueChannelMove", Float) = 0.4
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}
    SubShader
	{
		Tags { "RenderType"="Opaque" "RenderType"="Opaque" }
		
		Pass
		{
			
			Name "First"
			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			Cull Back
			ColorMask R
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _FocusDistance;
			uniform float _FocusRange;
			uniform float _BokehRadius;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(ase_clipPos);
				o.ase_texcoord = screenPos;
				
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float4 screenPos = i.ase_texcoord;
				float4 ase_screenPosNorm = screenPos / screenPos.w;
				ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
				float eyeDepth13 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
				float clampResult20 = clamp( ( ( eyeDepth13 + ( _FocusDistance * -1.0 ) ) / _FocusRange ) , -1.0 , 1.0 );
				float BokehRadius122 = _BokehRadius;
				float4 temp_cast_0 = (( clampResult20 * BokehRadius122 )).xxxx;
				
				
				finalColor = temp_cast_0;
				return finalColor;
			}
			ENDCG
		}

		
		Pass
		{
			Name "Second"
			
			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform sampler2D _CoCTex;
			uniform float4 _MainTex_TexelSize;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float2 uv_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode108 = tex2D( _MainTex, uv_MainTex );
				float2 uv0_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 MainTexUV55 = uv0_MainTex;
				float4 TexelSize82 = _MainTex_TexelSize;
				float4 break86 = TexelSize82;
				float4 appendResult89 = (float4(break86.x , break86.y , break86.x , break86.y));
				float4 temp_output_90_0 = ( appendResult89 * float4(-0.5,-0.5,0.5,0.5) );
				float4 tex2DNode102 = tex2D( _CoCTex, ( MainTexUV55 + (temp_output_90_0).xy ) );
				float4 tex2DNode103 = tex2D( _CoCTex, ( MainTexUV55 + (temp_output_90_0).yz ) );
				float4 tex2DNode100 = tex2D( _CoCTex, ( MainTexUV55 + (temp_output_90_0).xw ) );
				float4 tex2DNode101 = tex2D( _CoCTex, ( MainTexUV55 + (temp_output_90_0).zw ) );
				float temp_output_116_0 = max( max( max( tex2DNode102.r , tex2DNode103.r ) , tex2DNode100.r ) , tex2DNode101.r );
				float temp_output_114_0 = min( min( min( tex2DNode102.r , tex2DNode103.r ) , tex2DNode100.r ) , tex2DNode101.r );
				float ifLocalVar117 = 0;
				if( temp_output_116_0 >= ( temp_output_114_0 * -1.0 ) )
				ifLocalVar117 = temp_output_116_0;
				else
				ifLocalVar117 = temp_output_114_0;
				float4 appendResult109 = (float4(tex2DNode108.r , tex2DNode108.g , tex2DNode108.b , ifLocalVar117));
				
				
				finalColor = appendResult109;
				return finalColor;
			}
			ENDCG
		}
		
		
		Pass
		{
			Name "Third"
			
			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float _RedChannelMove;
			uniform float _GreenChannelMove;
			uniform float _BlueChannelMove;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float2 uv0_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 MainTexUV55 = uv0_MainTex;
				float2 uv_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 appendResult137 = (float4(tex2D( _MainTex, ( MainTexUV55 + ( _RedChannelMove * 0.01 ) ) ).r , tex2D( _MainTex, ( MainTexUV55 + ( _GreenChannelMove * 0.01 ) ) ).g , tex2D( _MainTex, ( MainTexUV55 + ( _BlueChannelMove * 0.01 ) ) ).b , tex2D( _MainTex, uv_MainTex ).b));
				
				
				finalColor = appendResult137;
				return finalColor;
			}
			ENDCG
		}
		
		Pass
		{
			Name "Fourth"
			
			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float2 uv0_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 MainTexUV55 = uv0_MainTex;
				float4 TexelSize82 = _MainTex_TexelSize;
				float4 break85 = TexelSize82;
				float4 appendResult47 = (float4(break85.x , break85.y , break85.x , break85.y));
				float4 temp_output_48_0 = ( appendResult47 * float4(-0.5,-0.5,0.5,0.5) );
				
				
				finalColor = ( ( tex2D( _MainTex, ( MainTexUV55 + (temp_output_48_0).xy ) ) + tex2D( _MainTex, ( MainTexUV55 + (temp_output_48_0).yz ) ) + tex2D( _MainTex, ( MainTexUV55 + (temp_output_48_0).xw ) ) + tex2D( _MainTex, ( MainTexUV55 + (temp_output_48_0).zw ) ) ) * 0.25 );
				return finalColor;
			}
			ENDCG
		}
		
		Pass
		{
			Name "Fifth"
			
			CGINCLUDE
			#pragma target 3.0
			ENDCG
			Blend Off
			Cull Back
			ColorMask RGBA
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
				float4 ase_texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform sampler2D _DoFTex;
			uniform float4 _DoFTex_ST;
			uniform sampler2D _CoCTex;
			uniform float4 _CoCTex_ST;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				fixed4 finalColor;
				float2 uv_MainTex = i.ase_texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float2 uv_DoFTex = i.ase_texcoord.xy * _DoFTex_ST.xy + _DoFTex_ST.zw;
				float2 uv_CoCTex = i.ase_texcoord.xy * _CoCTex_ST.xy + _CoCTex_ST.zw;
				float4 tex2DNode127 = tex2D( _CoCTex, uv_CoCTex );
				float smoothstepResult129 = smoothstep( 0.1 , 1.0 , tex2DNode127.r);
				float4 lerpResult131 = lerp( tex2D( _MainTex, uv_MainTex ) , tex2D( _DoFTex, uv_DoFTex ) , smoothstepResult129);
				
				
				finalColor = lerpResult131;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=17000
-549.6;1636;1524;751;3202.575;1368.886;2.17329;True;False
Node;AmplifyShaderEditor.SamplerNode;127;1929.544,763.4427;Float;True;Property;_TextureSample12;Texture Sample 12;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;107;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;125;1439.607,298.2513;Float;True;Property;_DoFTex;DoFTex;17;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;77;-867.3708,-2250.824;Float;False;3215.877;991.0487;;21;50;47;48;61;56;62;63;64;66;65;57;67;70;71;68;69;72;75;74;84;85;Gaussian blur;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;110;911.1682,-1088.571;Float;False;3359.216;1057.597;Comment;32;102;95;94;109;108;42;93;98;87;81;107;92;86;88;89;90;91;99;103;100;101;96;97;111;113;114;112;115;116;117;118;119;Downsampling CoC;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;126;1968.834,52.9308;Float;True;Property;_TextureSample11;Texture Sample 11;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;79;-1517.942,-6.574034;Float;False;1228.898;382.482;Comment;10;15;17;13;18;16;20;19;41;120;123;Coc;1,1,1,1;0;0
Node;AmplifyShaderEditor.SmoothstepOpNode;129;2419.67,767.4196;Float;False;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;128;1929.938,337.933;Float;True;Property;_TextureSample13;Texture Sample 13;16;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;78;-1499.416,-1218.187;Float;False;1558.231;752.2026;Comment;14;35;38;40;51;34;55;43;33;82;122;132;133;134;137;BlurSample;1,1,1,1;0;0
Node;AmplifyShaderEditor.CustomExpressionNode;33;-447.9286,-1171.314;Float;False;half3 bgcolor = 0@$float bgweight = 0@$$static const int kernelSampleCount = 16@$static const float2 kernel[kernelSampleCount] = {$float2(0, 0),$					float2(0.54545456, 0),$					float2(0.16855472, 0.5187581),$					float2(-0.44128203, 0.3206101),$					float2(-0.44128197, -0.3206102),$float2(0.1685548, -0.5187581),$float2(1, 0),$float2(0.809017, 0.58778524),$					float2(0.30901697, 0.95105654),$					float2(-0.30901703, 0.9510565),$					float2(-0.80901706, 0.5877852),$float2(-1, 0),$					float2(-0.80901694, -0.58778536),$					float2(-0.30901664, -0.9510566),$					float2(0.30901712, -0.9510565),$					float2(0.80901694, -0.5877853),$				}@$$$$for (int k = 0@ k < kernelSampleCount@ k++) {$float2 o = kernel[k] * bokehRadius@$half radius = length(o)@$o *= texelSize.xy@$half4 s = tex2D( mainTex, MainTexUV + o)@$half coc = tex2D(mainTex, MainTexUV).a@$half bgw = saturate((max(0, min(s.a,coc)) - radius + 2) / 2)@$bgcolor += s.rgb *bgw@$bgweight += bgw@$}					$bgcolor *= 1.0 / (bgweight + (bgweight == 0))@$return half4(bgcolor,1)@;4;False;4;True;texelSize;FLOAT4;0,0,0,0;In;;Float;False;True;mainTex;SAMPLER2D;_Sampler133;In;;Float;False;True;MainTexUV;FLOAT2;0,0;In;;Float;False;True;bokehRadius;FLOAT;0;In;;Float;False;ForLoopBokeh;True;False;0;4;0;FLOAT4;0,0,0,0;False;1;SAMPLER2D;_Sampler133;False;2;FLOAT2;0,0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;35;-1474.423,-1088.399;Float;True;Property;_MainTex;MainTex;2;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;115;3396.823,-741.7579;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;1747.532,-1784.506;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;111;3260.372,-477.302;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;82;-719.1627,-1193.038;Float;False;TexelSize;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;102;2753.062,-896.4872;Float;True;Property;_TextureSample8;Texture Sample 8;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;107;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;69;888.04,-1919.522;Float;True;Property;_TextureSample2;Texture Sample 2;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexelSizeNode;38;-1014.655,-1168.187;Float;False;-1;1;0;SAMPLER2D;;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;95;2128.152,-257.6205;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;2472.552,-348.3105;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;119;3531.114,-345.816;Float;False;Constant;_Float1;Float 1;13;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;63;180.4726,-1821.154;Float;False;True;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;91;2121.778,-506.1925;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;961.1682,-709.9435;Float;False;82;TexelSize;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMinOpNode;114;3551.572,-469.302;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;113;3394.771,-475.7016;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;92;1924.904,-674.6415;Float;False;55;MainTexUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-1205.468,179.9645;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;103;2758.517,-692.584;Float;True;Property;_TextureSample9;Texture Sample 9;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;107;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;68;882.5846,-2125.289;Float;True;Property;_TextureSample1;Texture Sample 1;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;81;1240.882,-928.7996;Float;True;Property;_CoCTex;_CoCTex;16;0;Create;True;0;0;False;0;None;None;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-1443.307,-255.4803;Float;False;Property;_GreenChannelMove;GreenChannelMove;18;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;93;2110.305,-425.8845;Float;False;False;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;1935.367,-524.6056;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;-954.5704,-316.4494;Float;False;2;2;0;FLOAT2;0.3,0;False;1;FLOAT;-0.008;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;136;-952.8792,-445.4944;Float;False;2;2;0;FLOAT2;0.3,0;False;1;FLOAT;-0.008;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;132;-1318.458,-579.1403;Float;False;55;MainTexUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;108;3328.298,-1038.571;Float;True;Property;_TextureSample10;Texture Sample 10;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;84;-790.7635,-2205.169;Float;False;82;TexelSize;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;51;-1260.257,-789.7938;Float;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;55;-706.421,-937.9;Float;False;MainTexUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;98;2474.127,-668.7526;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;122;-464.5034,-825.7324;Float;False;BokehRadius;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;101;2779.922,-260.9742;Float;True;Property;_TextureSample7;Texture Sample 7;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;107;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;117;3856.597,-603.774;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;86;1173.639,-542.0145;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;109;3810.69,-947.0763;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-1024.694,-941.4834;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;3690.114,-482.816;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-669.8023,-827.4539;Float;False;Property;_BokehRadius;BokehRadius;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;99;2471.277,-206.8156;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-1458.92,-447.3201;Float;False;Property;_RedChannelMove;RedChannelMove;19;0;Create;True;0;0;False;0;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;19;-861.0323,199.5863;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;88;1556.145,-446.6755;Float;False;Constant;_Vector1;Vector 1;4;0;Create;True;0;0;False;0;-0.5,-0.5,0.5,0.5;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMaxOpNode;112;3276.873,-740.4026;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;61;186.8462,-1989.418;Float;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;57;539.1951,-2151.978;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;62;175.3737,-1909.11;Float;False;False;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;-382.4976,-2200.824;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode;94;2115.404,-337.9286;Float;False;True;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;537.6204,-1831.536;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;65;546.8088,-1989.388;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;89;1552.434,-717.5985;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1042.698,261.308;Float;False;Property;_FocusRange;FocusRange;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;85;-761.2924,-2025.24;Float;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMaxOpNode;116;3532.025,-745.6579;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;71;906.8784,-1492.343;Float;True;Property;_TextureSample4;Texture Sample 4;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;50;-378.7869,-1929.901;Float;False;Constant;_Vector0;Vector 0;4;0;Create;True;0;0;False;0;-0.5,-0.5,0.5,0.5;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;0.4350321,-2007.831;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-724.051,284.3964;Float;False;122;BokehRadius;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;56;-10.02785,-2157.867;Float;False;55;MainTexUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;17;-1038.806,96.96448;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;1507.138,-1822.376;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;64;193.22,-1740.846;Float;False;False;False;True;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;-1209.557,-345.6753;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;-1167.699,-459.2893;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;137;-308.1492,-653.7448;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;143;-777.5173,-550.615;Float;True;Property;_TextureSample16;Texture Sample 16;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-1467.942,180.8952;Float;False;Property;_FocusDistance;FocusDistance;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;130;2227.096,759.384;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;1531.836,-1654.429;Float;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;70;892.9796,-1715.351;Float;True;Property;_TextureSample3;Texture Sample 3;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;67;536.3458,-1690.041;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;20;-715.723,118.3146;Float;False;3;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;97;2481.74,-506.1625;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;-514.2606,216.4151;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;134;-765.0761,-691.7364;Float;True;Property;_TextureSample14;Texture Sample 14;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;133;-959.5439,-572.78;Float;False;2;2;0;FLOAT2;0.3,0;False;1;FLOAT;0.008;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenDepthNode;13;-1466.852,71.73688;Float;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;141;-1469.979,-357.6296;Float;False;Property;_BlueChannelMove;BlueChannelMove;20;0;Create;True;0;0;False;0;0.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;135;-746.4094,-362.1188;Float;True;Property;_TextureSample15;Texture Sample 15;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;51;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;107;1629.906,-923.6043;Float;True;Property;_TextureSample5;Texture Sample 5;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;146;-1180.302,-235.5269;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0.01;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;131;2983.061,251.2886;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;100;2763.457,-486.5492;Float;True;Property;_TextureSample6;Texture Sample 6;12;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Instance;107;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;43;-167.5847,282.4259;Float;False;False;2;Float;ASEMaterialInspector;0;10;Hidden/Legacy/DoFMultiPass;ca5196d2b87315949a20b4e2139d10c4;True;Third;0;2;Third;2;False;False;False;False;False;False;False;False;False;True;2;RenderType=Opaque=RenderType;RenderType=Opaque=RenderType;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;True;2;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;124;3161.865,512.4259;Float;False;False;2;Float;ASEMaterialInspector;0;10;Hidden/Legacy/DoFMultiPass;ca5196d2b87315949a20b4e2139d10c4;True;Fifth;0;4;Fifth;2;False;False;False;False;False;False;False;False;False;True;1;RenderType=Opaque=RenderType;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;True;2;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;41;-515.444,43.42593;Float;False;True;2;Float;ASEMaterialInspector;0;10;Hidden/RedBlueDepthOfField;ca5196d2b87315949a20b4e2139d10c4;True;First;0;0;First;2;False;False;False;False;False;False;False;False;False;True;2;RenderType=Opaque=RenderType;RenderType=Opaque=RenderType;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;False;False;False;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;True;2;0;;0;0;Standard;0;0;5;True;True;True;True;True;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;42;4073.582,167.4259;Float;False;False;2;Float;ASEMaterialInspector;0;10;Hidden/Legacy/DoFMultiPass;ca5196d2b87315949a20b4e2139d10c4;True;Second;0;1;Second;2;False;False;False;False;False;False;False;False;False;True;2;RenderType=Opaque=RenderType;RenderType=Opaque=RenderType;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;True;2;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;44;2349.075,381.4672;Float;False;False;2;Float;ASEMaterialInspector;0;10;Hidden/Legacy/DoFMultiPass;ca5196d2b87315949a20b4e2139d10c4;True;Fourth;0;3;Fourth;2;False;False;False;False;False;False;False;False;False;True;2;RenderType=Opaque=RenderType;RenderType=Opaque=RenderType;False;0;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;False;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;True;2;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;129;0;127;1
WireConnection;128;0;125;0
WireConnection;33;0;38;0
WireConnection;33;1;35;0
WireConnection;33;2;34;0
WireConnection;33;3;40;0
WireConnection;115;0;112;0
WireConnection;115;1;100;1
WireConnection;74;0;72;0
WireConnection;74;1;75;0
WireConnection;111;0;102;1
WireConnection;111;1;103;1
WireConnection;82;0;38;0
WireConnection;102;1;98;0
WireConnection;69;1;65;0
WireConnection;38;0;35;0
WireConnection;95;0;90;0
WireConnection;96;0;92;0
WireConnection;96;1;94;0
WireConnection;63;0;48;0
WireConnection;91;0;90;0
WireConnection;114;0;113;0
WireConnection;114;1;101;1
WireConnection;113;0;111;0
WireConnection;113;1;100;1
WireConnection;18;0;15;0
WireConnection;103;1;97;0
WireConnection;68;1;57;0
WireConnection;93;0;90;0
WireConnection;90;0;89;0
WireConnection;90;1;88;0
WireConnection;145;0;132;0
WireConnection;145;1;146;0
WireConnection;136;0;132;0
WireConnection;136;1;142;0
WireConnection;51;0;35;0
WireConnection;55;0;34;0
WireConnection;98;0;92;0
WireConnection;98;1;91;0
WireConnection;122;0;40;0
WireConnection;101;1;99;0
WireConnection;117;0;116;0
WireConnection;117;1;118;0
WireConnection;117;2;116;0
WireConnection;117;3;116;0
WireConnection;117;4;114;0
WireConnection;86;0;87;0
WireConnection;109;0;108;1
WireConnection;109;1;108;2
WireConnection;109;2;108;3
WireConnection;109;3;117;0
WireConnection;34;2;35;0
WireConnection;118;0;114;0
WireConnection;118;1;119;0
WireConnection;99;0;92;0
WireConnection;99;1;95;0
WireConnection;19;0;17;0
WireConnection;19;1;16;0
WireConnection;112;0;102;1
WireConnection;112;1;103;1
WireConnection;61;0;48;0
WireConnection;57;0;56;0
WireConnection;57;1;61;0
WireConnection;62;0;48;0
WireConnection;47;0;85;0
WireConnection;47;1;85;1
WireConnection;47;2;85;0
WireConnection;47;3;85;1
WireConnection;94;0;90;0
WireConnection;66;0;56;0
WireConnection;66;1;63;0
WireConnection;65;0;56;0
WireConnection;65;1;62;0
WireConnection;89;0;86;0
WireConnection;89;1;86;1
WireConnection;89;2;86;0
WireConnection;89;3;86;1
WireConnection;85;0;84;0
WireConnection;116;0;115;0
WireConnection;116;1;101;1
WireConnection;71;1;67;0
WireConnection;48;0;47;0
WireConnection;48;1;50;0
WireConnection;17;0;13;0
WireConnection;17;1;18;0
WireConnection;72;0;68;0
WireConnection;72;1;69;0
WireConnection;72;2;70;0
WireConnection;72;3;71;0
WireConnection;64;0;48;0
WireConnection;142;0;141;0
WireConnection;138;0;139;0
WireConnection;137;0;134;1
WireConnection;137;1;143;2
WireConnection;137;2;135;3
WireConnection;137;3;51;3
WireConnection;143;1;145;0
WireConnection;130;0;127;1
WireConnection;70;1;66;0
WireConnection;67;0;56;0
WireConnection;67;1;64;0
WireConnection;20;0;19;0
WireConnection;97;0;92;0
WireConnection;97;1;93;0
WireConnection;120;0;20;0
WireConnection;120;1;123;0
WireConnection;134;1;133;0
WireConnection;133;0;132;0
WireConnection;133;1;138;0
WireConnection;135;1;136;0
WireConnection;107;0;81;0
WireConnection;146;0;144;0
WireConnection;131;0;126;0
WireConnection;131;1;128;0
WireConnection;131;2;129;0
WireConnection;100;1;96;0
WireConnection;43;0;137;0
WireConnection;124;0;131;0
WireConnection;41;0;120;0
WireConnection;42;0;109;0
WireConnection;44;0;74;0
ASEEND*/
//CHKSM=B45F151A6A0DB483763A218E72AE758334009DE6