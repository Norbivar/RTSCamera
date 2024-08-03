# RTS Camera
(Old name: EnhancedMission, Enhanced Mission RTS Camera)

A mod for Mount&Blade II Bannerlord that provides free camera and more features in battle.

## Dependencies
You will need these modules to enable RTS Camera:
- Harmony
- UIExtenderEx

## Save Compability
This mod does not read or write stuffs to your game save. Installing or removing this mod has no impact on your game save.

## Features

- RTS Camera

  - Free camera mode switchable at any time.

  - Control one of your soldiers and continue fighting/commanding after player character is injured in battle.

  - Watch any unit and optionally take control of any unit in player team.

  - Change time speed in battle, or pause the battle.

- Command System

  - Command your troops to attack any enemy formation.

  - Select your formation by middle clicking on units in formation.

## How to Use

- RTS Camera
  - You can press `F10` to switch to free camera and press again to switch back. Player character will be controlled by AI when switched to free camera.

    - You can press `L` and check `Use Free Camera By Default` to automatically switch to free camera when entering a battle.
  
  - ~~You can select your formations by clicking on troop cards.~~ You can press `Alt` to show the cursor and drag right mouse button to rotate the camera when cursor is shown.
  
  - After your character dies, you can press ~~`F`~~`E` to follow one of your soldiers and press ~~`F`~~`E` again to control it and continue fighting. Soldiers in the same formation as player character will be considered first when deciding which soldier to control.

    - You can immediately control one of your soldiers to avoid all your formations been delegated to AI when your charater dies. Press `L` and check `Control Ally After Death` option to enable this feature. It's always enabled in free camera mode to ensure a smooth gaming experience.

  - When the camera is following a character, you can press ~~`F`~~`E` to control the character. You can make the camera follow a character in the following ways:

    - In free camera press ~~`F`~~`E` to follow the player character.

    - You can make the camera follow any hero by pressing `L` to open the menu and selecting a hero in `Watch Another Hero` drop-down list.

    - You can make the camera follow any character by pressing `;` and clicking a character, then pressing ~~`F`~~`E`.

    - You can press left/right mouse button to change the character that the camera is following.

  - If your character is injured in campaign map and you encounter an enemy party, you can still choose "Watch the battle" option to begin the battle. In this battle you can command your troops in free camera but cannot directly control a character and fighting.

  - Pause game (`[`) or adjust time speed (`'`).

  - Toggle HUD by pressing `]`. If you rebind the key and forget the key set for toggling HUD, you can always use `HOME` key to enable HUD.

- Command System

  - You can make your formation **only charge to a specific enemy formation** by clicking middle mouse button on the enemy formation, rather than charging to all enemies when you press `F1` `F3`.

    - The target enemy formation that your selected troops are charging to will be highlighted with red outline when you open command panel.

    - AI formation will also have the ability to charge to a specific formation. Your formation targeted by the enemy formation will be highlighted with dark blue outline when you open command panel.

    - I didn't find a way to restrict range weapon's target to specific formation, so remember to hold fire when using this feature if they have javelins.

    - You can press `L` and uncheck `Enable Attack Specific Formation` to disable this feature.

  - You can select your formations by clicking middle mouse button on soldiers. Selected troops will be highlighted with green outline.

    - You can press `L` and uncheck `Middle Click to select formation` to disable this feature.

- You can rebind hotkeys by pressing `L` and click `Config Key`.

- Configuration will be automatically saved.

## How to Install
1. Remove any old installation of this mod. You can go to `Modules` folder of bannerlord installation folder (For example `C:\Program Files\Steam\steamapps\common\Mount & Blade II Bannerlord\Moudles\`) and remove folders like `RTSCamera` or `EnhancedMission`.

2. Copy `RTSCamera` and `RTSCamera.CommandSystem` folder you downloaded into Bannerlord `Modules` folder. Or use Vortex to install it automatically.

## Details

- Press `L` to open menu of this mod when in a scene. You can adjust all the options of this mod in it.

  - You can close the menu by pressing `esc`, left clicking outside the menu or right clicking.

- When in free camera:

  - Use `W`, `A`, `S`, `D`, `Space`, `Z` and mouse middle button to move the camera.

  - Use `shift` to speed up camera movement.

  - Move your mouse to rotate the camera, or when order panel is opened, drag right mouse button to rotate the camera.

  - Left click on the ground and drag to change the position, direction and width of the formation you selected.

    - Hold `ctrl` when dragging to arrange multiple formations vertically.

  - Hold `ctrl` and scroll mouse to adjust camera movement speed.

  - Hold `ctrl` and click middle mouse button to reset camera movement speed.

  - Hold `ctrl` and drag middle mouse button vertically to adjust camera height.
   
- The configuration is saved in directory `(user directory)\Documents\Mount and Blade II Bannerlord\Configs\RTSCamera\`.

  The main config is saved in file `RTSCameraConfig.xml`.

  The hot key config is saved in file `GameKeyConfig.xml`.

  You can modify them manually, but if you edit them incorrectly or remove them, the configuration will be reset to default.

## Troubleshoot
- If the launcher can not start:

  - Uninstall all the third-party mods and reinstall them one by one to detect which one cause the launcher cannot start.

- If it shows "Unable to initialize Steam API":

  - Please start steam first, and make sure that Bannerlord is in your steam account.

- If the game crashed after starting:

  - If you upgraded this mod from version lower than e3.2.0, pleas remove the old mod folder "EnhancedMission" to prevent crash.
  
  - I would appreciate it if you send dump file to me to help me to solve the crash by followinig steps below:

    - Click `Yes` when the game crashes and ask whether to collect information.

    - Before sending files to TaleWorlds, go to `C:\ProgramData\Mount and Blade II Bannerlord\crashes`(**Not the game installation path**) and find the folder related to the crash by timestamp.

    - Then send `dump.dmp` file in the folder to me.

- If you forget the hotkey set for opening menu:

  - You can remove the config file so that config will be reset to default.

## Optional File
- ["Improved Combat AI"](https://www.nexusmods.com/mountandblade2bannerlord/mods/449/): Adjusting combat AI.

- ["Cinematic Camera"](https://www.nexusmods.com/mountandblade2bannerlord/mods/1627): Adjusting camera parameters such as moving speed, depth of field, etc.

## Source Code

Source code should be available within `source` folder in release of this mod. Or you can get it at [github.com](https://github.com/lzh-mb-mod/RTSCamera).

## Contact with me
- Email:

  - lizhenhuan1019@outlook.com

  - lizhenhuan1019@qq.com
