using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Toolbar;
using osucc.Plugin;
using osuTK;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.Tweaks
{
    public partial class ToolbarStyleManager : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private Toolbar? targetToolbar;
        private Drawable? zonesContainer;
        private Box? backgroundBox;

        private Box? neonGlow;
        private Box? neonCore;

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

            // Setup Neon Glow lines if not created
            if (neonGlow == null)
            {
                neonGlow = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 4,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0,
                    Blending = BlendingParameters.Additive
                };
                targetToolbar.Add(neonGlow);
            }

            if (neonCore == null)
            {
                neonCore = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1.5f,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre,
                    Alpha = 0
                };
                targetToolbar.Add(neonCore);
            }

            // Bind value change handlers
            settings.FloatingIslandMode.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarCornerRadius.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarBackgroundOpacity.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarHeight.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarWidth.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarOffsetX.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarOffsetY.BindValueChanged(_ => ApplyAll(), true);
            settings.NeonGlowLine.BindValueChanged(_ => ApplyAll(), true);
            settings.NeonGlowOffset.BindValueChanged(_ => ApplyAll(), true);
            settings.ToolbarAccentColor.BindValueChanged(_ => ApplyAll(), true);

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
            bool glow = settings.NeonGlowLine.Value;
            float glowOffset = settings.NeonGlowOffset.Value;
            var accent = settings.ToolbarAccentColor.Value;

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

            if (neonGlow != null && neonCore != null)
            {
                Colour4 accentColour = getAccentColour(accent);

                neonGlow.Alpha = glow ? 0.6f : 0f;
                neonGlow.Colour = accentColour;
                neonGlow.Y = glowOffset;

                neonCore.Alpha = glow ? 0.9f : 0f;
                neonCore.Colour = accentColour.Lighten(0.4f);
                neonCore.Y = glowOffset;
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

        private static Colour4 getAccentColour(ToolbarAccentColor accent)
        {
            return accent switch
            {
                ToolbarAccentColor.Pink => Colour4.FromHex("#ff66aa"),
                ToolbarAccentColor.Purple => Colour4.FromHex("#bb66ff"),
                ToolbarAccentColor.Cyan => Colour4.FromHex("#00e5ff"),
                ToolbarAccentColor.Lime => Colour4.FromHex("#55ff77"),
                ToolbarAccentColor.Gold => Colour4.FromHex("#ffcc22"),
                ToolbarAccentColor.White => Colour4.White,
                _ => Colour4.FromHex("#ff66aa")
            };
        }
    }
}
