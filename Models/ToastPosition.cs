using osu.Framework.Localisation;
using ExtendedToolbar.Localisation;

namespace ExtendedToolbar.Models
{
    /// <summary>
    /// Позиция всплывающих тост-уведомлений на экране.
    /// </summary>
    public enum ToastPosition
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ToastPosTopRight))]
        TopRight,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ToastPosTopCentre))]
        TopCentre,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ToastPosTopLeft))]
        TopLeft,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ToastPosBottomRight))]
        BottomRight,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ToastPosBottomLeft))]
        BottomLeft
    }
}
