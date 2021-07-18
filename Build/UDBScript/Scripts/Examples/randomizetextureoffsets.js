`#name Randomize Texture Offsets`;

`#description Randomized texture offsets. Distinct upper, middle, and lower offsets only work if the game configuration supports those local offsets.`;

`#scriptconfiguration

global_x
{
    description = "Global X Offset";
    default = "False";
    type = 3; // Boolean
}

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

// Gets a random number
function getRandomOffset(max)
{
    return Math.floor(Math.random() * max);
}

// Checks if the given name is a proper texture name and if the texture exists
function isValidTexture(texture)
{
    return texture != '-' && Data.textureExists(texture);
}

function randomizeSidedefOffsets(sd)
{
    // Global texture offset
    if(ScriptOptions.global_x && (isValidTexture(sd.upperTexture) || isValidTexture(sd.middleTexture) || isValidTexture(sd.lowerTexture)))
    {
        let widths = [];

        if(isValidTexture(sd.upperTexture))
            widths.push(Data.getTextureInfo(sd.upperTexture).width);

        if(isValidTexture(sd.middleTexture))
            widths.push(Data.getTextureInfo(sd.middleTexture).width);

        if(isValidTexture(sd.lowerTexture))
            widths.push(Data.getTextureInfo(sd.lowerTexture).width);

        if(widths.length > 0)
            sd.offsetX = getRandomOffset(Math.max(widths));
    }

    // Local texture offsets
    if(GameConfiguration.hasLocalSidedefTextureOffsets)
    {
        if(ScriptOptions.upper_x && isValidTexture(sd.upperTexture))
            sd.fields.offsetx_top = getRandomOffset(Data.getTextureInfo(sd.upperTexture).width);

        if(ScriptOptions.middle_x && isValidTexture(sd.middleTexture))
            sd.fields.offsetx_mid = getRandomOffset(Data.getTextureInfo(sd.middleTexture).width);

        if(ScriptOptions.lower_x && isValidTexture(sd.lowerTexture))
            sd.fields.offsetx_bottom = getRandomOffset(Data.getTextureInfo(sd.lowerTexture).width);
    }
}

// Randomize offset of front and back sidedefs of all selected linedefs
Map.getSelectedLinedefs().forEach(ld => {
    if(ld.front != null)
        randomizeSidedefOffsets(ld.front);
    
    if(ld.back != null)
        randomizeSidedefOffsets(ld.back);
});