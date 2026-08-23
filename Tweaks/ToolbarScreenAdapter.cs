using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Screens;
using osu.Game.Screens.Select;
using osucc.Plugin;

namespace ExtendedToolbar.Tweaks
{
    /// <summary>
    /// Адаптирует экранные панели (например, в SongSelect: BeatmapInfoWedge и FilterControl)
    /// под стиль плавающего острова, закругляя их верхние углы и задавая аккуратные отступы.
    /// Полностью обратим при отключении настройки или возврате в стандартный режим тулбара.
    /// </summary>
    public partial class ToolbarScreenAdapter : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private ScreenStack? screenStack;
        private IScreen? currentObservedScreen;

        public ToolbarScreenAdapter(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;
        }

        public void Attach(ScreenStack? stack)
        {
            screenStack = stack;

            if (screenStack != null)
            {
                screenStack.ScreenPushed += onScreenChanged;
                screenStack.ScreenExited += onScreenChanged;
            }

            settings.AdaptScreensToIsland.BindValueChanged(_ => UpdateCurrentScreen(), true);
            settings.FloatingIslandMode.BindValueChanged(_ => UpdateCurrentScreen(), true);
            settings.ToolbarCornerRadius.BindValueChanged(_ => UpdateCurrentScreen(), true);

            UpdateCurrentScreen();
        }

        private void onScreenChanged(IScreen prev, IScreen next)
        {
            if (IsDisposed) return;

            currentObservedScreen = next;
            host.Scheduler?.AddOnce(UpdateCurrentScreen);
        }

        public void UpdateCurrentScreen()
        {
            if (IsDisposed) return;

            var screen = screenStack?.CurrentScreen ?? currentObservedScreen;
            if (screen is not Drawable screenDrawable) return;

            try
            {
                bool shouldAdapt = settings.AdaptScreensToIsland.Value && settings.FloatingIslandMode.Value;
                float radius = Math.Clamp(settings.ToolbarCornerRadius.Value, 8f, 20f);

                adaptSongSelectPanels(screenDrawable, shouldAdapt, radius);
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("ToolbarScreenAdapter.UpdateCurrentScreen failed", ex);
            }
        }

        private void adaptSongSelectPanels(Drawable screenRoot, bool shouldAdapt, float cornerRadius)
        {
            if (IsDisposed || screenRoot == null) return;

            // 1. Поиск левой панели с информацией о карте (BeatmapInfoWedge / WedgeContainer)
            var wedgeContainers = screenRoot.ChildrenOfType<Container>()
                .Where(c => c.GetType().Name.Contains("Wedge", StringComparison.OrdinalIgnoreCase) ||
                            c.GetType().Name.Contains("BeatmapDetail", StringComparison.OrdinalIgnoreCase) ||
                            c.GetType().Name.Contains("BeatmapInfo", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var wedge in wedgeContainers)
            {
                if (!wedge.IsAlive || wedge.Parent == null) continue;

                if (shouldAdapt)
                {
                    wedge.Masking = true;
                    wedge.CornerRadius = cornerRadius;
                    wedge.Margin = new MarginPadding { Top = 6f, Left = 8f, Bottom = 4f };
                }
                else
                {
                    wedge.CornerRadius = 0f;
                    wedge.Masking = false;
                    wedge.Margin = new MarginPadding(0);
                }
            }

            // 2. Поиск правой панели поиска и фильтров (FilterControl)
            var filterControls = screenRoot.ChildrenOfType<Container>()
                .Where(c => c.GetType().Name.Contains("FilterControl", StringComparison.OrdinalIgnoreCase) ||
                            c.GetType().Name.Contains("SearchControl", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var filter in filterControls)
            {
                if (!filter.IsAlive || filter.Parent == null) continue;

                if (shouldAdapt)
                {
                    filter.Masking = true;
                    filter.CornerRadius = cornerRadius;
                    filter.Margin = new MarginPadding { Top = 6f, Right = 8f, Bottom = 4f };
                }
                else
                {
                    filter.CornerRadius = 0f;
                    filter.Masking = false;
                    filter.Margin = new MarginPadding(0);
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (screenStack != null)
            {
                screenStack.ScreenPushed -= onScreenChanged;
                screenStack.ScreenExited -= onScreenChanged;
            }

            // Revert changes on current screen upon disposal
            if (screenStack?.CurrentScreen is Drawable screenDrawable)
            {
                try
                {
                    adaptSongSelectPanels(screenDrawable, false, 0f);
                }
                catch { }
            }

            base.Dispose(isDisposing);
        }
    }
}
