using System;
using System.Linq;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;
using ExtendedToolbar.Patches;
using ExtendedToolbar.Tweaks;
using ExtendedToolbar.UI;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar
{
    public class ExtendedToolbarPlugin : OsuCcPlugin
    {
        public static ExtendedToolbarPlugin? Instance { get; private set; }

        public ExtendedToolbarSettings Settings { get; } = new();

        private ToolbarLayoutManager? layoutManager;
        private ToolbarStyleManager? styleManager;
        private ToolbarProfileLayoutManager? profileManager;
        private ToolbarVisibilityManager? visibilityManager;

        protected override void OnLoad()
        {
            Instance = this;
            ExtendedToolbarLog.Init(Host);
            ExtendedToolbarLog.Info("Extended Toolbar: OnLoad() starting...");

            try
            {
                osucc.Localisation.OsuCcLocalisation.RegisterAssembly(typeof(ExtendedToolbarPlugin).Assembly);
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Failed to register localization assembly", ex);
            }

            Settings.FloatingIslandMode = Host.GetSettings().Bind("floating_island_mode", false);
            Settings.ToolbarCornerRadius = Host.GetSettings().Bind("toolbar_corner_radius", 12.0f);
            Settings.ToolbarBackgroundOpacity = Host.GetSettings().Bind("toolbar_bg_opacity", 1.0f);
            Settings.ToolbarHeight = Host.GetSettings().Bind("toolbar_height", 40.0f);
            Settings.ToolbarWidth = Host.GetSettings().Bind("toolbar_width", 0.985f);
            Settings.ToolbarOffsetX = Host.GetSettings().Bind("toolbar_offset_x", 0.0f);
            Settings.ToolbarOffsetY = Host.GetSettings().Bind("toolbar_offset_y", 0.0f);
            Settings.ToolbarSpacing = Host.GetSettings().Bind("toolbar_spacing", 4.0f);
            Settings.NeonGlowLine = Host.GetSettings().Bind("neon_glow_line", false);
            Settings.NeonGlowOffset = Host.GetSettings().Bind("neon_glow_offset", 0.0f);
            Settings.ToolbarAccentColor = Host.GetSettings().Bind("toolbar_accent_color", Models.ToolbarAccentColor.Pink);

            Settings.UserProfileDisplayMode = Host.GetSettings().Bind("user_profile_display_mode", Models.UserProfileDisplayMode.Default);
            Settings.ProfileStatsPosition = Host.GetSettings().Bind("profile_stats_position", Models.ProfileStatsPosition.Right);
            Settings.SpacerStyle = Host.GetSettings().Bind("spacer_style", Models.SpacerStyle.Blank);
            Settings.ActivePresetName = Host.GetSettings().Bind("active_preset_name", "Default");

            layoutManager = new ToolbarLayoutManager(Host, Settings);
            styleManager = new ToolbarStyleManager(Host, Settings);
            profileManager = new ToolbarProfileLayoutManager(Host, Settings);
            visibilityManager = new ToolbarVisibilityManager(Host, Settings);

            Host.AddPatch(new ToolbarPatch(this, Host));
            Host.AddPatch(new ToolbarPopInPatch(this, Host, Settings.FloatingIslandMode, Settings.ToolbarOffsetY));

            ExtendedToolbarLog.Info("Extended Toolbar: OnLoad() complete.");
        }

        public override void AttachToGame()
        {
            ExtendedToolbarLog.Info("Extended Toolbar: AttachToGame() called.");

            try
            {
                if (Host.Data != null)
                {
                    ToolbarPresetManager.Init(Host.Data);
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Error initializing ToolbarPresetManager", ex);
            }

            Host.AddSettingsSubsection(() => new ExtendedToolbarSettingsSubsection(Settings));

            if (Host.Game is OsuGame game)
            {
                Host.Scheduler?.Add(() =>
                {
                    try
                    {
                        var toolbar = game.ChildrenOfType<Toolbar>().FirstOrDefault();
                        if (toolbar != null)
                        {
                            ExtendedToolbarLog.Info($"AttachToGame: Found existing Toolbar ({toolbar.GetHashCode()}), attaching manager.");
                            OnToolbarLoaded(toolbar);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtendedToolbarLog.Error("Error checking Toolbar in AttachToGame scheduler", ex);
                    }
                });
            }
        }

        internal void OnToolbarLoaded(Toolbar toolbar)
        {
            if (layoutManager != null && styleManager != null && profileManager != null && visibilityManager != null)
            {
                layoutManager.AttachToolbar(toolbar, styleManager, profileManager, visibilityManager);
            }
        }

        public override void Dispose()
        {
            ExtendedToolbarLog.Info("Extended Toolbar: Disposing plugin...");
            layoutManager?.Dispose();
            layoutManager = null;
            styleManager?.Dispose();
            styleManager = null;
            profileManager?.Dispose();
            profileManager = null;
            visibilityManager?.Dispose();
            visibilityManager = null;

            base.Dispose();
            GC.SuppressFinalize(this);
            ExtendedToolbarLog.Info("Extended Toolbar: Plugin disposed.");
        }
    }
}
