using osu.Framework.Localisation;
using ExtendedToolbar.Localisation;

namespace ExtendedToolbar.Models
{
    /// <summary>
    /// Режим визуального отображения кнопки профиля пользователя в тулбаре.
    /// </summary>
    public enum UserProfileDisplayMode
    {
        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileDefault))]
        Default,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileAvatarLeft))]
        AvatarLeft,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileWithSeparator))]
        WithSeparator,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileAvatarLeftWithSep))]
        AvatarLeftWithSep,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileAvatarOnly))]
        AvatarOnly,

        [LocalisableDescription(typeof(ExtendedToolbarStrings), nameof(ExtendedToolbarStrings.ProfileUsernameOnly))]
        UsernameOnly
    }
}
