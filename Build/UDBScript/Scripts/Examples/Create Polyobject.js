// Get the mouse position in the map, snapped to the grid
var cursorpos = Map.snappedToGrid(Map.mousePosition);

// The polyobject number that will be used
var polyobjectnumber = -1;

// Get selected linedefs
var lines = Map.getSelectedLinedefs();

// Make sure exactly one linedef is selected
if(lines.length != 1)
	throw 'You have to select exactly 1 line';

log(lines[0]);

// This stores polyobject numbers that are already used
var usednumbers = []

// Find all polyobject things and get their polyobject numbers
Map.getThings().filter(o => o.type >= 9300 && o.type <= 9303).forEach(o => {
		usednumbers.push(o.angle);
});

// Find the first free polyobject number
for(var i=1; i < 360; i++) {
	if(!usednumbers.includes(i)) {
		polyobjectnumber = i;
		break;
	}
}

// Make sure we actually found a free polyobject number
if(polyobjectnumber == -1)
	throw 'No free Polyobject numbers!';

// Set the line action and argument, and get the position where the
// polyobject anchor thing will be placed
lines[0].action = 1; // Polyobject Start Line
lines[0].args[0] = polyobjectnumber;
var anchorpos = lines[0].line.getCoordinatesAt(0.5); // Center of line

// Create the polyobject start spot thing
var t = Map.createThing(cursorpos, 9301); // 9301 = Polyobject Start Spot
t.angle = polyobjectnumber;

// Create the polyobject anchor thing
t = Map.createThing(anchorpos, 9300); // 9300 = Polyobject Anchor
t.angle = polyobjectnumber