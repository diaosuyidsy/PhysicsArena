// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Crystal Glass/Blur" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float4 uv01 : TEXCOORD1;
				float4 uv23 : TEXCOORD2;
				float4 uv45 : TEXCOORD3;
			};
			float4 _Offsets;
			sampler2D _MainTex;
			v2f vert (appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy;
				o.uv01 = v.texcoord.xyxy + _Offsets.xyxy * float4(1,1, -1,-1);
				o.uv23 = v.texcoord.xyxy + _Offsets.xyxy * float4(1,1, -1,-1) * 2.0;
				o.uv45 = v.texcoord.xyxy + _Offsets.xyxy * float4(1,1, -1,-1) * 3.0;
				return o;
			}
			half4 frag (v2f i) : COLOR
			{
				half4 c = 0.0;
				c += 0.40 * tex2D(_MainTex, i.uv);
				c += 0.15 * tex2D(_MainTex, i.uv01.xy);
				c += 0.15 * tex2D(_MainTex, i.uv01.zw);
				c += 0.10 * tex2D(_MainTex, i.uv23.xy);
				c += 0.10 * tex2D(_MainTex, i.uv23.zw);
				c += 0.05 * tex2D(_MainTex, i.uv45.xy);
				c += 0.05 * tex2D(_MainTex, i.uv45.zw);
				return c;
			}
			ENDCG
		}
	}
	Fallback Off
}
