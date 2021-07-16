# What's still missing
## Wrappers
- `Line3D` wrapper

## Interop
- Make (parts of) game configuration (for example ThingTypeInfo) available?

## Map elements
- is the current implementation for thing and linedef action arguments OK?

## UI
- add favorite script functionality (in the UI and with hotkeys)
- add "quick script" function to write and run one-shot scripts in UDB without having to create files
- monitor filesystem for changes, so that scripts added to the folders at runtime will show up in the tree

## Other
- remove the `Wrapper` part from the wrapper class names?
- some way to get information about the current editing mode (name, ...)
- add a way to communicate with the user, like message boxes with "OK", "yes/no" etc.