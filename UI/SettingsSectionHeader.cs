using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace ExtendedToolbar.UI
{
    /// <summary>
    /// Аккуратный визуальный заголовок секции настроек с разделительной линией.
    /// </summary>
    public partial class SettingsSectionHeader : CompositeDrawable
    {
        private readonly LocalisableString title;

        public SettingsSectionHeader(LocalisableString title)
        {
            this.title = title;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Horizontal = 20, Top = 16, Bottom = 6 };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 4),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = title,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                        Colour = colours.Pink
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 1.5f,
                        Colour = Colour4.White.Opacity(0.12f)
                    }
                }
            };
        }
    }
}
