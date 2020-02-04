
// functions begin
#define SUM4 4
#define SUM6 6
#define MULTIPLY_COLOR float4(1,1,1,0.5)
#define SCENE_COLOR float4(0,0,0,0)

struct _3x3UVDatas
{
	float2 LT;
	float2 T;
	float2 RT;
	float2 L;
	float2 C;
	float2 R;
	float2 LB;
	float2 B;
	float2 RB;
};

_3x3UVDatas PP_3x3UVs(float2 uv, float2 dist)
{
	_3x3UVDatas result;
	result.LT = (float2(-1, 1) * dist) + uv;
	result.T = (float2(0, 1) * dist) + uv;
	result.RT = (float2(1, 1) * dist) + uv;

	result.L = (float2(-1, 0) * dist) + uv;
	result.C = uv;
	result.R = (float2(1, 0) * dist) + uv;

	result.LB = (float2(-1, -1) * dist) + uv;
	result.B = (float2(0, -1) * dist) + uv;
	result.RB = (float2(1, -1) * dist) + uv;
	return result;
}

float WeightedSum6(float _w[SUM6], float _a[SUM6])
{
	float result = 0;
	for(int i = 0; i < SUM6; ++i)
		result += _w[i] * _a[i];

	return result;
}

float WeightedSum4(float _w[SUM4], float _a[SUM4])
{
	float result = 0;
	for(int i = 0; i < SUM4; ++i)
		result += _w[i] * _a[i];

	return result;
}

float GetDepth(sampler2D depthNormalTexture, float2 uv)
{
	return tex2D(depthNormalTexture, uv).w;
}

float3 GetNormal(sampler2D depthNormalTexture, float2 uv)
{
	float3 pn = normalize(tex2D(depthNormalTexture, uv).xyz);
	return pn;
}

float4 GetNormalAndDepth(sampler2D depthNormalTexture, float2 uv)
{
	float3 normalValues;
	float depthValue;
	DecodeDepthNormal(tex2D(depthNormalTexture, uv), depthValue, normalValues);
	return float4(normalValues, depthValue);
}

float SilhouetteSobel(sampler2D depthTexture, float lineWidth, float2 uv, float2 invSize)
{
	float2 dist = lineWidth * invSize;

	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float LT = GetDepth(depthTexture, uvDatas.LT);
	float T = GetDepth(depthTexture, uvDatas.T);
	float RT = GetDepth(depthTexture, uvDatas.RT);

	float L = GetDepth(depthTexture, uvDatas.L);
	float C = GetDepth(depthTexture, uvDatas.C);
	float R = GetDepth(depthTexture, uvDatas.R);

	float LB = GetDepth(depthTexture, uvDatas.LB);
	float B = GetDepth(depthTexture, uvDatas.B);
	float RB = GetDepth(depthTexture, uvDatas.RB);

	float w[SUM6] = { 1, 2, 1, -1, -2, -1 };
	float h[SUM6] = { LT, T, RT, LB, B, RB };
	float v[SUM6] = { RT, R, RB, LT, L, LB };

	float2 r;
	r.x = WeightedSum6(w, h);
	r.y = WeightedSum6(w, v);
	return length(r);
}

float SilhouetteSobelDepthAdaptive(sampler2D depthTexture, float lineWidth, float baseDepth, float2 uv, float2 invSize)
{
	float2 dist = lineWidth * invSize / GetDepth(depthTexture, uv) * baseDepth;
	dist = clamp(dist, invSize * 0.5, lineWidth * invSize);

	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float LT = GetDepth(depthTexture, uvDatas.LT);
	float T = GetDepth(depthTexture, uvDatas.T);
	float RT = GetDepth(depthTexture, uvDatas.RT);

	float L = GetDepth(depthTexture, uvDatas.L);
	float C = GetDepth(depthTexture, uvDatas.C);
	float R = GetDepth(depthTexture, uvDatas.R);

	float LB = GetDepth(depthTexture, uvDatas.LB);
	float B = GetDepth(depthTexture, uvDatas.B);
	float RB = GetDepth(depthTexture, uvDatas.RB);

	float w[SUM6] = { 1, 2, 1, -1, -2, -1 };
	float h[SUM6] = { LT, T, RT, LB, B, RB };
	float v[SUM6] = { RT, R, RB, LT, L, LB };

	float2 r;
	r.x = WeightedSum6(w, h);
	r.y = WeightedSum6(w, v);
	return length(r) / C;
}

float CreaseSobel(sampler2D depthNormalTexture, float lineWidth, float2 uv, float2 invSize)
{
	float dist = lineWidth * invSize;

	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float3 LT = GetNormal(depthNormalTexture, uvDatas.LT);
	float3 T = GetNormal(depthNormalTexture, uvDatas.T);
	float3 RT = GetNormal(depthNormalTexture, uvDatas.RT);

	float3 L = GetNormal(depthNormalTexture, uvDatas.L);
	float3 C = GetNormal(depthNormalTexture, uvDatas.C);
	float3 R = GetNormal(depthNormalTexture, uvDatas.R);

	float3 LB = GetNormal(depthNormalTexture, uvDatas.LB);
	float3 B = GetNormal(depthNormalTexture, uvDatas.B);
	float3 RB = GetNormal(depthNormalTexture, uvDatas.RB);

	float w[SUM4] = {2, 2, 1, 1};
	float a[SUM4];
	a[0] = dot(R, L);
	a[1] = dot(T, B);
	a[2] = dot(LT, RB);
	a[3] = dot(RT, LB);

	return (1 - (WeightedSum4(w, a) / 6.0)) * 2;
}

float CreaseSobelDepthAdaptive(sampler2D depthNormalTexture, float lineWidth, float baseDepth, float2 uv, float2 invSize)
{
	float depth = GetNormalAndDepth(depthNormalTexture, uv).a;
	float2 dist = lineWidth * invSize / depth * baseDepth;
	dist = clamp(dist, invSize * 0.5, lineWidth * invSize);

	_3x3UVDatas uvDatas = PP_3x3UVs(uv, dist);
	float3 LT = GetNormal(depthNormalTexture, uvDatas.LT);
	float3 T = GetNormal(depthNormalTexture, uvDatas.T);
	float3 RT = GetNormal(depthNormalTexture, uvDatas.RT);

	float3 L = GetNormal(depthNormalTexture, uvDatas.L);
	float3 C = GetNormal(depthNormalTexture, uvDatas.C);
	float3 R = GetNormal(depthNormalTexture, uvDatas.R);

	float3 LB = GetNormal(depthNormalTexture, uvDatas.LB);
	float3 B = GetNormal(depthNormalTexture, uvDatas.B);
	float3 RB = GetNormal(depthNormalTexture, uvDatas.RB);

	float w[SUM4] = {2, 2, 1, 1};
	float a[SUM4];
	a[0] = dot(R, L);
	a[1] = dot(T, B);
	a[2] = dot(LT, RB);
	a[3] = dot(RT, LB);

	return (1 - (WeightedSum4(w, a) / 6.0)) / (depth / baseDepth);
}


// functions end