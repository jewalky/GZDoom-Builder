// 3D world rendering shader
// Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com

// Vertex input data
struct VertexData
{
	float3 pos		  : POSITION;
	float4 color	  : COLOR0;
	float2 uv		    : TEXCOORD0;
	float3 normal   : NORMAL; //mxd
};

// Pixel input data
struct PixelData
{
	float4 pos		: POSITION;
	float4 color	: COLOR0;
	float2 uv		  : TEXCOORD0;
};

//mxd. Vertex input data for sky rendering
struct SkyVertexData
{
	float3 pos		: POSITION;
	float2 uv			: TEXCOORD0;
};

//mxd. Pixel input data for sky rendering
struct SkyPixelData
{
	float4 pos		: POSITION;
	float3 tex		: TEXCOORD0;
};

//mxd. Pixel input data for light pass
struct LitPixelData
{
	float4 pos		  : POSITION;
	float4 color	  : COLOR0;
	float2 uv		    : TEXCOORD0;
	float3 pos_w    : TEXCOORD1; //mxd. pixel position in world space
	float3 normal   : TEXCOORD2; //mxd. normal
};

// Highlight color
float4 highlightcolor;

// Matrix for final transformation
const float4x4 worldviewproj;

//mxd
float4x4 world;
float4 vertexColor;
// [ZZ]
float4 stencilColor;

//light
float4 lightPosAndRadius;
float4 lightColor; //also used as fog color

//fog
const float4 campos;  //w is set to fade factor (distance, at wich fog color completely overrides pixel color)

//sky
static const float4 skynormal = float4(0.0f, 1.0f, 0.0f, 0.0f);

// Texture input
const texture texture1;

// Filter settings
const dword minfiltersettings;
const dword magfiltersettings;
const dword mipfiltersettings;
const float maxanisotropysetting;

// Texture sampler settings
sampler2D texturesamp = sampler_state
{
	Texture = <texture1>;
	MagFilter = magfiltersettings;
	MinFilter = minfiltersettings;
	MipFilter = mipfiltersettings;
	MipMapLodBias = 0.0f;
	MaxAnisotropy = maxanisotropysetting;
};

//mxd. Skybox texture sampler settings
samplerCUBE skysamp = sampler_state
{
	Texture = <texture1>;
	MagFilter = magfiltersettings;
	MinFilter = minfiltersettings;
	MipFilter = mipfiltersettings;
	MipMapLodBias = 0.0f;
	MaxAnisotropy = maxanisotropysetting;
};

// Vertex shader
PixelData vs_main(VertexData vd) 
{
	PixelData pd;
	
	// Fill pixel data input
	pd.pos = mul(float4(vd.pos, 1.0f), worldviewproj);
	pd.color = vd.color;
	pd.uv = vd.uv;
	
	// Return result
	return pd;
}

//mxd. same as vs_main, but uses vertexColor var instead of actual vertex color. used in models rendering
PixelData vs_customvertexcolor(VertexData vd) 
{
	PixelData pd;
	
	// Fill pixel data input
	pd.pos = mul(float4(vd.pos, 1.0f), worldviewproj);
	pd.color = vertexColor;
	pd.uv = vd.uv;
	
	// Return result
	return pd;
}

LitPixelData vs_customvertexcolor_fog(VertexData vd) 
{
	LitPixelData pd;
	
	// Fill pixel data input
	pd.pos = mul(float4(vd.pos, 1.0f), worldviewproj);
	pd.pos_w = mul(float4(vd.pos, 1.0f), world).xyz;
	pd.color = vertexColor;
	pd.uv = vd.uv;
	pd.normal = vd.normal;
	
	// Return result
	return pd;
}

//mxd. light pass vertex shader
LitPixelData vs_lightpass(VertexData vd) 
{
	LitPixelData pd;
	pd.pos = mul(float4(vd.pos, 1.0f), worldviewproj);
	pd.pos_w = mul(float4(vd.pos, 1.0f), world).xyz;
	pd.color = vd.color;
	pd.uv = vd.uv;
	pd.normal = vd.normal;

	// Return result
	return pd;
}

