using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osucc.Plugin;
using ExtendedToolbar.Models;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar.Tweaks
{
    public partial class ToolbarProfileLayoutManager : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private Drawable? targetUserButton;

        public ToolbarProfileLayoutManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;

            settings.UserProfileDisplayMode.BindValueChanged(_ => Apply(), true);
            settings.ProfileStatsPosition.BindValueChanged(_ => Apply(), true);
        }

        public void Attach(Drawable? userButton)
        {
            targetUserButton = userButton;
            Apply();
        }

        public void Apply()
        {
            if (targetUserButton == null || IsDisposed) return;

            ApplyUserProfileDisplayMode(targetUserButton, settings.UserProfileDisplayMode.Value);
            ApplyProfileStatsPosition(targetUserButton, settings.ProfileStatsPosition.Value);
        }

        public static void ApplyUserProfileDisplayMode(Drawable? userButton, UserProfileDisplayMode mode)
        {
            if (userButton == null) return;

            try
            {
                var flow = userButton.ChildrenOfType<FillFlowContainer>().FirstOrDefault()
                           ?? userButton.GetType().GetField("Flow", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(userButton) as FillFlowContainer;

                if (flow == null) return;

                var avatar = flow.Children.FirstOrDefault(c => c is UpdateableAvatar || c.GetType().Name.Contains("Avatar", StringComparison.OrdinalIgnoreCase))
                             ?? userButton.GetType().GetField("avatar", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(userButton) as Drawable;

                var username = flow.Children.FirstOrDefault(c => c is OsuSpriteText || c is SpriteText || c.GetType().Name.Contains("Username", StringComparison.OrdinalIgnoreCase) || c.GetType().Name.Contains("Text", StringComparison.OrdinalIgnoreCase))
                               ?? userButton.GetType().GetField("usernameText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(userButton) as Drawable;

                if (avatar == null || username == null) return;

                var otherChildren = flow.Children.Where(c => c != avatar && c != username).ToList();
                flow.Clear(disposeChildren: false);

                switch (mode)
                {
                    case UserProfileDisplayMode.Default:
                    case UserProfileDisplayMode.WithSeparator:
                        avatar.Alpha = 1;
                        avatar.BypassAutoSizeAxes = Axes.None;
                        username.Alpha = 1;
                        username.BypassAutoSizeAxes = Axes.None;
                        flow.Add(username);
                        flow.Add(avatar);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.AvatarLeft:
                    case UserProfileDisplayMode.AvatarLeftWithSep:
                        avatar.Alpha = 1;
                        avatar.BypassAutoSizeAxes = Axes.None;
                        username.Alpha = 1;
                        username.BypassAutoSizeAxes = Axes.None;
                        flow.Add(avatar);
                        flow.Add(username);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.AvatarOnly:
                        avatar.Alpha = 1;
                        avatar.BypassAutoSizeAxes = Axes.None;
                        username.Alpha = 0;
                        username.BypassAutoSizeAxes = Axes.Both;
                        flow.Add(avatar);
                        flow.Add(username);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.UsernameOnly:
                        username.Alpha = 1;
                        username.BypassAutoSizeAxes = Axes.None;
                        avatar.Alpha = 0;
                        avatar.BypassAutoSizeAxes = Axes.Both;
                        flow.Add(username);
                        flow.Add(avatar);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("ApplyUserProfileDisplayMode failed", ex);
            }
        }

        public static void ApplyProfileStatsPosition(Drawable? userButton, ProfileStatsPosition position)
        {
            if (userButton == null) return;

            try
            {
                var statsDisplay = ReflectionHelper.FindStatsDisplay(userButton);
                if (statsDisplay == null) return;

                if (position == ProfileStatsPosition.Left)
                {
                    statsDisplay.Anchor = Anchor.CentreLeft;
                    statsDisplay.Origin = Anchor.CentreRight;
                    statsDisplay.X = -5f;

                    if (statsDisplay.Parent is Drawable parentContainer)
                    {
                        parentContainer.Anchor = Anchor.CentreLeft;
                        parentContainer.Origin = Anchor.CentreRight;
                    }
                }
                else
                {
                    statsDisplay.Anchor = Anchor.CentreRight;
                    statsDisplay.Origin = Anchor.CentreLeft;
                    statsDisplay.X = 5f;

                    if (statsDisplay.Parent is Drawable parentContainer)
                    {
                        parentContainer.Anchor = Anchor.CentreRight;
                        parentContainer.Origin = Anchor.CentreLeft;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("ApplyProfileStatsPosition failed", ex);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (IsDisposed || targetUserButton == null) return;

            // Периодически следим за сжатием и скрытием элементов при обновлении профиля игрой
            var mode = settings.UserProfileDisplayMode.Value;
            if (mode == UserProfileDisplayMode.UsernameOnly || mode == UserProfileDisplayMode.AvatarOnly)
            {
                ApplyUserProfileDisplayMode(targetUserButton, mode);
            }
        }
    }
}
