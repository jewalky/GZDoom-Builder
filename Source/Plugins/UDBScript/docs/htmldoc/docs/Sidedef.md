# Sidedef

## Properties
### index
The `Sidedef`'s index. Read-only.
### isFront
`true` if this `Sidedef` is the front of its `Linedef`, otherwise `false`. Read-only.
### sector
The `Sector` the `Sidedef` belongs to. Read-only.
### line
The `Linedef` the `Sidedef` belongs to. Read-only.
### other
The `Sidedef` on the other side of this `Sidedef`'s `Linedef`. Returns `null` if there is no other. Read-only.
### angle
The `Sidedef`'s angle in degrees. Read-only.
### angleRad
The `Sidedef`'s angle in radians. Read-only.
### offsetX
The x offset of the `Sidedef`'s textures.
### offsetY
The y offset of the `Sidedef`'s textures.
### flags
`Sidedef` flags. It's an object with the flags as properties. Only available in UDMF.


```js
s.flags['noattack'] = true; // Monsters in this sector don't attack
s.flags.noattack = true; // Also works
```
### upperTexture
The `Sidedef`'s upper texture.
### middleTexture
The `Sidedef`'s middle texture.
### lowerTexture
The `Sidedef`'s lower texture.
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
