using osu.Framework.Localisation;
using ExtendedToolbar.Localisation;

namespace ExtendedToolbar.Models
{
    /// <summary>
    /// Сторона выезда боковой шторки списка уведомлений (NotificationOverlay).
    /// </summary>
    public enum NotificationSidebarPosition
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.SidebarPosRight))]
        Right,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.SidebarPosLeft))]
        Left
    }
}
