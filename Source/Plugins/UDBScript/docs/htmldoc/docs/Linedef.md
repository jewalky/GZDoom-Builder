# Linedef

## Properties
### index
The linedef's index. Read-only.
### start
The linedef's start `Vertex`.
### end
The linedef's end `Vertex`.
### front
The `Linedef`'s front `Sidedef`. Is `null` when there is no front (should not happen).
### back
The `Linedef`'s back `Sidedef`. Is `null` when there is no back.
### line
The `Line2D` from the `start` to the `end` `Vertex`.
### selected
If the `Linedef` is selected or not.
### marked
If the `Linedef` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
### flags
`Linedef` flags. It's an object with the flags as properties. In Doom format and Hexen format they are identified by numbers, in UDMF by their name.
Doom and Hexen:

```js
ld.flags['64'] = true; // Set the block sound flag
```
UDMF:

```js
ld.flags['blocksound'] = true; // Set the block sound flag
ld.flags.blocksound = true; // Also works
```
### args
`Array` of arguments of the `Linedef`. Number of arguments depends on game config (usually 5). Hexen format and UDMF only.
### action
`Linedef` action.
### tag
`Linedef` tag. UDMF only.
### lengthSq
The `Linedef`'s squared length. Read-only.
### length
The `Linedef`'s length. Read-only.
### lengthInv
1.0 / length. Read-only.
### angle
The `Linedef`'s angle in degree. Read-only.
### angleRad
The `Linedef`'s angle in radians. Read-only.
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
### copyPropertiesTo(other)
Copies the properties of this `Linedef` to another `Linedef`.
#### Parameters
* other: The `Linedef` to copy the properties to
### clearFlags()
Clears all flags.
### flipVertices()
Flips the `Linedef`'s vertex attachments.
### flipSidedefs()
Flips the `Linedef`'s `Sidedefs`.
### getSidePoint(front)
Gets a `Vector2D` for testing on one side. The `Vector2D` is on the front when `true` is passed, otherwise on the back.
#### Parameters
* front: `true` for front, `false` for back
#### Return value
`Vector2D` that's either on the front of back of the Linedef
### getCenterPoint()
Gets a `Vector2D` that's in the center of the `Linedef`.
#### Return value
`Vector2D` in the center of the `Linedef`
### applySidedFlags()
Automatically sets the blocking and two-sided flags based on the existing `Sidedef`s.
### nearestOnLine(pos)
Get a `Vector2D` that's *on* the line, closest to `pos`. `pos` can either be a `Vector2D`, or an array of numbers.

```js
var v1 = ld.nearestOnLine(new Vector2D(32, 64));
var v2 = ld.nearestOnLine([ 32, 64 ]);
```
#### Parameters
* pos: Point to check against
#### Return value
`Vector2D` that's on the linedef
### safeDistanceToSq(pos, bounded)
Gets the shortest "safe" squared distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Squared distance to the line
### safeDistanceTo(pos, bounded)
Gets the shortest "safe" distance from `pos` to the line. If `bounded` is `true` that means that the not the whole line's length will be used, but `lengthInv` less at the start and end.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Distance to the line
### distanceToSq(pos, bounded)
Gets the shortest squared distance from `pos` to the line.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Squared distance to the line
### distanceTo(pos, bounded)
Gets the shortest distance from `pos` to the line.
#### Parameters
* pos: Point to check against
* bounded: `true` if only the finite length of the line should be used, `false` if the infinite length of the line should be used
#### Return value
Distance to the line
### sideOfLine(pos)
Tests which side of the `Linedef` `pos` is on. Returns < 0 for front (right) side, > for back (left) side, and 0 if `pos` is on the line.
#### Parameters
* pos: Point to check against
#### Return value
< 0 for front (right) side, > for back (left) side, and 0 if `pos` is on the line
### split(v)
Splits the `Linedef` at the given `Vertex`. The result will be two lines, from the start `Vertex` of the `Linedef` to `v`, and from `v` to the end `Vertex` of the `Linedef`.
#### Parameters
* v: `Vertex` to split by
#### Return value
The newly created `Linedef`
### delete()
Deletes the `Linedef`. Note that this will result in unclosed `Sector`s unless it has the same `Sector`s on both sides.
### getTags()
Returns an `Array` of the `Linedef`'s tags. UDMF only. Supported game configurations only.
#### Return value
`Array` of tags
### addTag(tag)
Adds a tag to the `Linedef`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to add
#### Return value
`true` when the tag was added, `false` when the tag already exists
### removeTag(tag)
Removes a tag from the `Linedef`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to remove
#### Return value
`true` when the tag was removed successfully, `false` when the tag did not exist
