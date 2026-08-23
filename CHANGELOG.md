# Changelog

All notable changes to Extended Toolbar are documented in this file.

## [1.0.6] - 2026-08-23

### Fixed
- **Fullscreen Notification Overlay**: Made `NotificationOverlay` occupy the full viewport (`RelativeSizeAxes = Axes.Both`), resolving issues where toast notifications and left-side sliding sidebars were constrained to the default right-hand 400px column.
- **Direct Child Flow Reordering**: Fixed `ToolbarProfileLayoutManager` to directly manipulate immediate children of `ToolbarUserButton.Flow` (`usernameText` and `avatarContainer`), eliminating repetitive `May not add a drawable to multiple containers` exceptions.

## [1.0.5] - 2026-08-23

### Fixed
- **Component Lifecycle & Settings Binding Activation**: Migrated all bindable event handlers directly into constructors and properly added `NotificationLayoutManager` and `ToolbarProfileLayoutManager` into the active scene graph, fixing non-responsive settings changes.
- **Profile Button Formatting & Dynamic Shrinking**: Added `BypassAutoSizeAxes = Axes.Both` when hiding username or avatar, enabling the profile button to cleanly shrink to the exact width of the single visible element.
- **Rank & PP Stats Positioning**: Correctly targeted `TransientUserStatisticsUpdateDisplay` inside `ToolbarUserButton` to anchor statistics popups to the Left or Right of the user profile.
- **Dual Toast & Overlay Anchor Synchronization**: Synchronized `Anchor`, `Origin`, and `Margin` across both `toastTray` and `toastFlow` for all 5 monitor positions, with seamless left/right sliding animations for the notification sidebar.

## [1.0.4] - 2026-08-23

### Fixed
- **Avatar & Username Layout Reordering**: Fixed physical child reordering in `ToolbarProfileLayoutManager` so selecting `Avatar on left (Avatar | Username)` immediately moves the avatar to the left of the username on the toolbar.
- **Dynamic ToastTray Resolution**: Implemented lazy component resolution for `toastTray` and `mainContent` in `NotificationLayoutManager`, ensuring toast notifications reliably appear in the chosen screen corner (Top Left, Top Centre, Top Right, Bottom Left, Bottom Right) even when notification components load asynchronously.

## [1.0.3] - 2026-08-23

### Fixed
- **Notification Overlay & Sidebar Opening Fix**: Rewrote `NotificationOverlay` patch to non-destructive `Postfix` hooks, ensuring native `VisibilityContainer` state management, sounds, and focus work seamlessly while animating `mainContent` from Left or Right side.
- **Accurate Toast Notifications Positioning**: Directly targeted `toastTray` component inside `NotificationOverlay` to reliably position floating toast popups across all 5 screen zones (Top Left, Top Centre, Top Right, Bottom Left, Bottom Right).

## [1.0.2] - 2026-08-23

### Added
- **Interactive Toast Position Monitor Selector (`ToastPositionMonitorSelector`)**: A stylized mini-monitor widget in settings allowing users to visually click and select where floating toast notifications appear on screen (Top Left, Top Centre, Top Right, Bottom Left, Bottom Right) with animated mini-toast preview pills.
- **Notification Overlay Sidebar Side Switching**: Added option to open the full notification panel from either the Right (default) or Left side of the screen, with automatic anchor, shadow orientation and smooth sliding animations.
- **Max Visible Toasts Limit**: Configurable slider (1 to 5) to control how many toast popups can be visible on screen simultaneously.
- **Dedicated Notifications & Sidebar Settings Section**: New settings category grouping all notification customization options.

## [1.0.1] - 2026-08-23

### Added
- **Top Screen Dark Glow / Vignette**: Adjustable background dark gradient slider (0% to 100%) positioned behind the toolbar to smoothly blend screen headers and backgrounds.
- **Screen Adaptation for Floating Island (`ToolbarScreenAdapter`)**: Automatically applies soft rounded corners and subtle padding to SongSelect panels (`BeatmapInfoWedge` and `FilterControl`).
- **Seamless Ruleset Selector**: Hides solid opaque background rectangles inside `ToolbarRulesetSelector` for floating and transparent toolbar styles.
- **Structured 4-Section Settings Subsection**: Reorganized settings into 4 structured sections with headers and dividers:
  - Layout & Presets
  - Floating Island & Geometry
  - Background Effects & Glow
  - Profile & Spacers
- **Compact 2x2 Action Button Grid (`SettingsDoubleButtonRow`)**: Replaced full-width vertical buttons with compact paired rows, reducing menu height by ~60%.

## [1.0.0] - 2026-08-23

### Added
- **Initial Release (Split from osu!tweaks)**:
  - Complete modular toolbar manager supporting Left, Center, and Right zones.
  - Interactive Edit Mode with Drag & Drop element reordering.
  - Context menu for hiding elements, moving between zones, and adding custom spacers (Blank, Line, Dot).
  - Floating Island dock mode with configurable Corner Radius, Width (Length), Horizontal Offset X, and Vertical Offset Y.
  - Toolbar item density spacing slider.
  - Neon Glow underline with customizable accent colors (Pink, Purple, Cyan, Lime, Gold, White) and offset.
  - User profile customization (Avatar/Username placement, Rank and PP stats positioning on Left or Right).
  - Named JSON preset management with import/export Base64 share codes.
  - Dynamic overlay positioning keeping Login and Music overlays anchored beneath moved toolbar buttons.
  - Automatic screen-based ruleset selector visibility handling.
