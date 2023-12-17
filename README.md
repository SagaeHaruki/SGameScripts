### 🔨 SGame NPC & Player Scripts :
3D Game using Unity Engine
---

<h1>Dev's Note:</h1>

- Note that sometime in the future this will be sorted base on the how many objects will be added to the game.
- If Some Objects can be added here then i will also publicly allow anyone to use it.

<h1>Future plans:</h1>

- Attacking System
- Improved Animation System
- Code Cleanup
- Sounds based on the terrain
- Day/Night Cycle
- NPC Movement & Interaction
- Might USE Procedural animation on some movements

<h1>Current Changes:</h1>

[Dec. 17, 2023]
- Added a version 4 of the player movement script
- New PlayerState System indicates if the player is Idling, Walking, Moving, Jumping, etc...
- New Gravity System with the v4
- New Jumping System with the v4
- New Slope Detection with the v4
- New Jump motion when moving on walking state (small jump forward instead of just upward only)
- Gravity Adjustments

<h1>Previous Changes</h1>

[Dec. 16, 2023]
- Fix some part of the jumping system causing animation & movement bug
- Jump height adjustment (Current jump height is too high)

[Dec. 15, 2023]
- Gravity Adjustments
- Jump Height Adjustments, (Gravity adjustments affected jump height)
- Falling Character System
- Wall Detection (Beta)

[Dec. 14, 2023]
- Speed Adjustments
- Jumping Height Adjustments
- Fall Distance Detection
- Jump height change based on the movement (because of the animation)

[Dec. 13, 2023]
- Speed change based on the angle of the slope and if the player is moving up or down
- Adjustments to speed
- IK System Pelvis movement adjustment based on the angle of the foot

[Dec. 12, 2023]
- Minor adjustments to ik system
- Adding slope detections (for speed change)
- Adjusting pelvis movement
- Minor adjustments to the jumping method
- Preventing movements when jumping (including the booleans)
- Re-Coding some parts of the old player movement system

[Dec. 11, 2023]
- Adjustments to the IK System.
- Fix Pelvis height on the ik system (When going up or down a slope or stairs).
- New Animation System

[Dec. 10, 2023]
> [Current Player Movement Script]
  - Some minor adjustment
  - Adjustment to the Character Control
  - Character can step over small obstacles (no animations yet)

> [New Player Movement Script]
  - Cleaner code
  - Better Jumping System
  - Better Gravity Physics

[Dec. 9, 2023]
- Trying to fix a jumping issue (Has something to do with the Gravity)
- Trying to create a new Gravity Logic
- A V4 of a PlayerMovement script might happen (Will be putted on the V3 Folder)

[Dec. 7, 2023]
- Fix the Curves of the animator for the IK System (Not included in this package)
- Adjust some part of the IK System
- Added movement & jump detection for the ik system to stop the player from staying at the ground
- Added a system on when the player is moving, i changes the value of the height & ground detection for the raycast

[Dec. 6, 2023]
- Fine Tuning the IK System to make it smooth
- Trying to fix the stairs issue (Bouncy character & Camera)
- Might change from freelook camera to virtual camera

[Dec. 4, 2023]
- Fully Functioning IK System (Reference from other projects)
- Trying to fix character stairs climbing making the character glitching out
- Animator controls fix

[Dec. 3, 2023]
- Fixed Bunny Hop
- Added jump motion (stops the player movement)
- Trying to use IK (Inverse Kinematics)
- Functioning IK [GMT + 09:00] (Currenly not as accurate like the others)

[Dec. 1, 2023]
- Tring to fix some animation problems for the game
- Trying to fix a bunny hop movement

[Nov. 30, 2023]
- V3 of the Scripts now available
- Switched back to Character Controler instead of rigidboy (i have my own reasons lol)

[Nov. 26, 2023]
- Creating a different methods on controlling player
- Using RigidBody to control instead of Character control, for more realisting physics

[Nov. 25, 2023]
- Player Control files, some animations controls are fixed
- Changed character speed (Sprint, Run, Walk)
- Jump Animation Added
- Dampening of camera movement

[Nov. 24, 2023]
- Fixed where the character doesn't regen stamina, (This happens when the player is not moving but holding the sprint key).
- Fix Camera Sensitivity, (On full screen, the sensitivity is different from when it is on a small window).
- The Character can now jump even when not moving, (This was disabled by me).

[Nov. 23, 2023]
- Added Minimap & Stamina bar.
- Added Dynamic color bases on current stamina.
- Added player navigation icon, camera direction icon, and custom minimap icon.
- Player icon moves based on the players direction (not camera).
- Camera icon moves based on the camera on where it is poiting.

[Nov. 19, 2023]
- Added Characters & Terrain (On Development).
- Animations Introduced.
- Added Toggle Cursor.
- On Cursor toggled, the camera movement is disabled.
- Changes to Sprint, Running and Walking Speed.
- Removed Dashing Ability.

[Nov. 18, 2023]
- Removed the player collider (Doesn't need this).
- Removed the old camera movement (Am currently using a cinemachine for the camera movement).
- Added a camera zoom script.
- Added smoothening to the camera zoom script.

[Nov. 15, 2023]
- Fixing the camera movements.
- Trying to fix on which the camera can freely move while the direction of movement is the direction of the player is facing.
