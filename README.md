# HD Snapshot 1.0.0

[Nexus Mods page](https://www.nexusmods.com/kingdominc/mods/20)

Take crystal-clear, high-definition snapshots of your Kingdom Inc workshop layouts.

In the standard game, zooming out to capture a large factory shrinks machines to tiny sub-pixel clusters, aliases pipe connections, and blurs TextMeshPro text into unreadable smudges. **HD Snapshot** provides a dedicated box selection tool that scans the selected area at native close-up zoom (1:1 pixel scale, or 2x Ultra-HD supersampling) and stitches it into a pristine, high-resolution PNG image where every single card, text label, recipe icon, and pipe remains razor-sharp.

## Features

- **Box Selection Tool**: Click the camera icon in the edit hotbar or press **F9** to enter snapshot mode. Drag a rectangular box across any section of your factory. Live dimension and resolution counters show the exact width and height in pixels.
- **One-Click Factory Capture**: Press **Space** or **Enter** while the tool is active to automatically frame and capture your entire factory layout with clean padding.
- **True HD Offscreen Tiling**: Does not upscale a zoomed-out screen. The camera renders offscreen tiles at 1:1 close-up zoom with point-filtered text and full font clarity, stitching large areas seamlessly without exceeding GPU texture limits.
- **Instant Clipboard Paste**: Automatically copies the high-resolution image (and file path) to your Windows clipboard so you can press **Ctrl+V** straight into Discord, Reddit, Slack, or image editors.
- **Direct PNG Export**: Saves full-quality PNGs directly into your game's `Screenshots/` directory (`KingdomInc/Screenshots/`).
- **Foundation Integration**: Full in-game configuration menu under **Options -> Mod Settings -> HD Snapshot** (Resolution scale 1.0x / 1.5x / 2.0x, Background style: Workshop Grid / Transparent PNG / Dark Slate, auto-open folder, hotkey).

## Controls

| Action | Control |
| --- | --- |
| Activate Snapshot Tool | Click Hotbar Camera Button or press **F9** |
| Box Select Area | Left Click & Drag |
| Capture Entire Factory | Press **Space** or **Enter** while tool is active |
| Cancel Selection | Press **Escape** or **Right Click** |

## Requirements and installation

Requires Kingdom Inc, BepInEx 5 (Windows x64 Mono), and optionally **Foundation 0.5.0 or later** for in-game mod settings.

1. Install BepInEx 5 and optionally Foundation.
2. Close Kingdom Inc.
3. Extract the HD Snapshot ZIP so `HDSnapshot.dll` is located at `BepInEx/plugins/HDSnapshot/HDSnapshot.dll`.
4. Launch Kingdom Inc. The camera icon appears in your edit hotbar when viewing any workshop flow diagram.

## Updating and removal

Close the game before replacing `HDSnapshot.dll`.

HD Snapshot is a client-side visual utility. It does not alter your save files or introduce custom entities into save data. To remove it, simply close the game and delete the `BepInEx/plugins/HDSnapshot` folder. Your saves will load normally.

Author: Urakka.

[Source code](https://github.com/urakkaamyx/KingdomInc_HDSnapshot)

