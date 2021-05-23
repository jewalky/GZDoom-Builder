# Line2D

## Constructors
### Line2D(v1, v2)
Creates a new `Line2D` from two points.

```js
let line1 = new Line2D(new Vector2D(32, 64), new Vector2D(96, 128));
let line2 = new Line2D([ 32, 64 ], [ 96, 128 ]);
```
#### Parameters
* v1: First point
* v2: Second point
## Static methods
### areIntersecting(line1, line2, bounded=true)
Checks if two lines intersect. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* line1: First `Line2D`
* line2: Second `Line2D`
* bounded: `true` to use finite length of lines, `false` to use infinite length of lines
#### Return value
`true` if the lines intersect, `false` if they do not
### areIntersecting(a1, a2, b1, b2, bounded=true)
Checks if two lines defined by their start and end points intersect. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* a1: First point of first line
* a2: Second point of first line
* b1: First point of second line
* b2: Second point of second line
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
`true` if the lines intersect, `false` if they do not
### getIntersectionPoint(a1, a2, b1, b2, bounded = true)
Returns the intersection point of two lines as `Vector2D`. If the lines do not intersect the `x` and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* a1: First point of first line
* a2: Second point of first line
* b1: First point of second line
* b2: Second point of second line
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
The intersection point as `Vector2D`
### getSideOfLine(v1, v2, p)
Returns which the of the line defined by its start and end point a given point is on.
#### Parameters
* v1: First point of the line
* v2: Second point of the line
* p: Point to check
#### Return value
`< 0` if `p` is on the front (right) side, `> 0` if `p` is on the back (left) side, `== 0` if `p` in on the line
### getDistanceToLine(v1, v2, p, bounded=true)
Returns the shortest distance from point `p` to the line defined by its start and end points. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* v1: First point of the line
* v2: Second point of the line
* p: Point to get the distance to
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
The shortest distance to the line
### getDistanceToLineSq(v1, v2, p, bounded = true)
Returns the shortest square distance from point `p` to the line defined by its start and end points. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* v1: First point of the line
* v2: Second point of the line
* p: Point to get the distance to
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
The shortest square distance to the line
### getNearestOnLine(v1, v2, p)
Returns the offset coordinate on the line nearest to the given point. `0.0` being on the first point, `1.0` being on the second point, and `u = 0.5` being in the middle between the points.
#### Parameters
* v1: First point of the line
* v2: Second point of the line
* p: Point to get the nearest offset coordinate from
#### Return value
The offset value relative to the first point of the line.
### getCoordinatesAt(v1, v2, u)
Returns the coordinate on a line defined by its start and end points as `Vector2D`.
#### Parameters
* v1: First point of the line
* v2: Second point of the line
* u: Offset coordinate relative to the first point of the line
#### Return value
Point on the line as `Vector2D`
## Properties
### v1
`Vector2D` position of start of the line.
### v2
`Vector2D` position of end of the line.
## Methods
### getCoordinatesAt(u)
Returns the coordinates on the line, where `u` is the position between the first and second point, `u = 0.0` being on the first point, `u = 1.0` being on the second point, and `u = 0.5` being in the middle between the points.
#### Parameters
* u: Position on the line, between 0.0 and 1.0
#### Return value
Position on the line as `Vector2D`
### getLength()
Returns the length of the `Line2D`.
#### Return value
Length of the `Line2D`
### getAngleRad()
Returns the angle of the `Line2D` in radians.
#### Return value
Angle of `Line2D` in radians
### getAngle()
Return the angle of the `Line2D` in degrees.
#### Return value
Angle of the `Line2D` in degrees
### getPerpendicular()
Returns the perpendicular of this line as `Vector2D`.
#### Return value
Perpendicular of this line as `Vector2D`
### isIntersecting(ray, bounded=true)
Checks if the given `Line2D` intersects this line. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* ray: `Line2D` to check against
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
`true` if lines intersect, `false` if they do not intersect
### isIntersecting(a1, a2, bounded = true)
Checks if the given line intersects this line. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* a1: First point of the line to check against
* a2: Second point of the line to check against
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
`true` if the lines intersect, `false` if they do not
### getIntersectionPoint(a1, a2, bounded = true)
Returns the intersection point of of the given line defined by its start and end points with this line as `Vector2D`. If the lines do not intersect the `x` and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* a1: First point of first line
* a2: Second point of first line
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
The intersection point as `Vector2D`
### getIntersectionPoint(ray, bounded=true)
Returns the intersection point of of the given line with this line as `Vector2D`. If the lines do not intersect the `x` and `y` properties of the `Vector2D` are `NaN`. If `bounded` is set to `true` (default) the finite length of the lines is used, otherwise the infinite length of the lines is used.
#### Parameters
* ray: Other `Line2D` to get the intersection point from
* bounded: `true` (default) to use finite length of lines, `false` to use infinite length of lines
#### Return value
The intersection point as `Vector2D`
### getSideOfLine(p)
Returns which the of the line defined by its start and end point a given point is on.
#### Parameters
* p: Point to check
#### Return value
`< 0` if `p` is on the front (right) side, `> 0` if `p` is on the back (left) side, `== 0` if `p` in on the line
