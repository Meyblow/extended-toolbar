using osu.Framework.Bindables;
using ExtendedToolbar.Models;

namespace ExtendedToolbar
{
    public class ExtendedToolbarSettings
    {
        public int SchemaVersion { get; set; } = 1;

        public Bindable<bool> FloatingIslandMode { get; set; } = new(false);
        public Bindable<float> ToolbarCornerRadius { get; set; } = new(12.0f);
        public Bindable<float> ToolbarBackgroundOpacity { get; set; } = new(1.0f);
        public Bindable<float> ToolbarHeight { get; set; } = new(40.0f);
        public Bindable<float> ToolbarWidth { get; set; } = new(0.985f);
        public Bindable<float> ToolbarOffsetX { get; set; } = new(0.0f);
        public Bindable<float> ToolbarOffsetY { get; set; } = new(0.0f);
        public Bindable<float> ToolbarSpacing { get; set; } = new(4.0f);

        public Bindable<UserProfileDisplayMode> UserProfileDisplayMode { get; set; } = new(Models.UserProfileDisplayMode.Default);
        public Bindable<ProfileStatsPosition> ProfileStatsPosition { get; set; } = new(Models.ProfileStatsPosition.Right);
        public Bindable<SpacerStyle> SpacerStyle { get; set; } = new(Models.SpacerStyle.Blank);
        public Bindable<string> ActivePresetName { get; set; } = new("Default");

        public Bindable<bool> AdaptScreensToIsland { get; set; } = new(true);
        public Bindable<float> TopScreenDarkGlow { get; set; } = new(0.0f);
        public Bindable<bool> SeamlessRulesetSelector { get; set; } = new(true);

        public Bindable<ToastPosition> ToastPosition { get; set; } = new(Models.ToastPosition.TopRight);
        public Bindable<int> MaxVisibleToasts { get; set; } = new(3);
        public Bindable<NotificationSidebarPosition> NotificationSidebarPosition { get; set; } = new(Models.NotificationSidebarPosition.Right);
    }
}
