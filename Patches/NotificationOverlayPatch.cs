using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;
using osucc.Core;
using osucc.Plugin;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.Patches
{
    /// <summary>
    /// Патч на NotificationOverlay.PopIn для плавной анимации открытия шторки mainContent со стороны Left или Right.
    /// </summary>
    public sealed class NotificationOverlayPopInPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        private static ExtendedToolbarSettings? settings;
        private static readonly FieldInfo? mainContentField = typeof(NotificationOverlay).GetField("mainContent", BindingFlags.NonPublic | BindingFlags.Instance);

        public NotificationOverlayPopInPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, ExtendedToolbarSettings settings)
            : base(plugin, host, "osu.Game.Overlays.NotificationOverlay", "PopIn", MethodType.Postfix)
        {
            NotificationOverlayPopInPatch.settings = settings;
        }

        public static void Postfix(NotificationOverlay __instance)
        {
            if (__instance == null || settings == null)
                return;

            var mainContent = mainContentField?.GetValue(__instance) as Container;
            if (mainContent == null)
                return;

            bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;

            if (isLeft)
            {
                mainContent.Anchor = Anchor.TopLeft;
                mainContent.Origin = Anchor.TopLeft;
                mainContent.ClearTransforms(targetMember: nameof(Drawable.X));
                mainContent.MoveToX(0, 500, Easing.OutQuint);
            }
            else
            {
                mainContent.Anchor = Anchor.TopRight;
                mainContent.Origin = Anchor.TopRight;
                mainContent.ClearTransforms(targetMember: nameof(Drawable.X));
                mainContent.MoveToX(0, 500, Easing.OutQuint);
            }
        }
    }

    /// <summary>
    /// Патч на NotificationOverlay.PopOut для плавной анимации закрытия шторки mainContent в сторону Left или Right.
    /// </summary>
    public sealed class NotificationOverlayPopOutPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        private static ExtendedToolbarSettings? settings;
        private static readonly FieldInfo? mainContentField = typeof(NotificationOverlay).GetField("mainContent", BindingFlags.NonPublic | BindingFlags.Instance);

        public NotificationOverlayPopOutPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, ExtendedToolbarSettings settings)
            : base(plugin, host, "osu.Game.Overlays.NotificationOverlay", "PopOut", MethodType.Postfix)
        {
            NotificationOverlayPopOutPatch.settings = settings;
        }

        public static void Postfix(NotificationOverlay __instance)
        {
            if (__instance == null || settings == null)
                return;

            var mainContent = mainContentField?.GetValue(__instance) as Container;
            if (mainContent == null)
                return;

            bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;
            float width = mainContent.DrawWidth > 0 ? mainContent.DrawWidth : 400f;

            if (isLeft)
            {
                mainContent.Anchor = Anchor.TopLeft;
                mainContent.Origin = Anchor.TopLeft;
                mainContent.ClearTransforms(targetMember: nameof(Drawable.X));
                mainContent.MoveToX(-width, 500, Easing.OutQuint);
            }
            else
            {
                mainContent.Anchor = Anchor.TopRight;
                mainContent.Origin = Anchor.TopRight;
                mainContent.ClearTransforms(targetMember: nameof(Drawable.X));
                mainContent.MoveToX(width, 500, Easing.OutQuint);
            }
        }
    }
}
