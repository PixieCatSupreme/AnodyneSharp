About the project
-------------
AnodyneSharp is a fan rewrite of the game Anodyne by Analgesic Productions in C# using MonoGame.

It's based on the original game's source code, which can be found over here: https://github.com/analgesicproductions/Anodyne-1-Repo

The aim is to be faithful to the original, with some UX improvements and a more modular/moddable codebase.

Overview of changes
---------------

 - Render at 60 FPS using OpenGL/DirectX, effects are done using shaders instead of CPU-side computation
 - Controller support
   - Switch Pro controller support needs Steam to be active to work correctly
 - Indicator of currently selected broom in top left of the screen
   - Swapping between brooms doesn't require using the menu - PgUp/Dwn and controller shoulder buttons switch between them, 1-4 sets the current broom directly
 - Unopened chest indicator on minimaps on screens you've visited. No more searching for that one chest in the entire dungeon you missed
 - Minimap shows floor numbers for multi-floor dungeons
 - Return to entrance now returns you to the screen where you entered the dungeon(most notable in REDCAVE with its multiple entrances)
 - Bugfix: Dust at the edge of water acts more consistently


License
----
This code and assets are released under the same conditions as the Anodyne 1 license, this repo does not claim ownership over the IP.

For more information, check out the complete license in LICENSE.md.
