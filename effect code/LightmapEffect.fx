//-----------------------------------------------------------------------------
// DualTextureEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#include "Macros.fxh"


DECLARE_TEXTURE(TexDiffuse, 0);
DECLARE_TEXTURE(TexLightmap, 1);
DECLARE_TEXTURE(TexDetail, 2);


BEGIN_CONSTANTS

    float4 DiffuseColor     _vs(c0) _cb(c0);
    float3 FogColor         _ps(c0) _cb(c1);
    float4 FogVector        _vs(c5) _cb(c2);

	float2 DetailScale;

	float  Gamma = 2.2f;
	float Brightness = 0.91f;
	float AlphaCutoff = 0.5f;

MATRIX_CONSTANTS

    float4x4 WorldViewProj  _vs(c1) _cb(c0);

END_CONSTANTS


#include "Structures.fxh"
#include "Common.fxh"

// Converts from linear RGB space to sRGB.
float3 LinearToSRGB(in float3 color)
{
    return pow(color, 1/Gamma);
}
// Converts from sRGB space to linear RGB.
float3 SRGBToLinear(in float3 color)
{
    return pow(color, Gamma);
}

float3 GammaFunc(in float3 color)
{
	return log(1 + color) * Gamma;
}

// Vertex shader: basic.
VSOutputTx2 VSDualTexture(VSInputTx2 vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: no fog.
VSOutputTx2NoFog VSDualTextureNoFog(VSInputTx2 vin)
{
    VSOutputTx2NoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;

    return vout;
}


// Vertex shader: vertex color.
VSOutputTx2 VSDualTextureVc(VSInputTx2Vc vin)
{
    VSOutputTx2 vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParams;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Vertex shader: vertex color, no fog.
VSOutputTx2NoFog VSDualTextureVcNoFog(VSInputTx2Vc vin)
{
    VSOutputTx2NoFog vout;
    
    CommonVSOutput cout = ComputeCommonVSOutput(vin.Position);
    SetCommonVSOutputParamsNoFog;
    
    vout.TexCoord = vin.TexCoord;
    vout.TexCoord2 = vin.TexCoord2;
    vout.Diffuse *= vin.Color;
    
    return vout;
}


// Pixel shader: basic.
float4 PSDualTexture(PSInputTx2 pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(TexDiffuse, pin.TexCoord);
	//if (color.a <= AlphaCutoff)
	//	discard;

    float4 light = SAMPLE_TEXTURE(TexLightmap, pin.TexCoord2);

    color.rgb = GammaFunc(color.rgb);
    color *= light * pin.Diffuse;
	color.a = 1;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog.
float4 PSDualTextureNoFog(PSInputTx2NoFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(TexDiffuse, pin.TexCoord);
	//if (color.a <= AlphaCutoff)
	//	discard;

    float4 light = SAMPLE_TEXTURE(TexLightmap, pin.TexCoord2);

	color.rgb = GammaFunc(color.rgb);
    color *= light * pin.Diffuse;
	color.a = 1;
    
    return color;
}



// Pixel shader: basic, with detail
float4 PSDualTextureDetail(PSInputTx2 pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(TexDiffuse, pin.TexCoord);
	//if (color.a <= AlphaCutoff)
	//	discard;

    float4 light = SAMPLE_TEXTURE(TexLightmap, pin.TexCoord2);
	float4 detail = SAMPLE_TEXTURE(TexDetail, pin.TexCoord * DetailScale);

    color.rgb = GammaFunc(color.rgb);
	color *= 2;
	color *= detail;
    color *= light * pin.Diffuse;
	color.a = 1;
    
    ApplyFog(color, pin.Specular.w);
    
    return color;
}


// Pixel shader: no fog, with detail
float4 PSDualTextureDetailNoFog(PSInputTx2NoFog pin) : SV_Target0
{
    float4 color = SAMPLE_TEXTURE(TexDiffuse, pin.TexCoord);
	//if (color.a <= AlphaCutoff)
	//	discard;

    float4 light = SAMPLE_TEXTURE(TexLightmap, pin.TexCoord2);
	float4 detail = SAMPLE_TEXTURE(TexDetail, pin.TexCoord * DetailScale);

	color.rgb = GammaFunc(color.rgb);
	color *= 2;
	color *= detail;
    color *= light * pin.Diffuse;
	color.a = 1;
    
    return color;
}




VertexShader VSArray[8] =
{
    compile vs_2_0 VSDualTexture(),
    compile vs_2_0 VSDualTextureNoFog(),
    compile vs_2_0 VSDualTextureVc(),
    compile vs_2_0 VSDualTextureVcNoFog(),
	compile vs_2_0 VSDualTexture(),
    compile vs_2_0 VSDualTextureNoFog(),
    compile vs_2_0 VSDualTextureVc(),
    compile vs_2_0 VSDualTextureVcNoFog(),
};


PixelShader PSArray[4] =
{
    compile ps_2_0 PSDualTexture(),
    compile ps_2_0 PSDualTextureNoFog(),
	compile ps_2_0 PSDualTextureDetail(),
    compile ps_2_0 PSDualTextureDetailNoFog(),
};


int PSIndices[8] =
{
    0,      // basic
    1,      // no fog
    0,      // vertex color
    1,      // vertex color, no fog
	2,      // basic, detail
    3,      // no fog, detail
    2,      // vertex color, detail
    3,      // vertex color, no fog, detail
};


int ShaderIndex = 0;


Technique DualTextureEffect
{
    Pass
    {
        VertexShader = (VSArray[ShaderIndex]);
        PixelShader  = (PSArray[PSIndices[ShaderIndex]]);
    }
}