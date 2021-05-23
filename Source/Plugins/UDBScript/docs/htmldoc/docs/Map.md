# Map

## Properties
### isDoom
`true` if the map is in Doom format, `false` if it isn't. Read-only.
### isHexen
`true` if the map is in Hexen format, `false` if it isn't. Read-only.
### isUDMF
`true` if the map is in UDMF, `false` if it isn't. Read-only.
### mousePosition
The map coordinates of the mouse position as a `Vector2D`. Read-only.
## Methods
### snappedToGrid(pos)
Returns the given point snapped to the current grid.
#### Parameters
* pos: Point that should be snapped to the grid
#### Return value
Snapped position as `Vector2D`
### getThings()
Returns an `Array` of all `Thing`s in the map.
#### Return value
`Array` of `Thing`s
### getSectors()
Returns an `Array` of all `Sector`s in the map.
#### Return value
`Array` of `Sector`s
### getSidedefs()
Returns an `Array` of all `Sidedef`s in the map.
#### Return value
`Array` of `Sidedef`s
### getLinedefs()
Returns an `Array` of all `Linedef`s in the map.
#### Return value
`Array` of `Linedef`s
### stitchGeometry(mergemode = MergeGeometryMode.CLASSIC)
Stitches marked geometry with non-marked geometry.
#### Parameters
* mergemode: Mode to merge by
#### Return value
`true` if successful, `false` if failed
### snapAllToAccuracy(usepreciseposition = true)
Snaps all vertices and things to the map format accuracy. Call this to ensure the vertices and things are at valid coordinates.
#### Parameters
* usepreciseposition: `true` if decimal places defined by the map format should be used, `false` if no decimal places should be used
### getNewTag(usedtags = null)
Gets a new tag.
#### Parameters
* usedtags: `Array` of tags to skip
#### Return value
The new tag
### getMultipleNewTags(count)
Gets multiple new tags.
#### Parameters
* count: Number of tags to get
#### Return value
`Array` of the new tags
### nearestLinedef(pos, maxrange = double.NaN)
Gets the `Linedef` that's nearest to the specified position.
#### Parameters
* pos: Position to check against
* maxrange: Maximum range (optional)
#### Return value
Nearest `Linedef`
### nearestThing(pos, maxrange = double.NaN)
Gets the `Thing` that's nearest to the specified position.
#### Parameters
* pos: Position to check against
* maxrange: Maximum range (optional)
#### Return value
Nearest `Linedef`
### nearestVertex(pos, maxrange = double.NaN)
Gets the `Vertex` that's nearest to the specified position.
#### Parameters
* pos: Position to check against
* maxrange: Maximum range (optional)
#### Return value
Nearest `Vertex`
### nearestSidedef(pos)
Gets the `Sidedef` that's nearest to the specified position.
#### Parameters
* pos: Position to check against
* maxrange: Maximum range (optional)
#### Return value
Nearest `Sidedef`
### drawLines(data)
Draws lines. Data has to be an `Array` of `Array` of numbers, `Vector2D`s, `Vector3D`s, or objects with x and y properties. Note that the first and last element have to be at the same positions to make a complete drawing.

