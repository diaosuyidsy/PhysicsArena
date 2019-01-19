Shader "Hidden/DCEffect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_dcLength("Dark Cornered Length", Float) = 0
		_CenterPoint("Center", Vector) = (0,0,0,0)
	}
		SubShader
		{
			// No culling or depth
			//Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		sampler2D _MainTex;
		float _dcLength;
		float4 _CenterPoint;

		fixed4 frag(v2f i) : SV_Target
		{
			float4 c = tex2D(_MainTex, i.uv);
			// Calculate distance between i and centerpoint
			float curDistance = distance(_CenterPoint.xyz, i.uv);
			// Calculate the changefactor in according to the distance
			//float changeFactor = saturate(curDistance - _dcLength);
			float changeFactor = sign(curDistance - _dcLength);
			//float lum = c.r*.3 + c.g*.59 + c.b*.11;
			float3 b = float3(0, 0, 0);

			float4 result = c;
			//result.rgb = lerp(c.rgb, b, changeFactor);
			result.rgb = b * changeFactor + (1 - changeFactor) * c.rgb;
			return result;
	}
	ENDCG
}
		}
}
