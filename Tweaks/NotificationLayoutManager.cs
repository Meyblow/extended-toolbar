using System;
using System.Linq;
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

            notificationOverlay = game.ChildrenOfType<OverlayContainer>()
                .FirstOrDefault(c => c.GetType().Name.Contains("Notification", StringComparison.OrdinalIgnoreCase));

            if (notificationOverlay != null)
            {
                ExtendedToolbarLog.Info($"NotificationLayoutManager: Found NotificationOverlay ({notificationOverlay.GetType().Name})");

                toastContainer = notificationOverlay.ChildrenOfType<Container>()
                    .FirstOrDefault(c => c.GetType().Name.Contains("Toast", StringComparison.OrdinalIgnoreCase)
                                      || c.GetType().Name.Contains("Floating", StringComparison.OrdinalIgnoreCase)
                                      || (c.GetType().Name.Contains("FillFlow", StringComparison.OrdinalIgnoreCase) && c.Anchor == Anchor.TopRight));

                notificationOverlay.State.BindValueChanged(e =>
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
                bool isOpen = notificationOverlay.State.Value == Visibility.Visible;
                float width = notificationOverlay.DrawWidth > 0 ? notificationOverlay.DrawWidth : 400f;

                if (isOpen)
                {
                    // === Режим открытой шторки ===
                    bool isLeft = settings.NotificationSidebarPosition.Value == NotificationSidebarPosition.Left;

                    notificationOverlay.Anchor = isLeft ? Anchor.TopLeft : Anchor.TopRight;
                    notificationOverlay.Origin = isLeft ? Anchor.TopLeft : Anchor.TopRight;
                    notificationOverlay.X = 0f;
                    notificationOverlay.Y = 0f;
                }
                else
                {
                    // === Режим скрытой шторки (позиция всплывающих тостов) ===
                    var toastPos = settings.ToastPosition.Value;

                    switch (toastPos)
                    {
                        case ToastPosition.TopLeft:
                            notificationOverlay.Anchor = Anchor.TopLeft;
                            notificationOverlay.Origin = Anchor.TopLeft;
                            notificationOverlay.X = -width;
                            notificationOverlay.Y = 0f;
                            break;

                        case ToastPosition.TopCentre:
                            notificationOverlay.Anchor = Anchor.TopCentre;
                            notificationOverlay.Origin = Anchor.TopCentre;
                            notificationOverlay.X = 0f;
                            notificationOverlay.Y = 0f;
                            break;

                        case ToastPosition.TopRight:
                            notificationOverlay.Anchor = Anchor.TopRight;
                            notificationOverlay.Origin = Anchor.TopRight;
                            notificationOverlay.X = width;
                            notificationOverlay.Y = 0f;
                            break;

                        case ToastPosition.BottomLeft:
                            notificationOverlay.Anchor = Anchor.BottomLeft;
                            notificationOverlay.Origin = Anchor.BottomLeft;
                            notificationOverlay.X = -width;
                            notificationOverlay.Y = 0f;
                            break;

                        case ToastPosition.BottomRight:
                            notificationOverlay.Anchor = Anchor.BottomRight;
                            notificationOverlay.Origin = Anchor.BottomRight;
                            notificationOverlay.X = width;
                            notificationOverlay.Y = 0f;
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
            if (toastContainer == null || IsDisposed) return;

            try
            {
                int max = settings.MaxVisibleToasts.Value;
                var toasts = toastContainer.Children.Where(c => c.IsAlive && c.Alpha > 0.01f).ToList();

                if (toasts.Count > max)
                {
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
