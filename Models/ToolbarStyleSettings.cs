using osu.Framework.Localisation;
using ExtendedToolbar.Localisation;

namespace ExtendedToolbar.Models
{
    /// <summary>
    /// Стили отображения визуальных разделителей (спейсеров).
    /// </summary>
    public enum SpacerStyle
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.SpacerBlank))]
        Blank,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.SpacerLine))]
        Line,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.SpacerDot))]
        Dot
    }

    /// <summary>
    /// Акцентные цвета неоновой подсветки тулбара.
    /// </summary>
    public enum ToolbarAccentColor
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentPink))]
        Pink,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentPurple))]
        Purple,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentCyan))]
        Cyan,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentLime))]
        Lime,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentGold))]
        Gold,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.AccentWhite))]
        White
    }
}
