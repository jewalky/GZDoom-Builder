`#name Randomize Texture Offsets`;

`#scriptconfiguration

upper_x
{
	description = "Upper X Offset";
	default = "True";
	type = 3; // Boolean
}

middle_x
{
	description = "Middle X Offset";
	default = "True";
	type = 3; // Boolean
}

lower_x
{
	description = "Lower X Offset";
	default = "True";
	type = 3; // Boolean
}
`;

function getRandomOffset(max)
{
    return Math.floor(Math.random() * max);
}

function randomizeSidedefOffsets(sd)
{
    if(ScriptOptions.upper_x && sd.upperTexture != '-' && Data.textureExists(sd.upperTexture))
        sd.fields.offsetx_top = getRandomOffset(Data.getTextureInfo(sd.upperTexture).width);

    if(ScriptOptions.middle_x && sd.middleTexture != '-' && Data.textureExists(sd.middleTexture))
        sd.fields.offsetx_mid = getRandomOffset(Data.getTextureInfo(sd.middleTexture).width);        

    if(ScriptOptions.lower_x && sd.lowerTexture != '-' && Data.textureExists(sd.lowerTexture))
        sd.fields.offsetx_bottom = getRandomOffset(Data.getTextureInfo(sd.lowerTexture).width);        
}

Map.getSelectedLinedefs().forEach(ld => {
    if(ld.front != null)
        randomizeSidedefOffsets(ld.front);
    
    if(ld.back != null)
        randomizeSidedefOffsets(ld.back);
});