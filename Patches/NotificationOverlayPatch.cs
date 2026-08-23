using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osucc.Core;
using osucc.Plugin;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.Patches
{
    /// <summary>
    /// Патч на NotificationOverlay.PopIn для анимации появления шторки слева.
    /// </summary>
    public sealed class NotificationOverlayPopInPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        private static ExtendedToolbarSettings? settings;

        public NotificationOverlayPopInPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, ExtendedToolbarSettings settings)
            : base(plugin, host, "osu.Game.Overlays.NotificationOverlay", "PopIn", MethodType.Prefix)
        {
            NotificationOverlayPopInPatch.settings = settings;
        }

        public static bool Prefix(OverlayContainer __instance)
        {
            if (__instance == null || settings?.NotificationSidebarPosition.Value != NotificationSidebarPosition.Left)
                return true;

            __instance.Anchor = Anchor.TopLeft;
            __instance.Origin = Anchor.TopLeft;

            float width = __instance.DrawWidth > 0 ? __instance.DrawWidth : 400f;
            __instance.ClearTransforms();
            __instance.TransformTo(nameof(__instance.X), -width);
            __instance.TransformTo(nameof(__instance.X), 0f, 400, Easing.OutQuint);
            __instance.FadeIn(200, Easing.OutQuint);

            return false;
        }
    }

    /// <summary>
    /// Патч на NotificationOverlay.PopOut для анимации скрытия шторки влево.
    /// </summary>
    public sealed class NotificationOverlayPopOutPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        private static ExtendedToolbarSettings? settings;

        public NotificationOverlayPopOutPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, ExtendedToolbarSettings settings)
            : base(plugin, host, "osu.Game.Overlays.NotificationOverlay", "PopOut", MethodType.Prefix)
        {
            NotificationOverlayPopOutPatch.settings = settings;
        }

        public static bool Prefix(OverlayContainer __instance)
        {
            if (__instance == null || settings?.NotificationSidebarPosition.Value != NotificationSidebarPosition.Left)
                return true;

            __instance.Anchor = Anchor.TopLeft;
            __instance.Origin = Anchor.TopLeft;

            float width = __instance.DrawWidth > 0 ? __instance.DrawWidth : 400f;
            __instance.ClearTransforms();
            __instance.TransformTo(nameof(__instance.X), 0f);
            __instance.TransformTo(nameof(__instance.X), -width, 400, Easing.OutQuint);
            __instance.FadeOut(200, Easing.OutQuint);

            return false;
        }
    }
}
