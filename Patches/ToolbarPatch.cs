using osu.Game.Overlays.Toolbar;
using osucc.Plugin;

namespace ExtendedToolbar.Patches
{
    public sealed class ToolbarPatch : PluginPatch<ExtendedToolbarPlugin>
    {
        public ToolbarPatch(ExtendedToolbarPlugin plugin, IOsuCcPluginHost host)
            : base(plugin, host, "osu.Game.Overlays.Toolbar.Toolbar", "load")
        {
        }

        public static void Postfix(Toolbar __instance)
        {
            ExtendedToolbarLog.Info($"Toolbar.load Postfix triggered! Toolbar instance: {__instance?.GetHashCode()}");
            if (__instance != null)
            {
                ExtendedToolbarPlugin.Instance?.OnToolbarLoaded(__instance);
            }
        }
    }
}
