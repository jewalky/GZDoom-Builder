`#name Create Line Portal`;

`#scriptconfiguration

depth
{
	description = "Depth of space behind portal";
	default = 64;
	type = 0; // Integer
}

texture
{
	description = "Filler texture";
	default = "FIREBLU1";
	type = 6; // Texture
}
`;

// Function to draw the space behind the portal line
function drawPortalSector(line, depth)
{
    // The origin is the bottom left point of the rectanble
    let origin = line.v1;

    // The base points of the rectangle. There are five because
    // the first and last point need to be at the same position
    // to get a complete drawing
    let points = [
        new Vector2D(0, 0),
        new Vector2D(0, depth),
        new Vector2D(line.getLength(), depth),
        new Vector2D(line.getLength(), 0),
        new Vector2D(0, 0)
    ];

    // The points need to be rotated by the line's angle and moved
    // by the origin's position
    for(let i=0; i < points.length; i++)
    {
        let v = points[i].getRotated(line.getAngle() - 90);
        points[i] = new Vector2D(v.x + origin.x, v.y + origin.y);
    }

    // Draw the lines
    if(!Map.drawLines(points))
        throw 'Failed drawing space behind line ' + line;
    
    // Set the front middle texture for the new 1-sided linedefs
    Map.getMarkedLinedefs().filter(ld => ld.back == null).forEach(ld => ld.front.middleTexture = ScriptOptions.texture)
}

// The line action for portals only works in UDMF (GZDoom and Eternity Engine)
if(!Map.isUDMF)
    throw 'This script only works in UDMF maps';

// Get selected linedefs
let lines = Map.getSelectedLinedefs();

// Make sure exactly two lines are selected
if(lines.length != 2)
    throw 'You need to select exactly two lines';

// Both lines have to have the same length, otherwise the portal will
// be broken
if(lines[0].line.getLength() != lines[1].line.getLength())
    throw 'Both lines need to have the same length';

// Get a new tag to use for the portal
let newtag = Map.getNewTag();

// Set the action, arg, and tag of both lines
lines[0].action = lines[1].action = 301; // Line_QuickPortal
lines[0].args[0] = lines[1].args[0] = 0;
lines[0].tag = lines[1].tag = newtag;

// Draw the sectors behind the portal
drawPortalSector(lines[0].line, ScriptOptions.depth);
drawPortalSector(lines[1].line, ScriptOptions.depth);