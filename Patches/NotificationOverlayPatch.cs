using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osucc.Core;
using osucc.Plugin;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.Patches
{
    /// <summary>
    /// Патч на NotificationOverlay.PopIn для надежной анимации открытия шторки со стороны Left или Right.
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
            if (__instance == null || settings == null)
                return true;

            float width = __instance.DrawWidth > 0 ? __instance.DrawWidth : 400f;
            bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;

            if (isLeft)
            {
                __instance.Anchor = Anchor.TopLeft;
                __instance.Origin = Anchor.TopLeft;
                __instance.Y = 0f;
                __instance.ClearTransforms();
                __instance.TransformTo(nameof(Drawable.X), -width);
                __instance.TransformTo(nameof(Drawable.X), 0f, 400, Easing.OutQuint);
                __instance.FadeIn(200, Easing.OutQuint);
                return false;
            }
            else
            {
                __instance.Anchor = Anchor.TopRight;
                __instance.Origin = Anchor.TopRight;
                __instance.Y = 0f;
                __instance.ClearTransforms();
                __instance.TransformTo(nameof(Drawable.X), width);
                __instance.TransformTo(nameof(Drawable.X), 0f, 400, Easing.OutQuint);
                __instance.FadeIn(200, Easing.OutQuint);
                return false;
            }
        }
    }

    /// <summary>
    /// Патч на NotificationOverlay.PopOut для надежной анимации закрытия шторки в сторону Left или Right.
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
            if (__instance == null || settings == null)
                return true;

            float width = __instance.DrawWidth > 0 ? __instance.DrawWidth : 400f;
            bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;

            if (isLeft)
            {
                __instance.Anchor = Anchor.TopLeft;
                __instance.Origin = Anchor.TopLeft;
                __instance.Y = 0f;
                __instance.ClearTransforms();
                __instance.TransformTo(nameof(Drawable.X), 0f);
                __instance.TransformTo(nameof(Drawable.X), -width, 400, Easing.OutQuint);
                __instance.FadeOut(200, Easing.OutQuint);
                return false;
            }
            else
            {
                __instance.Anchor = Anchor.TopRight;
                __instance.Origin = Anchor.TopRight;
                __instance.Y = 0f;
                __instance.ClearTransforms();
                __instance.TransformTo(nameof(Drawable.X), 0f);
                __instance.TransformTo(nameof(Drawable.X), width, 400, Easing.OutQuint);
                __instance.FadeOut(200, Easing.OutQuint);
                return false;
            }
        }
    }
}
