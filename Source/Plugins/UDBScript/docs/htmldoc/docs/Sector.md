# Sector

## Properties
### index
The `Sector`'s index. Read-only.
### floorHeight
Floor height of the `Sector`.
### ceilingHeight
Ceiling height of the `Sector`.
### floorTexture
Floor texture of the `Sector`.
### ceilingTexture
Ceiling texture of the `Sector`.
### selected
If the `Sector` is selected or not.
### marked
If the `Sector` is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
### flags
`Sector` flags. It's an object with the flags as properties. Only available in UDMF.


```js
s.flags['noattack'] = true; // Monsters in this sector don't attack
s.flags.noattack = true; // Also works
```
### special
The `Sector`'s special type.
### tag
The `Sector`'s tag.
### brightness
The `Sector`'s brightness.
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
### getSidedefs()
Returns an `Array` of all `Sidedef`s of the `Sector`.
#### Return value
`Array` of the `Sector`'s `Sidedef`s
### clearFlags()
Clears all flags.
### copyPropertiesTo(s)
Copies the properties from this `Sector` to another.
#### Parameters
* s: the `Sector` to copy the properties to
### intersect(p)
Checks if the given point is in this `Sector` or not. The given point can be a `Vector2D` or an `Array` of two numbers.

```js
if(s.intersect(new Vector2D(32, 64)))
	log('Point is in the sector!');

if(s.intersect([ 32, 64 ]))
	log('Point is in the sector!');
```
#### Parameters
* p: Point to test
#### Return value
*missing*
### join(other)
Joins this `Sector` with another `Sector`. Lines shared between the sectors will not be removed.
#### Parameters
* other: Sector to join with
### delete()
Deletes the `Sector` and its `Sidedef`s.
### getTags()
Returns an `Array` of the `Sector`'s tags. UDMF only. Supported game configurations only.
#### Return value
`Array` of tags
### addTag(tag)
Adds a tag to the `Sector`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to add
#### Return value
`true` when the tag was added, `false` when the tag already exists
### removeTag(tag)
Removes a tag from the `Sector`. UDMF only. Supported game configurations only.
#### Parameters
* tag: Tag to remove
#### Return value
`true` when the tag was removed successfully, `false` when the tag did not exist