```js
Map.drawLines([
	new Vector2D(64, 0),
	new Vector2D(128, 0),
	new Vector2D(128, 64),
	new Vector2D(64, 64),
	new Vector2D(64, 0)
]);

Map.drawLines([
	[ 0, 0 ],
	[ 64, 0 ],
	[ 64, 64 ],
	[ 0, 64 ],
	[ 0, 0 ]
]);
```
#### Parameters
* data: `Array` of positions
#### Return value
`true` if drawing was successful, `false` if it wasn't
### clearAllMarks(mark=false)
Sets the `marked` property of all map elements. Can be passed `true` to mark all map elements.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### clearMarkedVertices(mark=false)
Sets the `marked` property of all vertices. Can be passed `true` to mark all vertices.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### clearMarkedThings(mark=false)
Sets the `marked` property of all `Thing`s. Can be passed `true` to mark all `Thing`s.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### clearMarkeLinedefs(mark=false)
Sets the `marked` property of all `Linedef`s. Can be passed `true` to mark all `Linedef`s.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### clearMarkeSidedefs(mark = false)
Sets the `marked` property of all `Sidedef`s. Can be passed `true` to mark all `Sidedef`s.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### clearMarkeSectors(mark = false)
Sets the `marked` property of all `Sector`s. Can be passed `true` to mark all `Sector`s.
#### Parameters
* mark: `false` to set the `marked` property to `false` (default), `true` to set the `marked` property to `true`
### invertAllMarks()
Inverts all marks of all map elements.
### invertMarkedVertices()
Inverts the `marked` property of all vertices.
### invertMarkedThings()
Inverts the `marked` property of all `Thing`s.
### invertMarkedLinedefs()
Inverts the `marked` property of all `Linedef`s.
### invertMarkedSidedefs()
Inverts the `marked` property of all `Sidedef`s.
### invertMarkedSectors()
Inverts the `marked` property of all `Sector`s.
### getMarkedVertices(mark=true)
Gets all marked (default) or unmarked vertices.
#### Parameters
* mark: `true` to get all marked vertices (default), `false` to get all unmarked vertices
#### Return value
*missing*
### getMarkedThings(mark = true)
Gets all marked (default) or unmarked `Thing`s.
#### Parameters
* mark: `true` to get all marked `Thing`s (default), `false` to get all unmarked `Thing`s
#### Return value
*missing*
### getMarkedLinedefs(mark = true)
Gets all marked (default) or unmarked `Linedef`s.
#### Parameters
* mark: `true` to get all marked `Linedef`s (default), `false` to get all unmarked `Linedef`s
#### Return value
*missing*
### getMarkedSidedefs(mark = true)
Gets all marked (default) or unmarked `Sidedef`s.
#### Parameters
* mark: `true` to get all marked `Sidedef`s (default), `false` to get all unmarked `Sidedef`s
#### Return value
*missing*
### getMarkedSectors(mark = true)
Gets all marked (default) or unmarked `Sector`s.
#### Parameters
* mark: `true` to get all marked `Sector`s (default), `false` to get all unmarked `Sector`s
#### Return value
*missing*
### markSelectedVertices(mark=true)
Marks (default) or unmarks all selected vertices.
#### Parameters
* mark: `true` to mark all selected vertices (default), `false` to unmark
### markSelectedLinedefs(mark = true)
Marks (default) or unmarks all selected `Linedef`s.
#### Parameters
* mark: `true` to mark all selected `Linedef`s (default), `false` to unmark
### markSelectedSectors(mark = true)
Marks (default) or unmarks all selected `Sector`s.
#### Parameters
* mark: `true` to mark all selected `Sector`s (default), `false` to unmark
### markSelectedThings(mark = true)
Marks (default) or unmarks all selected `Thing`s.
#### Parameters
* mark: `true` to mark all selected `Thing`s (default), `false` to unmark
### getSelectedVertices(selected=true)
Gets all selected (default) or unselected vertices.
#### Parameters
* selected: `true` to get all selected vertices, `false` to get all unselected ones
#### Return value
*missing*
### getSelectedThings(selected = true)
Gets all selected (default) or unselected `Thing`s.
#### Parameters
* selected: `true` to get all selected `Thing`s, `false` to get all unselected ones
#### Return value
*missing*
### getSelectedSectors(selected = true)
Gets all selected (default) or unselected `Sector`s.
#### Parameters
* selected: `true` to get all selected `Sector`s, `false` to get all unselected ones
#### Return value
*missing*
### getSelectedLinedefs(selected = true)
Gets all selected (default) or unselected `Linedef`s.
#### Parameters
* selected: `true` to get all selected `Linedef`s, `false` to get all unselected ones
#### Return value
*missing*
### getSidedefsFromSelectedLinedefs(selected = true)
Gets all `Sidedef`s from the selected `Linedef`s.
#### Parameters
* selected: `true` to get all `Sidedef`s of all selected `Linedef`s, `false` to get all `Sidedef`s of all unselected `Linedef`s
#### Return value
*missing*
### clearAllSelected()
Clears all selected map elements.
### clearSelectedVertices()
Clears all selected vertices.
### clearSelectedThings()
Clears all selected `Thing`s.
### clearSelectedSectors()
Clears all selected `Sector`s.
### createVertex(pos)
Creates a new `Vertex` at the given position. The position can be a `Vector2D` or an `Array` of two numbers.

```js
var v1 = Map.createVertex(new Vector2D(32, 64));
var v2 = Map.createVertex([ 32, 64 ]);
```
#### Parameters
* pos: Position where the `Vertex` should be created at
#### Return value
The created `Vertex`
### createThing(pos, type=0)
Creates a new `Thing` at the given position. The position can be a `Vector2D` or an `Array` of two numbers. A thing type can be supplied optionally.

```js
var t1 = Map.createThing(new Vector2D(32, 64));
var t2 = Map.createThing([ 32, 64 ]);
var t3= Map.createThing(new Vector2D(32, 64), 3001); // Create an Imp
var t4 = Map.createThing([ 32, 64 ], 3001); // Create an Imp
```
#### Parameters
* pos: Position where the `Thing` should be created at
* type: Thing type (optional)
#### Return value
The new `Thing`
