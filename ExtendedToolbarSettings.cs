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
        public Bindable<bool> NeonGlowLine { get; set; } = new(false);
        public Bindable<float> NeonGlowOffset { get; set; } = new(0.0f);
        public Bindable<ToolbarAccentColor> ToolbarAccentColor { get; set; } = new(Models.ToolbarAccentColor.Pink);

        public Bindable<UserProfileDisplayMode> UserProfileDisplayMode { get; set; } = new(Models.UserProfileDisplayMode.Default);
        public Bindable<ProfileStatsPosition> ProfileStatsPosition { get; set; } = new(Models.ProfileStatsPosition.Right);
        public Bindable<SpacerStyle> SpacerStyle { get; set; } = new(Models.SpacerStyle.Blank);
        public Bindable<string> ActivePresetName { get; set; } = new("Default");
    }
}
