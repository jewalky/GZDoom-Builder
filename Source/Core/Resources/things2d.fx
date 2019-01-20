// Things 2D rendering shader
// Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com

// Vertex input data
struct VertexData
{
	float3 pos		: POSITION;
	float4 color	: COLOR0;
	float2 uv		: TEXCOORD0;
};

// Pixel input data
struct PixelData
{
	float4 pos		: POSITION;
	float4 color	: COLOR0;
	float2 uv		: TEXCOORD0;
};

// Render settings
// w = transparency
float4 rendersettings;

//mxd. solid fill color. used in model wireframe rendering
float4 fillColor;

//[ZZ]
float desaturation;

// Transform settings
float4x4 transformsettings;

// Texture1 input
texture texture1
<
	string UIName = "Texture1";
	string ResourceType = "2D";
>;

// Texture sampler settings
sampler2D texture1samp = sampler_state
{
	Texture = <texture1>;
	MagFilter = Linear;
	MinFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
	MipMapLodBias = -0.9f;
};

//mxd. Texture sampler settings for sprite rendering
sampler2D texture1sprite = sampler_state
{
	Texture = <texture1>;
	MagFilter = Point;
	MinFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
	MipMapLodBias = 0.0f;
};

// Transformation
PixelData vs_transform(VertexData vd)
{
	PixelData pd = (PixelData)0;
	pd.pos = mul(float4(vd.pos, 1.0f), transformsettings);
	pd.color = vd.color;
	pd.uv = vd.uv;
	return pd;
}

// [ZZ] desaturation routine. almost literal quote from GZDoom's GLSL
float3 desaturate(float3 texel)
{
	float gray = (texel.r * 0.3 + texel.g * 0.56 + texel.b * 0.14);	
	return lerp(texel, float3(gray,gray,gray), desaturation);
}

//mxd. Pixel shader for sprite drawing
float4 ps_sprite(PixelData pd) : COLOR
{
	// Take this pixel's color
	float4 c = tex2D(texture1sprite, pd.uv);
	
	// Modulate it by selection color
	if(pd.color.a > 0)
	{
		float3 cr = desaturate(c.rgb);
		return float4((cr.r + pd.color.r) / 2.0f, (cr.g + pd.color.g) / 2.0f, (cr.b + pd.color.b) / 2.0f, c.a * rendersettings.w * pd.color.a);
	}

	// Or leave it as it is
	return float4(desaturate(c.rgb), c.a * rendersettings.w);
}

//mxd. Pixel shader for thing box and arrow drawing
float4 ps_thing(PixelData pd) : COLOR
{
	// Take this pixel's color
	float4 c = tex2D(texture1samp, pd.uv);
	return float4(desaturate(c.rgb), c.a * rendersettings.w) * pd.color;
}

//mxd. Pretty darn simple pixel shader for wireframe rendering :)
float4 ps_fill(PixelData pd) : COLOR 
{
	return fillColor;
}

// Technique for shader model 2.0
technique SM20
{
	pass p0 //mxd
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_thing();
	}

	pass p1 //mxd
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_sprite();
	}


	pass p2 //mxd
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_fill();
	}
}
