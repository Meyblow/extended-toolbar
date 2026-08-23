using osu.Framework.Localisation;
using osucc.Localisation;

namespace ExtendedToolbar.Localisation
{
    public static class ExtendedToolbarStrings
    {
        private const string prefix = "extended-toolbar";
        private static string getKey(string name) => $"{prefix}:{name}";

        public static LocalisableString PluginName => OsuCcLocalisation.Get($"{prefix}:name", "Extended Toolbar");
        public static LocalisableString PluginDescription => OsuCcLocalisation.Get($"{prefix}:description", "Complete modular and customizable toolbar for osu!cc with custom layout, styles, positioning, and zones.");

        // Subsection Header
        public static LocalisableString Header => OsuCcLocalisation.Get(getKey(nameof(Header)), "Extended Toolbar");

        // Section Headers
        public static LocalisableString SectionLayoutPresets => OsuCcLocalisation.Get(getKey(nameof(SectionLayoutPresets)), "Layout & Presets");
        public static LocalisableString SectionFloatingIsland => OsuCcLocalisation.Get(getKey(nameof(SectionFloatingIsland)), "Floating Island & Geometry");
        public static LocalisableString SectionBackgroundEffects => OsuCcLocalisation.Get(getKey(nameof(SectionBackgroundEffects)), "Background Effects & Glow");
        public static LocalisableString SectionNotifications => OsuCcLocalisation.Get(getKey(nameof(SectionNotifications)), "Notifications & Sidebar");
        public static LocalisableString SectionProfileSpacers => OsuCcLocalisation.Get(getKey(nameof(SectionProfileSpacers)), "Profile & Spacers");

        // Section: Notifications & Sidebar
        public static LocalisableString ToastPositionLabel => OsuCcLocalisation.Get(getKey(nameof(ToastPositionLabel)), "Toast Notification Position");
        public static LocalisableString ToastPosTopRight => OsuCcLocalisation.Get(getKey(nameof(ToastPosTopRight)), "Top Right (Default)");
        public static LocalisableString ToastPosTopCentre => OsuCcLocalisation.Get(getKey(nameof(ToastPosTopCentre)), "Top Centre");
        public static LocalisableString ToastPosTopLeft => OsuCcLocalisation.Get(getKey(nameof(ToastPosTopLeft)), "Top Left");
        public static LocalisableString ToastPosBottomRight => OsuCcLocalisation.Get(getKey(nameof(ToastPosBottomRight)), "Bottom Right");
        public static LocalisableString ToastPosBottomLeft => OsuCcLocalisation.Get(getKey(nameof(ToastPosBottomLeft)), "Bottom Left");
        public static LocalisableString MaxVisibleToastsSlider => OsuCcLocalisation.Get(getKey(nameof(MaxVisibleToastsSlider)), "Max Visible Toasts");
        public static LocalisableString NotificationSidebarPositionDropdown => OsuCcLocalisation.Get(getKey(nameof(NotificationSidebarPositionDropdown)), "Sidebar Slide-in Direction");
        public static LocalisableString SidebarPosRight => OsuCcLocalisation.Get(getKey(nameof(SidebarPosRight)), "Right (Default)");
        public static LocalisableString SidebarPosLeft => OsuCcLocalisation.Get(getKey(nameof(SidebarPosLeft)), "Left (Slide from left)");

        // Section 1: Toolbar & Presets
        public static LocalisableString PresetDropdownLabel => OsuCcLocalisation.Get(getKey(nameof(PresetDropdownLabel)), "Layout Preset");
        public static LocalisableString DefaultPresetName => OsuCcLocalisation.Get(getKey(nameof(DefaultPresetName)), "Default");
        public static LocalisableString ImportedPresetName => OsuCcLocalisation.Get(getKey(nameof(ImportedPresetName)), "Imported Layout");

        public static LocalisableString ButtonEnterEditMode => OsuCcLocalisation.Get(getKey(nameof(ButtonEnterEditMode)), "Customize Toolbar (Edit Mode)");
        public static LocalisableString ButtonSavePreset => OsuCcLocalisation.Get(getKey(nameof(ButtonSavePreset)), "Save as Preset...");
        public static LocalisableString ButtonCopyCode => OsuCcLocalisation.Get(getKey(nameof(ButtonCopyCode)), "Share Layout Code...");
        public static LocalisableString ButtonImportCode => OsuCcLocalisation.Get(getKey(nameof(ButtonImportCode)), "Import Layout...");
        public static LocalisableString ButtonOpenPresetsFolder => OsuCcLocalisation.Get(getKey(nameof(ButtonOpenPresetsFolder)), "Open Presets Folder");
        public static LocalisableString ButtonResetToDefault => OsuCcLocalisation.Get(getKey(nameof(ButtonResetToDefault)), "Reset to Default Layout");