// Normal pixel shader
float4 ps_main(PixelData pd) : COLOR
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	return tcolor * pd.color;
}

// Full-bright pixel shader
float4 ps_fullbright(PixelData pd) : COLOR
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	tcolor.a *= pd.color.a;
	return tcolor;
}

// Normal pixel shader with highlight
float4 ps_main_highlight(PixelData pd) : COLOR
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	if(tcolor.a == 0) return tcolor;
	
	// Blend texture color and vertex color
	float4 ncolor = tcolor * pd.color;
	
	return float4(highlightcolor.rgb * highlightcolor.a + (ncolor.rgb - 0.4f * highlightcolor.a), max(pd.color.a + 0.25f, 0.5f));
}

// Full-bright pixel shader with highlight
float4 ps_fullbright_highlight(PixelData pd) : COLOR
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	if(tcolor.a == 0) return tcolor;
	
	// Blend texture color and vertex color
	float4 ncolor = tcolor * pd.color;
	
	return float4(highlightcolor.rgb * highlightcolor.a + (tcolor.rgb - 0.4f * highlightcolor.a), max(pd.color.a + 0.25f, 0.5f));
}

//mxd. This adds fog color to current pixel color
float4 getFogColor(LitPixelData pd, float4 color)
{
	float fogdist = max(16.0f, distance(pd.pos_w, campos.xyz));
	float fogfactor = exp2(campos.w * fogdist);

	color.rgb = lerp(lightColor.rgb, color.rgb, fogfactor);
	return color;
}

//mxd. Shaders with fog calculation
// Normal pixel shader
float4 ps_main_fog(LitPixelData pd) : COLOR 
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	if(tcolor.a == 0) return tcolor;
	
	return getFogColor(pd, tcolor * pd.color);
}

// Normal pixel shader with highlight
float4 ps_main_highlight_fog(LitPixelData pd) : COLOR 
{
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	if(tcolor.a == 0) return tcolor;
	
	// Blend texture color and vertex color
	float4 ncolor = getFogColor(pd, tcolor * pd.color);
	
	return float4(highlightcolor.rgb * highlightcolor.a + (ncolor.rgb - 0.4f * highlightcolor.a), max(ncolor.a + 0.25f, 0.5f));
}

//mxd: used to draw bounding boxes
float4 ps_constant_color(PixelData pd) : COLOR 
{
	return vertexColor;
}

//mxd: used to draw event lines
float4 ps_vertex_color(PixelData pd) : COLOR 
{
	return pd.color;
}

//mxd. dynamic light pixel shader pass, dood!
float4 ps_lightpass(LitPixelData pd) : COLOR
{
	//is face facing away from light source?
	// [ZZ] oddly enough pd.normal is not a proper normal, so using dot on it returns rather unexpected results. wrapped in normalize().
	//      update 01.02.2017: offset the equation by 3px back to try to emulate GZDoom's broken visibility check.
	float diffuseContribution = dot(normalize(pd.normal), normalize(lightPosAndRadius.xyz - pd.pos_w + normalize(pd.normal)*3));
	if (diffuseContribution < 0)
		clip(-1);
	diffuseContribution = max(diffuseContribution, 0); // to make sure

	//is pixel in light range?
	float dist = distance(pd.pos_w, lightPosAndRadius.xyz);
	if(dist > lightPosAndRadius.w)
		clip(-1);
	
	//is pixel tranparent?
	float4 tcolor = tex2D(texturesamp, pd.uv);
	tcolor = lerp(tcolor, float4(stencilColor.rgb, tcolor.a), stencilColor.a);
	if(tcolor.a == 0.0f)
		clip(-1);

	//if it is - calculate color at current pixel
	float4 lightColorMod = float4(0.0f, 0.0f, 0.0f, 0.0f);

	lightColorMod.rgb = lightColor.rgb * tcolor.a * max(lightPosAndRadius.w - dist, 0.0f) / lightPosAndRadius.w;
	if (lightColor.a > 0.979f && lightColor.a < 0.981f) // attenuated light 98%
		lightColorMod.rgb *= diffuseContribution;
	if(lightColorMod.r > 0.0f || lightColorMod.g > 0.0f || lightColorMod.b > 0.0f)
	{
		lightColorMod.rgb *= lightColor.a;
		if (lightColor.a > 0.4f) //Normal, vavoom or negative light (or attenuated)
			return tcolor * lightColorMod;
		return lightColorMod; //Additive light
	}
	clip(-1);
	return lightColorMod; //should never get here
}

