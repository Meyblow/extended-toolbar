# Extended Toolbar

A customizable toolbar plugin for osu!cc and osu!lazer.

Made this because the default toolbar is functional, but having the ability to actually arrange it the way you want is nicer.

### Features

**Custom layout**  
Move toolbar items between the left, center and right zones. Rearrange everything to fit your preferred layout.

**Edit mode**  
Enable edit mode and drag toolbar blocks around with a live preview.

**Floating island mode**  
Turn the standard full-width toolbar into a floating island. Adjust its width, corner radius, horizontal position and vertical offset.

**Screen adaptation & dark glow**  
Automatically adapts SongSelect panels with soft rounded corners and provides an adjustable top screen dark glow vignette behind the floating island.

**Visual customization**  
Change the toolbar height, background opacity and item spacing. Seamlessly hides rigid backgrounds inside ruleset selectors.

**Custom spacers**  
Add empty gaps, thin lines or dots between toolbar items.

**Profile customization**  
Change how your avatar, username, rank and PP are positioned inside the toolbar.

**Layout presets**  
Save multiple toolbar layouts as JSON presets and switch between them whenever you want.

**Import & export**  
Share your toolbar setup using compact share codes or import layouts created by someone else.

**Smart overlay positioning**  
Login and music overlays stay aligned with their corresponding buttons even after moving them around.

### Install

1. Go to the Releases tab
2. Download the latest `plugin-extended-toolbar-1.0.1.zip` (or `ExtendedToolbar.dll`)
3. Drop it into: `%APPDATA%\osu\osu-cc\plugins\extended-toolbar\`
4. Launch osu!cc
5. Open Settings or right-click the toolbar to start customizing

### Building

```bash
dotnet build -c Release
```

### Notes

* Requires osu!cc with plugin support.
* Layout presets are stored locally as JSON.
* Toolbar customization is done entirely in-game.
* You can switch between saved layouts without manually editing configuration files.

---

Meyblow — [osu! profile](https://osu.ppy.sh/users/39791134)
