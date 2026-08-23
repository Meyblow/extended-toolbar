using osu.Framework.Localisation;
using ExtendedToolbar.Localisation;

namespace ExtendedToolbar.Models
{
    /// <summary>
    /// Расположение блока статистики (ранг # и PP) в профиле: справа (по умолчанию) или слева.
    /// </summary>
    public enum ProfileStatsPosition
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileStatsRight))]
        Right,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileStatsLeft))]
        Left
    }
}
