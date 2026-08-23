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
        }

        public void Attach(Drawable? userButton)
        {
            targetUserButton = userButton;
            if (targetUserButton == null) return;

            settings.UserProfileDisplayMode.BindValueChanged(_ => Apply(), true);
            settings.ProfileStatsPosition.BindValueChanged(_ => Apply(), true);

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
                        username.Alpha = 1;
                        flow.Add(username);
                        flow.Add(avatar);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.AvatarLeft:
                    case UserProfileDisplayMode.AvatarLeftWithSep:
                        avatar.Alpha = 1;
                        username.Alpha = 1;
                        flow.Add(avatar);
                        flow.Add(username);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.AvatarOnly:
                        avatar.Alpha = 1;
                        username.Alpha = 0;
                        flow.Add(avatar);
                        flow.Add(username);
                        foreach (var other in otherChildren) flow.Add(other);
                        break;

                    case UserProfileDisplayMode.UsernameOnly:
                        avatar.Alpha = 0;
                        username.Alpha = 1;
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
                var statsFlow = ReflectionHelper.FindStatsFlow(userButton);
                if (statsFlow == null) return;

                if (position == ProfileStatsPosition.Left)
                {
                    statsFlow.Anchor = Anchor.CentreLeft;
                    statsFlow.Origin = Anchor.CentreLeft;
                    if (statsFlow.Parent is FillFlowContainer parentFlow)
                    {
                        parentFlow.SetLayoutPosition(statsFlow, -100);
                    }
                }
                else
                {
                    statsFlow.Anchor = Anchor.CentreRight;
                    statsFlow.Origin = Anchor.CentreRight;
                    if (statsFlow.Parent is FillFlowContainer parentFlow)
                    {
                        parentFlow.SetLayoutPosition(statsFlow, 100);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("ApplyProfileStatsPosition failed", ex);
            }
        }
    }
}
