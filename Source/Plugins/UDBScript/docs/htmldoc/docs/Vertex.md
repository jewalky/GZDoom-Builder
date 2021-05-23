# Vertex

## Properties
### index
The vertex index. Read-only.
### position
Position of the `Vertex`. It's an object with `x` and `y` properties.
The `x` and `y` accept numbers:

```js
v.position.x = 32;
v.position.y = 64;
```
It's also possible to set all fields immediately by assigning either a `Vector2D`, or an array of numbers:

```js
v.position = new Vector2D(32, 64);
v.position = [ 32, 64 ];
```
### selected
If the `Vertex` is selected or not.
### marked
If the `Vertex` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
### ceilingZ
The ceiling z position of the `Vertex`. Only available in UDMF. Only available for supported game configurations.
### floorZ
The floor z position of the `Vertex`. Only available in UDMF. Only available for supported game configurations.
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
### getLinedefs()
Gets all `Linedefs` that are connected to this `Vertex`.
#### Return value
Array of linedefs
### copyPropertiesTo(v)
Copies the properties from this `Vertex` to another.
#### Parameters
* v: the vertex to copy the properties to
### distanceToSq(pos)
Gets the squared distance between this `Vertex` and the given point.
The point can be either a `Vector2D` or an array of numbers.

```js
v.distanceToSq(new Vector2D(32, 64));
v.distanceToSq([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the squared distance to.
#### Return value
Squared distance to `pos`
### distanceTo(pos)
Gets the distance between this `Vertex` and the given point.
The point can be either a `Vector2D` or an array of numbers.

```js
v.distanceTo(new Vector2D(32, 64));
v.distanceTo([ 32, 64 ]);
```
#### Parameters
* pos: Point to calculate the distance to.
#### Return value
Distance to `pos`
### nearestLinedef(pos)
Returns the `Linedef` that is connected to this `Vertex` that is closest to the given point.
#### Parameters
* pos: Point to get the nearest `Linedef` connected to this `Vertex` from
#### Return value
*missing*
### snapToAccuracy()
Snaps the `Vertex`'s position to the map format's accuracy.
### snapToGrid()
Snaps the `Vertex`'s position to the grid.
### join(other)
Joins this `Vertex` with another `Vertex`, deleting this `Vertex` and keeping the other.
#### Parameters
* other: `Vertex` to join with
### delete()
Deletes the `Vertex`. Note that this can result in unclosed sectors.
