using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Toolbar;
using osucc.Core;
using osucc.Plugin;
using ExtendedToolbar.Tweaks;

namespace ExtendedToolbar.Patches
{
    /// <summary>
    /// Патч на Toolbar.PopIn для сохранения Y-координаты тулбара при появлении.
    /// </summary>
    public sealed class ToolbarPopInPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        private static Bindable<bool>? floatingIslandBindable;
        private static Bindable<float>? offsetYBindable;

        public ToolbarPopInPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host, Bindable<bool> floatingIsland, Bindable<float> offsetY)
            : base(plugin, host, "osu.Game.Overlays.Toolbar.Toolbar", "PopIn", MethodType.Postfix)
        {
            floatingIslandBindable = floatingIsland;
            offsetYBindable = offsetY;
        }

        public static void Postfix(Toolbar __instance)
        {
            if (__instance == null || floatingIslandBindable?.Value != true)
                return;

            float targetY = ToolbarStyleManager.CalculateTargetY(true, offsetYBindable?.Value ?? 0f);
            __instance.ClearTransforms(targetMember: nameof(Drawable.Y));
            __instance.MoveToY(targetY, 500, Easing.OutQuint);
        }
    }
}
