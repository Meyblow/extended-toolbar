using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;
using osuTK;
using ExtendedToolbar.Models;
using ExtendedToolbar.UI;

namespace ExtendedToolbar.Tweaks
{
    public partial class ToolbarStyleManager : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private Toolbar? targetToolbar;
        private Drawable? zonesContainer;
        private Box? backgroundBox;
        private Box? topDarkGlowBox;

        public static float CalculateTargetY(bool floatingIsland, float offsetY)
        {
            return floatingIsland ? (8.0f + offsetY) : offsetY;
        }

        public ToolbarStyleManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;
        }

        public void Attach(Toolbar toolbar, Drawable zones, Box? bg)
        {
            targetToolbar = toolbar;
            zonesContainer = zones;
            backgroundBox = bg;

            if (targetToolbar == null) return;

            // Setup Top Screen Dark Glow (Vignette) behind toolbar
            if (topDarkGlowBox == null)
            {
                topDarkGlowBox = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 110f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Alpha = 0f,
                    AlwaysPresent = true,
                    Depth = float.MaxValue - 100 // placed behind toolbar and UI
                };

                if (targetToolbar.Parent is Container<Drawable> parentContainer)
                {
                    if (topDarkGlowBox.Parent == null)
                        parentContainer.Add(topDarkGlowBox);
                }
                else if (host.Game is Container<Drawable> gameContainer)
                {
                    if (topDarkGlowBox.Parent == null)
                        gameContainer.Add(topDarkGlowBox);
                }
            }

            // Bind value change handlers
            settings.FloatingIslandMode.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarCornerRadius.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarBackgroundOpacity.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarHeight.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarWidth.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarOffsetX.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarOffsetY.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarSpacing.BindValueChanged(_ => ApplyAll(), true);
            settings.TopScreenDarkGlow.BindValueChanged(_ => ApplyAll(), true);
            settings.SeamlessRulesetSelector.BindValueChanged(_ => ApplyAll(), true);

            targetToolbar.State.BindValueChanged(state =>
            {
                if (state.NewValue == Visibility.Visible)
                {
                    Scheduler.AddOnce(ApplyAll);
                }
            });

            ApplyAll();
        }

        public void ApplyAll()
        {
            if (targetToolbar == null || IsDisposed) return;

            bool island = settings.FloatingIslandMode.Value;
            float radius = settings.ToolbarCornerRadius.Value;
            float opacity = settings.ToolbarBackgroundOpacity.Value;
            float height = settings.ToolbarHeight.Value;
            float widthPercent = settings.ToolbarWidth.Value;
            float offsetX = settings.ToolbarOffsetX.Value;
            float offsetY = settings.ToolbarOffsetY.Value;

            float targetY = CalculateTargetY(island, offsetY);

            if (island)
            {
                targetToolbar.Anchor = Anchor.TopCentre;
                targetToolbar.Origin = Anchor.TopCentre;
                targetToolbar.RelativeSizeAxes = Axes.None;
                targetToolbar.Masking = true;
                targetToolbar.CornerRadius = radius;

                float parentWidth = targetToolbar.Parent?.DrawWidth ?? 1366f;
                if (parentWidth <= 100f) parentWidth = 1366f;

                float targetWidth = Math.Clamp(parentWidth * widthPercent, 300f, parentWidth);
                targetToolbar.Width = targetWidth;

                float maxOffset = Math.Max(0f, (parentWidth - targetWidth) / 2f);
                targetToolbar.X = Math.Clamp(offsetX, -maxOffset, maxOffset);

                targetToolbar.Height = height;
                targetToolbar.Y = targetY;

                targetToolbar.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Colour4.Black.Opacity(0.55f),
                    Radius = 14,
                    Offset = new Vector2(0, 4)
                };
            }
            else
            {
                targetToolbar.Anchor = Anchor.TopLeft;
                targetToolbar.Origin = Anchor.TopLeft;
                targetToolbar.RelativeSizeAxes = Axes.X;
                targetToolbar.Masking = false;
                targetToolbar.CornerRadius = 0;
                targetToolbar.Width = 1f;
                targetToolbar.X = offsetX;
                targetToolbar.Height = height;
                targetToolbar.Y = targetY;
                targetToolbar.EdgeEffect = default;
            }

            if (backgroundBox != null)
            {
                backgroundBox.Alpha = opacity;
            }

            // Top Screen Dark Glow
            if (topDarkGlowBox != null)
            {
                float glowVal = Math.Clamp(settings.TopScreenDarkGlow.Value, 0f, 1f);
                if (glowVal > 0f)
                {
                    topDarkGlowBox.Alpha = 1f;
                    topDarkGlowBox.Colour = ColourInfo.GradientVertical(Colour4.Black.Opacity(glowVal), Colour4.Black.Opacity(0f));
                }
                else
                {
                    topDarkGlowBox.Alpha = 0f;
                }
            }

            // Apply Seamless styling to ToolbarRulesetSelector
            applyRulesetSelectorStyle();
        }

        private void applyRulesetSelectorStyle()
        {
            if (IsDisposed) return;

            try
            {
                var blockDrawables = zonesContainer?.ChildrenOfType<ToolbarBlockContainer>()
                    .Where(b => b.ItemId == "rulesets" || (b.ContentDrawable != null && b.ContentDrawable.GetType().Name.Contains("Ruleset", StringComparison.OrdinalIgnoreCase)))
                    .Select(b => b.ContentDrawable) ?? Enumerable.Empty<Drawable>();

                var directDrawables = targetToolbar?.ChildrenOfType<Drawable>()
                    .Where(d => d.GetType().Name.Contains("Ruleset", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<Drawable>();

                var rulesetDrawables = blockDrawables.Concat(directDrawables).Where(d => d != null).Distinct().ToList();

                foreach (var content in rulesetDrawables)
                {
                    if (content == null) continue;

                    if (content is Container cont)
                    {
                        cont.Masking = true;
                        cont.CornerRadius = settings.FloatingIslandMode.Value ? 8f : 0f;
                    }

                    var bgBoxes = content.ChildrenOfType<Box>().Where(b => b.RelativeSizeAxes == Axes.Both || b.RelativeSizeAxes == Axes.X).ToList();
                    foreach (var b in bgBoxes)
                    {
                        if (settings.SeamlessRulesetSelector.Value)
                        {
                            b.Alpha = 0f;
                        }
                        else
                        {
                            b.Alpha = settings.ToolbarBackgroundOpacity.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("applyRulesetSelectorStyle error", ex);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (IsDisposed || targetToolbar == null) return;

            // Recalculate dynamic width and clamp if parent size changes
            if (settings.FloatingIslandMode.Value && targetToolbar.Parent != null)
            {
                float parentWidth = targetToolbar.Parent.DrawWidth;
                if (parentWidth > 100f)
                {
                    float targetWidth = Math.Clamp(parentWidth * settings.ToolbarWidth.Value, 300f, parentWidth);
                    if (Math.Abs(targetToolbar.Width - targetWidth) > 0.5f)
                    {
                        targetToolbar.Width = targetWidth;
                    }

                    float maxOffset = Math.Max(0f, (parentWidth - targetWidth) / 2f);
                    float clampedX = Math.Clamp(settings.ToolbarOffsetX.Value, -maxOffset, maxOffset);
                    if (Math.Abs(targetToolbar.X - clampedX) > 0.5f)
                    {
                        targetToolbar.X = clampedX;
                    }
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (topDarkGlowBox?.Parent is Container<Drawable> parentContainer)
            {
                parentContainer.Remove(topDarkGlowBox, true);
            }
            topDarkGlowBox = null;

            base.Dispose(isDisposing);
        }
    }
}
