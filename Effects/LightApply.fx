float2 offset;
float2 screenSize;
float2 texSize;

texture sampleTexture;
sampler2D samplerTex = sampler_state { texture = <sampleTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };
texture targetTexture;
sampler2D targetTex = sampler_state { texture = <targetTexture>; magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR; AddressU = mirror; AddressV = mirror; };

struct VertexShaderInput
{
	float2 coord : TEXCOORD0;
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float2 coord : TEXCOORD0;
	float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	output.coord = input.coord;
	output.Position = input.Position;
	return output;
}

float4 Fragment(VertexShaderOutput input) : COLOR
{
	float2 st = input.coord * texSize / screenSize + offset;

	float3 color = tex2D(samplerTex, st).xyz * tex2D(targetTex, input.coord).xyz;

	return float4(color, tex2D(targetTex, input.coord).w);
}

technique Technique1
{
	pass Shade
	{
		VertexShader = compile vs_2_0 MainVS();
		PixelShader = compile ps_2_0 Fragment();
	}
}