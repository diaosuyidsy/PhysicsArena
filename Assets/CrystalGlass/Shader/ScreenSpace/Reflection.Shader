// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Crystal Glass/Screen Space/Reflection" {
	SubShader {
		Pass {
			Name "SSR"
			Zwrite Off
			Ztest Lequal
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			Colormask RGBA

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ CRYSTAL_GLASS_BUMP
			#include "UnityCG.cginc"

			sampler2D _BumpMap, _Global_ScreenTex;
			float4 _BumpMap_ST;
			float  _FrezFalloff, _FrezPow, _Reflection;
			
			struct v2f
			{
				float4 pos    : SV_POSITION;
				float3 view   : TEXCOORD0;
				float2 uv     : TEXCOORD1;
				float3 t2v0   : TEXCOORD2;
				float3 t2v1   : TEXCOORD3;
				float4 scrpos : TEXCOORD4;
				float3 wldpos : TEXCOORD5;
			};
			v2f vert (appdata_tan v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.wldpos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.scrpos.xy = (float2(o.pos.x, o.pos.y) + o.pos.w) * 0.5;
				o.scrpos.zw = o.pos.zw;
								 
				TANGENT_SPACE_ROTATION; 
				o.view.xyz = mul(rotation, ObjSpaceViewDir(v.vertex));
				o.t2v0 = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
				o.t2v1 = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);
				return o;
			}
			float4 frag (v2f i) : SV_TARGET
			{
#if CRYSTAL_GLASS_BUMP
				float3 N = UnpackNormal(tex2D(_BumpMap, i.uv));
#else
				float3 N = float3(0, 0, 1);
#endif
				float2 vn;
				vn.x = dot(i.t2v0, N);
				vn.y = dot(i.t2v1, N);

				float2 absvn = abs(vn);
				float maxvn = max(absvn.x, absvn.y);
				maxvn = 1.0 / maxvn;
				vn = vn * maxvn;

				float dist = length(_WorldSpaceCameraPos - i.wldpos);
				float refscale = min(1.0 / dist, dist / 1.0);

				float angle = dot(normalize(i.view), N);
				angle = pow(angle, 2.0 * (1.0 - refscale));

				float2 uv = i.scrpos.xy / i.scrpos.w;
				uv += (vn * angle * (1.0 - angle * refscale));

				float3 c = tex2D(_Global_ScreenTex, uv).rgb;
				float frez = pow(1.0 - angle, _FrezFalloff) * _FrezPow;
				float blend = 1.0 - saturate(angle * 1.1);
				float atten = min((_Reflection + frez), blend);

				c.rgb *= atten;

				float2 edge = abs(uv * 2.0 - 1.0);
				edge = 1.0 - (edge * edge);
				float opacity = atten * min(edge.x, edge.y);
				return float4(c, opacity);
			}
			ENDCG
		}
	}
	Fallback Off
}
