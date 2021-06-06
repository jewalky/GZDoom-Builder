`#name Draw Voodoo Doll Closet`;

`#scriptconfiguration

length
{
	description = "Length of closet";
	default = 256;
	type = 0; // Integer
}

direction
{
	description = "Direction of closet";
	default = "North";
	type = 11; // Enum
	enumvalues {
		0 = "North";
		1 = "East";
		2 = "South";
		3 = "West";
	}
}

inactive
{
	description = "Start inactive";
	default = "True";
	type = 3; // Boolean
}

looping
{
	description = "Looping";
	default = "False";
	type = 3; // Boolean
}
`;

// Make sure the has a correct minimum length
if(ScriptOptions.length < 96)
	throw 'Voodoo doll closet has to be at least 96 map units long!';

// Get the mouse position in the map, snapped to the grid
let basepos = Map.snappedToGrid(Map.mousePosition);

// Closet width is static
let closetwidth = 64;

// Get the currently selected lines. Those will get actions to release the voodoo doll
let triggerlines = Map.getSelectedLinedefs();

// The number of tags we need depend on the selected options
let numnewtags = 1;
let newtagindex = 0;

// We need an additional tag if the player is blocked at the beginning
if(ScriptOptions.inactive)
	numnewtags++;

// We need an additional tag if the closet should be looping
if(ScriptOptions.looping)
	numnewtags++;

// Get thew new tags
let tags = Map.getMultipleNewTags(numnewtags);

// Create a pen for drawing geometry
var p = new Pen();

// Draw the closet
p.setAngleDegrees(90 * ScriptOptions.direction);
p.moveTo(basepos); p.drawVertex();
p.moveForward(ScriptOptions.length); p.drawVertex(); p.turnRight();
p.moveForward(closetwidth); p.drawVertex(); p.turnRight();
p.moveForward(ScriptOptions.length); p.drawVertex();

if(!p.finishDrawing())
	throw "Something went wrong while drawing!";

// Get the new sector, assign a tag, and set heights
let sector = Map.getMarkedSectors()[0];
sector.tag = tags[newtagindex];
sector.floorHeight = 0;
sector.ceilingHeight = 56;

// Draw the carrying line
p.setAngleDegrees(90 * ScriptOptions.direction);
p.moveTo(basepos); p.drawVertex();
p.moveForward(32); p.drawVertex();

if(!p.finishDrawing())
	throw 'Something went wrong while drawing!';

// Assign the action and tag to the line
let carryline = Map.getMarkedLinedefs()[0];
carryline.action = 252;
carryline.tag = tags[newtagindex];

// Increment the new tag index, so that the next new tag will be used for the next step
newtagindex++;

// Create the player blocking geometry if necessary
if(ScriptOptions.inactive)
{
	// Draw the blocking sector
	p.setAngleDegrees(90 * ScriptOptions.direction);
	p.moveTo(basepos);
	p.moveForward(64); p.turnRight(); p.moveForward(16); p.drawVertex();
	p.turnRight(); p.moveForward(8); p.drawVertex();
	p.turnLeft(); p.moveForward(closetwidth - 32); p.drawVertex();
	
	if(!p.finishDrawing())
		throw "Something went wrong while drawing!";
	
	// Get the new sectors and assign a tag
	sector = Map.getMarkedSectors()[0];
	sector.tag = tags[newtagindex];
	sector.floorHeight = 0;
	sector.ceilingHeight = 55;

	// Assign actions to release the voodoo doll to the previosly selected lines. If the line has a texture
	// starting with SW1 or SW2 a switch action will be applied. Otherwise a walk-over action is applied (but only if
	// it's a 2-sided line)
	triggerlines.forEach(tl => {
			if(	tl.front.upperTexture.startsWith('SW1') || tl.front.upperTexture.startsWith('SW2') ||
				tl.front.middleTexture.startsWith('SW1') || tl.front.middleTexture.startsWith('SW2') ||
				tl.front.lowerTexture.startsWith('SW1') || tl.front.lowerTexture.startsWith('SW2')
				)
			{
				tl.action = 166; // S1 Ceiling Raise to Highest Ceiling
				tl.tag = tags[newtagindex];
			}
			else if(tl.back != null)
			{
				tl.action = 40; // W1 Ceiling Raise to Highest Ceiling
				tl.tag = tags[newtagindex];
			}
	});

	// Increment the new tag index, so that the next new tag will be used for the next step
	newtagindex++;
}

// Create the looping teleporter geometry if necessary
if(ScriptOptions.looping)
{
	// Create the teleport destination line
	p.setAngleDegrees(90 * ScriptOptions.direction);
	p.moveTo(basepos);
	p.moveForward(32); p.turnRight(); p.moveForward(8); p.drawVertex();
	p.moveForward(closetwidth - 16); p.drawVertex();

	if(!p.finishDrawing())
		throw 'Something went wrong while drawing!';
	
	// The destination line only needs a tag and no action
	let line = Map.getMarkedLinedefs(true)[0];
	line.tag = tags[newtagindex];
	
	// Create the teleport line
	p.setAngleDegrees(90 * ScriptOptions.direction);
	p.moveTo(basepos);
	p.moveForward(ScriptOptions.length - 32); p.turnRight(); p.moveForward(8); p.drawVertex();
	p.moveForward(closetwidth - 16); p.drawVertex();
	
	if(!p.finishDrawing())
		throw 'Something went wrong while drawing!';
	
	// The teleport line needs a tag and an action
	line = Map.getMarkedLinedefs(true)[0];
	line.action = 263;
	line.tag = tags[newtagindex];
}

// Compute the new voodoo doll position
let newpos = new Vector2D(32, 32).GetRotated(Angle2D.DoomToReal(-90 * ScriptOptions.direction - 90));
newpos.x += basepos.x;
newpos.y += basepos.y;

// Get all player 1 starts
let playerthings = Map.getThings().filter(o => o.type == 1);

// The actual player always spawns on the last player 1 start that was placed, so
// we need to move the last player 1 start to the monster closet and create a new
// player 1 start at the old position
if(playerthings.length > 0)
{
	// Sort them by their index, so that the first element is that last player 1 start
	let pt = playerthings.sort((a, b) => b.index - a.index)[0];

	// Store old position and angle and move the last player 1 start to the closet
	let oldpos = pt.position;
	let oldangle = pt.angle;
	
	pt.position = newpos;
	pt.snapToAccuracy();

	// Create a new player 1 start and move it to the old position	
	let t = Map.createThing(oldpos, 1);
	t.angle = oldangle;
}
else
{
	let t = Map.createThing(newpos, 1);
	t.snapToAccuracy();
}