        // Section 2: Aesthetics
        public static LocalisableString FloatingIslandCheckbox => OsuCcLocalisation.Get(getKey(nameof(FloatingIslandCheckbox)), "Floating Toolbar Island (Dock)");
        public static LocalisableString ToolbarCornerRadiusSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarCornerRadiusSlider)), "Toolbar Corner Radius");
        public static LocalisableString BackgroundOpacitySlider => OsuCcLocalisation.Get(getKey(nameof(BackgroundOpacitySlider)), "Toolbar Background Opacity");
        public static LocalisableString ToolbarHeightSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarHeightSlider)), "Toolbar Height (Compact Mode)");
        public static LocalisableString ToolbarWidthSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarWidthSlider)), "Toolbar Width (Length)");
        public static LocalisableString ToolbarOffsetXSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarOffsetXSlider)), "Toolbar Horizontal Offset");
        public static LocalisableString ToolbarOffsetYSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarOffsetYSlider)), "Toolbar Vertical Offset");
        public static LocalisableString ToolbarSpacingSlider => OsuCcLocalisation.Get(getKey(nameof(ToolbarSpacingSlider)), "Toolbar Item Spacing (Compactness)");

        public static LocalisableString TopScreenDarkGlowSlider => OsuCcLocalisation.Get(getKey(nameof(TopScreenDarkGlowSlider)), "Top Screen Dark Glow / Vignette");
        public static LocalisableString AdaptScreensToIslandCheckbox => OsuCcLocalisation.Get(getKey(nameof(AdaptScreensToIslandCheckbox)), "Adapt Screens to Island (Soft Rounded Corners)");
        public static LocalisableString SeamlessRulesetSelectorCheckbox => OsuCcLocalisation.Get(getKey(nameof(SeamlessRulesetSelectorCheckbox)), "Seamless Ruleset Selector (Remove Solid Background)");

        // Section 3: Spacers
        public static LocalisableString SpacerStyleDropdown => OsuCcLocalisation.Get(getKey(nameof(SpacerStyleDropdown)), "Spacer Style");
        public static LocalisableString SpacerBlank => OsuCcLocalisation.Get(getKey(nameof(SpacerBlank)), "Blank gap");
        public static LocalisableString SpacerLine => OsuCcLocalisation.Get(getKey(nameof(SpacerLine)), "Thin vertical line");
        public static LocalisableString SpacerDot => OsuCcLocalisation.Get(getKey(nameof(SpacerDot)), "Dot");

        // Section 4: User Profile
        public static LocalisableString ProfileModeDropdown => OsuCcLocalisation.Get(getKey(nameof(ProfileModeDropdown)), "Avatar & Username Layout");
        public static LocalisableString ProfileDefault => OsuCcLocalisation.Get(getKey(nameof(ProfileDefault)), "Default (Username | Avatar)");
        public static LocalisableString ProfileAvatarLeft => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarLeft)), "Avatar on left (Avatar | Username)");
        public static LocalisableString ProfileWithSeparator => OsuCcLocalisation.Get(getKey(nameof(ProfileWithSeparator)), "With separator (Username │ Avatar)");
        public static LocalisableString ProfileAvatarLeftWithSep => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarLeftWithSep)), "Avatar on left with separator (Avatar │ Username)");
        public static LocalisableString ProfileAvatarOnly => OsuCcLocalisation.Get(getKey(nameof(ProfileAvatarOnly)), "Avatar only");
        public static LocalisableString ProfileUsernameOnly => OsuCcLocalisation.Get(getKey(nameof(ProfileUsernameOnly)), "Username only");

        public static LocalisableString ProfileStatsPositionDropdown => OsuCcLocalisation.Get(getKey(nameof(ProfileStatsPositionDropdown)), "Rank & PP Stats Position");
        public static LocalisableString ProfileStatsRight => OsuCcLocalisation.Get(getKey(nameof(ProfileStatsRight)), "On right (Default)");
        public static LocalisableString ProfileStatsLeft => OsuCcLocalisation.Get(getKey(nameof(ProfileStatsLeft)), "On left (Slide in from left)");

        // Edit Mode Banner
        public static LocalisableString EditBannerHint => OsuCcLocalisation.Get(getKey(nameof(EditBannerHint)), "Edit Mode | Drag blocks to reorder | RMB: menu | Esc / Click: Save");
        public static LocalisableString EditBannerSaveButton => OsuCcLocalisation.Get(getKey(nameof(EditBannerSaveButton)), "Save & Exit");

        // Context Menu Items
        public static LocalisableString ContextMenuHide => OsuCcLocalisation.Get(getKey(nameof(ContextMenuHide)), "Hide this element");
        public static LocalisableString ContextMenuShow => OsuCcLocalisation.Get(getKey(nameof(ContextMenuShow)), "Show this element");
        public static LocalisableString ContextMenuMoveLeft => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveLeft)), "Move to: Left");
        public static LocalisableString ContextMenuMoveCenter => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveCenter)), "Move to: Center");
        public static LocalisableString ContextMenuMoveRight => OsuCcLocalisation.Get(getKey(nameof(ContextMenuMoveRight)), "Move to: Right");
        public static LocalisableString ContextMenuResetBlock => OsuCcLocalisation.Get(getKey(nameof(ContextMenuResetBlock)), "Reset to default position");
        public static LocalisableString ContextMenuAddSpacer => OsuCcLocalisation.Get(getKey(nameof(ContextMenuAddSpacer)), "Add spacer (gap)");
        public static LocalisableString ContextMenuRemoveSpacer => OsuCcLocalisation.Get(getKey(nameof(ContextMenuRemoveSpacer)), "Delete spacer");

        // Block Friendly Names
        public static LocalisableString BlockSettings => OsuCcLocalisation.Get(getKey(nameof(BlockSettings)), "Settings");
        public static LocalisableString BlockHome => OsuCcLocalisation.Get(getKey(nameof(BlockHome)), "Home");
        public static LocalisableString BlockRulesets => OsuCcLocalisation.Get(getKey(nameof(BlockRulesets)), "Game Modes");
        public static LocalisableString BlockClock => OsuCcLocalisation.Get(getKey(nameof(BlockClock)), "Clock");
        public static LocalisableString BlockNotifications => OsuCcLocalisation.Get(getKey(nameof(BlockNotifications)), "Notifications");
        public static LocalisableString BlockRankings => OsuCcLocalisation.Get(getKey(nameof(BlockRankings)), "Rankings");
        public static LocalisableString BlockNews => OsuCcLocalisation.Get(getKey(nameof(BlockNews)), "News");
        public static LocalisableString BlockChangelog => OsuCcLocalisation.Get(getKey(nameof(BlockChangelog)), "Changelog");
        public static LocalisableString BlockWiki => OsuCcLocalisation.Get(getKey(nameof(BlockWiki)), "Wiki");
        public static LocalisableString BlockBeatmaps => OsuCcLocalisation.Get(getKey(nameof(BlockBeatmaps)), "Beatmap Listing");
        public static LocalisableString BlockChat => OsuCcLocalisation.Get(getKey(nameof(BlockChat)), "Chat");
        public static LocalisableString BlockSocial => OsuCcLocalisation.Get(getKey(nameof(BlockSocial)), "Social");
        public static LocalisableString BlockMusic => OsuCcLocalisation.Get(getKey(nameof(BlockMusic)), "Music");
        public static LocalisableString BlockUserProfile => OsuCcLocalisation.Get(getKey(nameof(BlockUserProfile)), "User Profile");
        public static LocalisableString BlockSpacer => OsuCcLocalisation.Get(getKey(nameof(BlockSpacer)), "Spacer");

        // Save Preset Dialog
        public static LocalisableString DialogSavePresetTitle => OsuCcLocalisation.Get(getKey(nameof(DialogSavePresetTitle)), "Save Toolbar Layout Preset");
        public static LocalisableString DialogSavePresetPrompt => OsuCcLocalisation.Get(getKey(nameof(DialogSavePresetPrompt)), "Enter a name for the new preset:");
        public static LocalisableString DialogSaveButton => OsuCcLocalisation.Get(getKey(nameof(DialogSaveButton)), "Save");
        public static LocalisableString DialogCancelButton => OsuCcLocalisation.Get(getKey(nameof(DialogCancelButton)), "Cancel");

        // Notifications
        public static LocalisableString NotifyClipboardCopied => OsuCcLocalisation.Get(getKey(nameof(NotifyClipboardCopied)), "Layout code copied to clipboard!");
        public static LocalisableString NotifyClipboardEmpty => OsuCcLocalisation.Get(getKey(nameof(NotifyClipboardEmpty)), "Clipboard is empty!");
        public static LocalisableString NotifyImportSuccess => OsuCcLocalisation.Get(getKey(nameof(NotifyImportSuccess)), "Toolbar layout successfully imported!");
        public static LocalisableString NotifyImportInvalid => OsuCcLocalisation.Get(getKey(nameof(NotifyImportInvalid)), "No valid layout code found in clipboard (ET_LAYOUT_v1:...)!");
        public static LocalisableString NotifyLayoutSaved => OsuCcLocalisation.Get(getKey(nameof(NotifyLayoutSaved)), "Toolbar layout saved");
        public static LocalisableString NotifyBlockReset(string name) => OsuCcLocalisation.Get(getKey(nameof(NotifyBlockReset)), "Block '{0}' restored to default position", name);
        public static LocalisableString NotifyDefaultPresetProtected => OsuCcLocalisation.Get(getKey(nameof(NotifyDefaultPresetProtected)), "Default preset cannot be overwritten!");
    }
}
