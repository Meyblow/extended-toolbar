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
            // Отключено: сохраняем ванильное поведение шторки уведомлений
        }
    }

    /// <summary>
    /// Патч на NotificationOverlay.PopOut для плавной анимации закрытия шторки mainContent в сторону Left или Right.
    /// </summary>
    public sealed class NotificationOverlayPopOutPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        public NotificationOverlayPopOutPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, ExtendedToolbarSettings settings)
            : base(plugin, host, "osu.Game.Overlays.NotificationOverlay", "PopOut", MethodType.Postfix)
        {
        }

        public static void Postfix(NotificationOverlay __instance)
        {
            // Отключено: сохраняем ванильное поведение шторки уведомлений
        }
    }
}
