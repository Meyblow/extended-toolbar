using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game;
using osu.Game.Overlays;
using osuTK;
using osucc.Plugin;
using ExtendedToolbar.Models;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar.Tweaks
{
    /// <summary>
    /// Менеджер геометрии и расположения шторки списка уведомлений и всплывающих тостов.
    /// </summary>
    public partial class NotificationLayoutManager : Component
    {
        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private OverlayContainer? notificationOverlay;
        private Container? toastContainer;

        public NotificationLayoutManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            settings.NotificationSidebarPosition.BindValueChanged(_ => applySidebarPosition(), true);
            settings.ToastPosition.BindValueChanged(_ => applyToastPosition(), true);
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
                    applySidebarPosition();
                    applyToastPosition();
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

            // Ищем NotificationOverlay в дереве игры
            notificationOverlay = game.ChildrenOfType<OverlayContainer>()
                .FirstOrDefault(c => c.GetType().Name.Contains("Notification", StringComparison.OrdinalIgnoreCase));

            if (notificationOverlay != null)
            {
                ExtendedToolbarLog.Info($"NotificationLayoutManager: Found NotificationOverlay ({notificationOverlay.GetType().Name})");

                // Ищем контейнер всплывающих тостов (Toast / NotificationSection)
                toastContainer = notificationOverlay.ChildrenOfType<Container>()
                    .FirstOrDefault(c => c.GetType().Name.Contains("Toast", StringComparison.OrdinalIgnoreCase)
                                      || c.GetType().Name.Contains("Floating", StringComparison.OrdinalIgnoreCase)
                                      || (c.GetType().Name.Contains("FillFlow", StringComparison.OrdinalIgnoreCase) && c.Anchor == Anchor.TopRight));

                notificationOverlay.State.BindValueChanged(_ =>
                {
                    Scheduler.AddOnce(applySidebarPosition);
                });
            }
        }

        private void applySidebarPosition()
        {
            if (notificationOverlay == null || IsDisposed) return;

            try
            {
                bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;

                if (isLeft)
                {
                    notificationOverlay.Anchor = Anchor.TopLeft;
                    notificationOverlay.Origin = Anchor.TopLeft;

                    if (notificationOverlay.State.Value == Visibility.Hidden)
                    {
                        float width = notificationOverlay.DrawWidth > 0 ? notificationOverlay.DrawWidth : 400f;
                        notificationOverlay.X = -width;
                    }
                    else
                    {
                        notificationOverlay.X = 0f;
                    }
                }
                else
                {
                    notificationOverlay.Anchor = Anchor.TopRight;
                    notificationOverlay.Origin = Anchor.TopRight;

                    if (notificationOverlay.State.Value == Visibility.Hidden)
                    {
                        float width = notificationOverlay.DrawWidth > 0 ? notificationOverlay.DrawWidth : 400f;
                        notificationOverlay.X = width;
                    }
                    else
                    {
                        notificationOverlay.X = 0f;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("applySidebarPosition error", ex);
            }
        }

        private void applyToastPosition()
        {
            if (toastContainer == null || IsDisposed) return;

            try
            {
                var pos = settings.ToastPosition.Value;

                switch (pos)
                {
                    case ToastPosition.TopLeft:
                        toastContainer.Anchor = Anchor.TopLeft;
                        toastContainer.Origin = Anchor.TopLeft;
                        toastContainer.Margin = new MarginPadding { Top = 50, Left = 15 };
                        break;

                    case ToastPosition.TopCentre:
                        toastContainer.Anchor = Anchor.TopCentre;
                        toastContainer.Origin = Anchor.TopCentre;
                        toastContainer.Margin = new MarginPadding { Top = 50 };
                        break;

                    case ToastPosition.TopRight:
                        toastContainer.Anchor = Anchor.TopRight;
                        toastContainer.Origin = Anchor.TopRight;
                        toastContainer.Margin = new MarginPadding { Top = 50, Right = 15 };
                        break;

                    case ToastPosition.BottomLeft:
                        toastContainer.Anchor = Anchor.BottomLeft;
                        toastContainer.Origin = Anchor.BottomLeft;
                        toastContainer.Margin = new MarginPadding { Bottom = 60, Left = 15 };
                        break;

                    case ToastPosition.BottomRight:
                        toastContainer.Anchor = Anchor.BottomRight;
                        toastContainer.Origin = Anchor.BottomRight;
                        toastContainer.Margin = new MarginPadding { Bottom = 60, Right = 15 };
                        break;
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("applyToastPosition error", ex);
            }
        }

        private void applyMaxVisibleToasts()
        {
            if (toastContainer == null || IsDisposed) return;

            try
            {
                int max = settings.MaxVisibleToasts.Value;
                var toasts = toastContainer.Children.Where(c => c.IsAlive && c.Alpha > 0.01f).ToList();

                if (toasts.Count > max)
                {
                    // Плавно скрываем самые старые тосты, превышающие лимит
                    int toHide = toasts.Count - max;
                    for (int i = 0; i < toHide; i++)
                    {
                        toasts[i].FadeOut(200).Expire();
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

            // Периодически поддерживаем лимит тостов при поступлении новых уведомлений
            if (toastContainer != null && toastContainer.Children.Count > settings.MaxVisibleToasts.Value)
            {
                applyMaxVisibleToasts();
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            notificationOverlay = null;
            toastContainer = null;
            base.Dispose(isDisposing);
        }
    }
}
