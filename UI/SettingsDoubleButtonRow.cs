using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;

namespace ExtendedToolbar.UI
{
    /// <summary>
    /// Компактная строка с двумя кнопками (48.5% / 48.5%) для настроек тулбара.
    /// </summary>
    public partial class SettingsDoubleButtonRow : CompositeDrawable
    {
        public SettingsDoubleButtonRow(
            LocalisableString textLeft, Action actionLeft,
            LocalisableString textRight, Action actionRight)
        {
            RelativeSizeAxes = Axes.X;
            Height = 36f;
            Padding = new MarginPadding { Horizontal = 20f };
            Margin = new MarginPadding { Vertical = 2f };

            InternalChildren = new Drawable[]
            {
                new RoundedButton
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.485f,
                    Height = 36f,
                    Text = textLeft,
                    Action = actionLeft
                },
                new RoundedButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.485f,
                    Height = 36f,
                    Text = textRight,
                    Action = actionRight
                }
            };
        }
    }
}
