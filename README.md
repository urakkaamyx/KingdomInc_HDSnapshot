# HD Snapshot

[GitHub releases](https://github.com/urakkaamyx/KingdomInc_HDSnapshot/releases) · [Nexus Mods](https://www.nexusmods.com/kingdominc/mods/20) · Version 1.0.0

High-definition box selection and panoramic offscreen screenshot capture for Kingdom Inc workshop flows.

Capture any section of your factory or your entire workshop layout in crisp, ultra-high-definition detail without screen resolution or GPU texture limits.

## Features

- **Box Selection Capture:** Drag-and-drop box selection tool to frame and capture any area of your workshop.
- **1:1 Close-Up & 2x Supersampling:** Emulates native close-up camera zoom with crisp TextMeshPro text and point-filtered font rendering.
- **Offscreen Tiled Rendering:** Renders seamlessly in memory across multiple tiles, bypassing GPU texture limits and avoiding VRAM exhaustion.
- **Instant Clipboard Export:** Automatically copies captured image bitmaps directly to the Windows clipboard for instant pasting into Discord, Reddit, or image editors.
- **Automatic PNG File Saving:** Automatically saves full-resolution PNG files to your game's `Screenshots/` directory.
- **Foundation Settings Integration:** Configure resolution scale, hotkey bindings, background style (Workshop Grid, Transparent PNG, or Dark Slate), and folder auto-open through Foundation.

## Usage and Controls

1. Open any workshop flow diagram.
2. Click the camera icon in the bottom-left edit hotbar or press **F9**.
3. **Box Select:** Left-click and drag across any area of your workshop. A live dimensions badge displays pixel size and extent.
4. **Capture All:** Press **Space** or **Enter** while the tool is active to automatically frame and capture your entire factory with clean margins.
5. **Cancel:** Press **Escape** or **Right-Click** to cancel selection.

## Requirements

- Kingdom Inc (Windows x64 Mono)
- [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.6) (5.4.23.6 or newer)
- [Foundation](https://www.nexusmods.com/kingdominc/mods/18) (0.11.0 or newer)

## Installation

1. Install BepInEx 5 for Kingdom Inc.
2. Install Foundation (0.11.0 or newer).
3. Extract the release ZIP into your Kingdom Inc game directory.
4. Verify the file exists at `BepInEx/plugins/HDSnapshot/HDSnapshot.dll`.
5. Launch the game and enter any workshop to use the camera tool!

## Compatibility

No Harmony patches or native method hooks are used. HD Snapshot operates entirely via Unity GameObject lifecycle, native UI button attachment, and offscreen camera rendering.

- **Non-destructive:** Does not modify game save data, factory simulation, or kingdom state.
- **Save file safe:** Safe to install or remove at any time without impacting worlds or saves.
- **UI integration:** Seamlessly attaches to the workshop editing toolbar alongside vanilla tools and other UI mods.

## Removal

Close the game and delete `BepInEx/plugins/HDSnapshot/`.

## Credits and Source

Created by **Urakka** (rockyamyx). [Source code on GitHub](https://github.com/urakkaamyx/KingdomInc_HDSnapshot). Game artwork belongs to its respective owners.
