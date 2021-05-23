# Thing

## Properties
### index
Index of the `Thing`. Read-only.
### type
Type of the `Thing`.
### angle
Angle of the `Thing` in degrees, see https://doomwiki.org/wiki/Angle.
### angleRad
Angle of the `Thing` in radians.
### args
`Array` of arguments of the `Thing`. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.
### action
`Thing` action. Hexen and UDMF only.
### tag
`Thing` tag. UDMF only.
### selected
If the `Thing` is selected or not.
### marked
If the `Thing` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
### flags
`Thing` flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
Doom and Hexen:

```js
t.flags["8"] = true; // Set the ambush flag
```
UDMF:

```js
t.flags['ambush'] = true; // Set the ambush flag
t.flags.ambush = true; // Also works
```
### position
Position of the `Thing`. It's an object with `x`, `y`, and `z` properties. The latter is only relevant in Hexen format and UDMF.
The `x`, `y`, and `z` accept numbers:

```js
t.position.x = 32;
t.position.y = 64;
```
It's also possible to set all fields immediately by assigning either a `Vector2D`, `Vector3D`, or an array of numbers:

```js
t.position = new Vector2D(32, 64);
t.position = new Vector3D(32, 64, 128);
t.position = [ 32, 64 ];
t.position = [ 32, 64, 128 ];
```
### pitch
Pitch of the `Thing` in degrees. Only valid for supporting game configurations.
### roll
Roll of the `Thing` in degrees. Only valid for supporting game configurations.
### fields
UDMF fields. It's an object with the fields as properties.

```js
s.fields.comment = 'This is a comment';
s.fields['comment'] = 'This is a comment'; // Also  works
s.fields.xscalefloor = 2.0;
t.score = 100;
```
It is also possible to define new fields:

```js
s.fields.user_myboolfield = true;
```
There are some restrictions, though:

* it only works for fields that are not in the base UDMF standard, since those are handled directly in the respective class
* it does not work for flags. While they are technically also UDMF fields, they are handled in the `flags` field of the respective class (where applicable)
* JavaScript does not distinguish between integer and floating point numbers, it only has floating point numbers (of double precision). For fields where UDB knows that they are integers this it not a problem, since it'll automatically convert the floating point numbers to integers (dropping the fractional part). However, if you need to specify an integer value for an unknown or custom field you have to work around this limitation, using the `UniValue` class:

```js
s.fields.user_myintfield = new UniValue(0, 25); // Sets the 'user_myintfield' field to an integer value of 25
```
## Methods
### copyPropertiesTo(t)
Copies the properties from this `Thing` to another.
#### Parameters
* t: The `Thing` to copy the properties to
### clearFlags()
Clears all flags.
### snapToGrid()
Snaps the `Thing`'s position to the grid.
### snapToAccuracy()
Snaps the `Thing`'s position to the map format's accuracy.
### distanceToSq(pos)
Gets the squared distance between this `Thing` and the given point.
The point can be either a `Vector2D` or an array of numbers.

```js
t.distanceToSq(new Vector2D(32, 64));
t.distanceToSq([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the squared distance to.
#### Return value
Distance to `pos`
### distanceTo(pos)
Gets the distance between this `Thing` and the given point. The point can be either a `Vector2D` or an array of numbers.

```js
t.distanceToSq(new Vector2D(32, 64));
t.distanceToSq([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the distance to.
#### Return value
Distance to `pos`
### delete()
Deletes the `Thing`.