//mxd. Vertex skybox shader
SkyPixelData vs_skybox(SkyVertexData vd)
{
	SkyPixelData pd;
	pd.pos = mul(float4(vd.pos, 1.0f), worldviewproj);
	float3 worldpos = mul(float4(vd.pos, 1.0f), world).xyz;
	pd.tex = reflect(worldpos - campos.xyz, normalize(mul(skynormal, world).xyz));
	return pd;
}

//mxd. Pixel skybox shader
float4 ps_skybox(SkyPixelData pd) : COLOR
{
	float4 ncolor = texCUBE(skysamp, pd.tex);
	return float4(highlightcolor.rgb * highlightcolor.a + (ncolor.rgb - 0.4f * highlightcolor.a), 1.0f);
}

// Technique for shader model 2.0
technique SM20 
{
	// Normal
	pass p0 
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 ps_main();
	}
	
	// Full brightness mode
	pass p1 
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 ps_fullbright();
	}

	// Normal with highlight
	pass p2 
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 ps_main_highlight();
	}
	
	// Full brightness mode with highlight
	pass p3 
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader = compile ps_2_0 ps_fullbright_highlight();
	}
	
	//mxd. same as p0-p3, but using vertexColor variable
	// Normal
	pass p4 
	{
		VertexShader = compile vs_2_0 vs_customvertexcolor();
		PixelShader = compile ps_2_0 ps_main();
	}
	
	//mxd. Skybox shader
	pass p5 
	{
		VertexShader = compile vs_2_0 vs_skybox();
		PixelShader  = compile ps_2_0 ps_skybox();
	}
	
	// Normal with highlight
	pass p6 
	{
		VertexShader = compile vs_2_0 vs_customvertexcolor();
		PixelShader = compile ps_2_0 ps_main_highlight();
	}

	pass p7 {} //mxd. need this only to maintain offset
	
	//mxd. same as p0-p3, but with fog calculation
	// Normal
	pass p8 
	{
		VertexShader = compile vs_2_0 vs_lightpass();
		PixelShader = compile ps_2_0 ps_main_fog();
	}
	
	pass p9 {} //mxd. need this only to maintain offset

	// Normal with highlight
	pass p10 
	{
		VertexShader = compile vs_2_0 vs_lightpass();
		PixelShader = compile ps_2_0 ps_main_highlight_fog();
	}

	pass p11 {} //mxd. need this only to maintain offset
	
	//mxd. same as p4-p7, but with fog calculation
	// Normal
	pass p12 
	{
		VertexShader = compile vs_2_0 vs_customvertexcolor_fog();
		PixelShader = compile ps_2_0 ps_main_fog();
	}

	pass p13 {} //mxd. need this only to maintain offset
	
	// Normal with highlight
	pass p14 
	{
		VertexShader = compile vs_2_0 vs_customvertexcolor_fog();
		PixelShader = compile ps_2_0 ps_main_highlight_fog();
	}

	//mxd. Used to render event lines
	pass p15
	{
		VertexShader = compile vs_2_0 vs_main();
		PixelShader  = compile ps_2_0 ps_vertex_color();
	}
	
	//mxd. Just fills everything with vertexColor. Used in ThingCage rendering.
	pass p16 
	{
		VertexShader = compile vs_2_0 vs_customvertexcolor();
		PixelShader  = compile ps_2_0 ps_constant_color();
	}
	
	//mxd. Light pass
	pass p17 
	{
		VertexShader = compile vs_2_0 vs_lightpass();
		PixelShader  = compile ps_2_0 ps_lightpass();
		AlphaBlendEnable = true;
	}
}
