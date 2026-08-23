using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game;
using osu.Game.Screens;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osucc.Plugin;
using ExtendedToolbar.UI;

namespace ExtendedToolbar.Tweaks
{
    public partial class ToolbarVisibilityManager : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private IReadOnlyDictionary<string, ToolbarBlockContainer>? allBlocks;
        private ScreenStack? screenStack;

        public ToolbarVisibilityManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;
        }

        public void Attach(IReadOnlyDictionary<string, ToolbarBlockContainer> blocks)
        {
            allBlocks = blocks;

            if (host.Game is OsuGame game)
            {
                screenStack = game.ScreenStack;
                if (screenStack != null)
                {
                    screenStack.ScreenPushed += onScreenChanged;
                    screenStack.ScreenExited += onScreenChanged;
                }
            }
        }

        private void onScreenChanged(IScreen prev, IScreen next)
        {
            updateScreenRulesetVisibility(next);
        }

        public void UpdateVisibilityForCurrentScreen()
        {
            if (screenStack?.CurrentScreen != null)
            {
                updateScreenRulesetVisibility(screenStack.CurrentScreen);
            }
        }

        private void updateScreenRulesetVisibility(IScreen? screen)
        {
            if (allBlocks != null && allBlocks.TryGetValue("rulesets", out var rulesetBlock))
            {
                bool shouldHide = isScreenDisallowingRulesetChanges(screen);
                rulesetBlock.IsHiddenByScreen = shouldHide;
            }
        }

        private static bool isScreenDisallowingRulesetChanges(IScreen? screen)
        {
            if (screen == null) return false;
            return screen is Player ||
                   screen is ResultsScreen ||
                   screen is Editor ||
                   screen.GetType().Name.Contains("Player", StringComparison.OrdinalIgnoreCase) ||
                   screen.GetType().Name.Contains("Results", StringComparison.OrdinalIgnoreCase) ||
                   screen.GetType().Name.Contains("Editor", StringComparison.OrdinalIgnoreCase);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (screenStack != null)
            {
                screenStack.ScreenPushed -= onScreenChanged;
                screenStack.ScreenExited -= onScreenChanged;
            }

            base.Dispose(isDisposing);
        }
    }
}
