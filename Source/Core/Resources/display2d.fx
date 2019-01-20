// 2D display rendering shader
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
// x = texel width
// y = texel height
// z = FSAA blend factor
// w = transparency
float4 rendersettings;

// Transform settings
float4x4 transformsettings;

// Filter settings
dword filtersettings;

//
float desaturation;

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
	MagFilter = filtersettings;
	MinFilter = filtersettings;
	MipFilter = filtersettings;
	AddressU = Wrap;
	AddressV = Wrap;
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

// This blends the max of 2 pixels
float4 addcolor(float4 c1, float4 c2)
{
	return float4(max(c1.r, c2.r),
				  max(c1.g, c2.g),
				  max(c1.b, c2.b),
				  saturate(c1.a + c2.a * 0.5f));
}

// [ZZ] desaturation routine. almost literal quote from GZDoom's GLSL
float3 desaturate(float3 texel)
{
	float gray = (texel.r * 0.3 + texel.g * 0.56 + texel.b * 0.14);	
	return lerp(texel, float3(gray,gray,gray), desaturation);
}

// Pixel shader for antialiased drawing
float4 ps_fsaa(PixelData pd) : COLOR
{
	// Take this pixel's color
	float4 c = tex2D(texture1samp, pd.uv);
	
	// If this pixel is not drawn on...
	if(c.a < 0.1f)
	{
		// Mix the colors of nearby pixels
		float4 n = (float4)0;
		n = addcolor(n, tex2D(texture1samp, float2(pd.uv.x + rendersettings.x, pd.uv.y)));
		n = addcolor(n, tex2D(texture1samp, float2(pd.uv.x - rendersettings.x, pd.uv.y)));
		n = addcolor(n, tex2D(texture1samp, float2(pd.uv.x, pd.uv.y + rendersettings.y)));
		n = addcolor(n, tex2D(texture1samp, float2(pd.uv.x, pd.uv.y - rendersettings.y)));
		
		// If any pixels nearby where found, return a blend, otherwise return nothing
		//if(n.a > 0.1f) return float4(desaturate(n.rgb), n.a * settings.z); else return (float4)0;
		return float4(desaturate(n.rgb), n.a * rendersettings.z * rendersettings.w);
	}
	else return float4(desaturate(c.rgb), c.a * rendersettings.w) * pd.color;
}

// Pixel shader for normal drawing
float4 ps_normal(PixelData pd) : COLOR
{
	// Take this pixel's color
	float4 c = tex2D(texture1samp, pd.uv);
	return float4(desaturate(c.rgb), c.a * rendersettings.w) * pd.color;
}

//mxd. Pixel shader for full bright drawing
float4 ps_fullbright(PixelData pd) : COLOR
{
	// Take this pixel's color
	float4 c = tex2D(texture1samp, pd.uv);
	return float4(c.rgb, c.a * rendersettings.w);
}

// Technique for shader model 2.0
technique SM20
{
	pass p0
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_fsaa();
	}
	
	pass p1
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_normal();
	}
	
	pass p2 //mxd
	{
		VertexShader = compile vs_2_0 vs_transform();
		PixelShader = compile ps_2_0 ps_fullbright();
	}
}
