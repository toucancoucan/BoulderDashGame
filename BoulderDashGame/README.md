﻿**Boulder Dash Game**

_I use Rider, so (unlikely) there can be troubles with opening it in VS_

**I added NetCoreAudio library in order to play sounds**

//TODO: 

Refactor `GameEngine.cs` - remove static fields and methodsand divide menu from game _**!important**_

Rewrite game interface: it refreshes all console but not a part 

rewrite infinite loops in threads

add music

add scores and leaders

introduce virtual

try to add emoji

~~refactor: game engine logic in inputProcessor~~

**How it now and how it be?**

now i initialize level1 in `Start()` method of `GameEngine.cs` and
then if `isGame` i use `gameLoop()` method of ???

Then i will remake it, so levels will be chosen from Menu process loop
and such level will be loaded through `gameLoop()`

`initializeLevel()` consists of:
creating matrix (generate or load from json [2x array]), setting logic ???

`gameLoop()` consists of: 
moveplayer, move enemies, triggers, drawing, 
