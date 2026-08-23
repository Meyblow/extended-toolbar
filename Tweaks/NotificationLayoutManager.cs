using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays;
using osucc.Plugin;
using ExtendedToolbar.Models;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar.Tweaks
{
    /// <summary>
    /// Менеджер геометрии и расположения шторки списка уведомлений и всплывающих тостов (ToastTray).
    /// </summary>
    public partial class NotificationLayoutManager : Component
    {
        private static readonly FieldInfo? mainContentField = typeof(NotificationOverlay).GetField("mainContent", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo? toastTrayField = typeof(NotificationOverlay).GetField("toastTray", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private NotificationOverlay? notificationOverlay;
        private Container? mainContent;
        private CompositeDrawable? toastTray;

        public NotificationLayoutManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            settings.NotificationSidebarPosition.BindValueChanged(_ => updateLayout(), true);
            settings.ToastPosition.BindValueChanged(_ => updateLayout(), true);
            settings.MaxVisibleToasts.BindValueChanged(_ => applyMaxVisibleToasts(), true);
        }

        public void Attach(OsuGame game)
        {
            if (IsDisposed) return;

            host.Scheduler?.Add(() =>
            {
                try
                {
                    findNotificationComponents(game);
                    updateLayout();
                }
                catch (Exception ex)
                {
                    ExtendedToolbarLog.Error("Error attaching NotificationLayoutManager", ex);
                }
            });
        }

        private void findNotificationComponents(OsuGame game)
        {
            if (IsDisposed) return;

            notificationOverlay = game.ChildrenOfType<NotificationOverlay>().FirstOrDefault();

            if (notificationOverlay != null)
            {
                ExtendedToolbarLog.Info($"NotificationLayoutManager: Found NotificationOverlay ({notificationOverlay.GetHashCode()})");

                mainContent = mainContentField?.GetValue(notificationOverlay) as Container;
                toastTray = toastTrayField?.GetValue(notificationOverlay) as CompositeDrawable;

                notificationOverlay.State.BindValueChanged(_ =>
                {
                    Scheduler.AddOnce(updateLayout);
                });
            }
        }

        private void updateLayout()
        {
            if (notificationOverlay == null || IsDisposed) return;

            try
            {
                // 1. Позиционирование шторки (mainContent)
                if (mainContent != null)
                {
                    bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;
                    float width = mainContent.DrawWidth > 0 ? mainContent.DrawWidth : 400f;

                    mainContent.Anchor = isLeft ? Anchor.TopLeft : Anchor.TopRight;
                    mainContent.Origin = isLeft ? Anchor.TopLeft : Anchor.TopRight;

                    if (notificationOverlay.State.Value == Visibility.Hidden)
                    {
                        mainContent.X = isLeft ? -width : width;
                    }
                    else
                    {
                        mainContent.X = 0f;
                    }
                }

                // 2. Позиционирование всплывающих тостов (toastTray)
                if (toastTray != null)
                {
                    var toastPos = settings.ToastPosition.Value;

                    switch (toastPos)
                    {
                        case ToastPosition.TopLeft:
                            toastTray.Anchor = Anchor.TopLeft;
                            toastTray.Origin = Anchor.TopLeft;
                            toastTray.Margin = new MarginPadding { Top = 50, Left = 15 };
                            break;

                        case ToastPosition.TopCentre:
                            toastTray.Anchor = Anchor.TopCentre;
                            toastTray.Origin = Anchor.TopCentre;
                            toastTray.Margin = new MarginPadding { Top = 50 };
                            break;

                        case ToastPosition.TopRight:
                            toastTray.Anchor = Anchor.TopRight;
                            toastTray.Origin = Anchor.TopRight;
                            toastTray.Margin = new MarginPadding { Top = 50, Right = 15 };
                            break;

                        case ToastPosition.BottomLeft:
                            toastTray.Anchor = Anchor.BottomLeft;
                            toastTray.Origin = Anchor.BottomLeft;
                            toastTray.Margin = new MarginPadding { Bottom = 60, Left = 15 };
                            break;

                        case ToastPosition.BottomRight:
                            toastTray.Anchor = Anchor.BottomRight;
                            toastTray.Origin = Anchor.BottomRight;
                            toastTray.Margin = new MarginPadding { Bottom = 60, Right = 15 };
                            break;
                    }
                }

                applyMaxVisibleToasts();
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("updateLayout error", ex);
            }
        }

        private void applyMaxVisibleToasts()
        {
            if (toastTray == null || IsDisposed) return;

            try
            {
                int max = settings.MaxVisibleToasts.Value;

                var flow = toastTray.ChildrenOfType<Container>()
                    .FirstOrDefault(c => c.GetType().Name.Contains("FillFlow", StringComparison.OrdinalIgnoreCase));

                if (flow != null)
                {
                    var toasts = flow.Children.Where(c => c.IsAlive && c.Alpha > 0.01f).ToList();
                    if (toasts.Count > max)
                    {
                        int toHide = toasts.Count - max;
                        for (int i = 0; i < toHide; i++)
                        {
                            toasts[i].FadeOut(200).Expire();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("applyMaxVisibleToasts error", ex);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (IsDisposed) return;

            if (toastTray != null)
            {
                var flow = toastTray.ChildrenOfType<Container>()
                    .FirstOrDefault(c => c.GetType().Name.Contains("FillFlow", StringComparison.OrdinalIgnoreCase));

                if (flow != null && flow.Children.Count > settings.MaxVisibleToasts.Value)
                {
                    applyMaxVisibleToasts();
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            notificationOverlay = null;
            mainContent = null;
            toastTray = null;
            base.Dispose(isDisposing);
        }
    }
}
