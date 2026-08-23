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
}
