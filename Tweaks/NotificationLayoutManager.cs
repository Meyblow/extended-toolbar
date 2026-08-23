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
using osuTK;
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
        private static readonly FieldInfo? toastFlowField = typeof(NotificationOverlay).Assembly.GetType("osu.Game.Overlays.NotificationOverlayToastTray")?.GetField("toastFlow", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly IOsuCcPluginHost host;
        private readonly ExtendedToolbarSettings settings;

        private OsuGame? gameInstance;
        private NotificationOverlay? notificationOverlay;
        private Container? mainContent;
        private CompositeDrawable? toastTray;

        public NotificationLayoutManager(IOsuCcPluginHost host, ExtendedToolbarSettings settings)
        {
            this.host = host;
            this.settings = settings;
            AlwaysPresent = true;

            settings.NotificationSidebarPosition.BindValueChanged(_ => updateLayout(), true);
            settings.ToastPosition.BindValueChanged(_ => updateLayout(), true);
            settings.MaxVisibleToasts.BindValueChanged(_ => applyMaxVisibleToasts(), true);
        }

        public void Attach(OsuGame game)
        {
            if (IsDisposed) return;

            gameInstance = game;

            host.Scheduler?.Add(() =>
            {
                try
                {
                    findNotificationComponents();
                    updateLayout();
                }
                catch (Exception ex)
                {
                    ExtendedToolbarLog.Error("Error attaching NotificationLayoutManager", ex);
                }
            });
        }

        private void findNotificationComponents()
        {
            if (IsDisposed || gameInstance == null) return;

            if (notificationOverlay == null)
            {
                notificationOverlay = gameInstance.ChildrenOfType<NotificationOverlay>().FirstOrDefault();
                if (notificationOverlay != null)
                {
                    ExtendedToolbarLog.Info($"NotificationLayoutManager: Found NotificationOverlay ({notificationOverlay.GetHashCode()})");
                    notificationOverlay.State.BindValueChanged(_ =>
                    {
                        Scheduler.AddOnce(updateLayout);
                    });
                }
            }

            if (notificationOverlay != null)
            {
                if (mainContent == null)
                {
                    mainContent = mainContentField?.GetValue(notificationOverlay) as Container
                                  ?? notificationOverlay.ChildrenOfType<Container>().FirstOrDefault(c => c.Name == "mainContent");
                }

                if (toastTray == null)
                {
                    toastTray = toastTrayField?.GetValue(notificationOverlay) as CompositeDrawable
                                ?? notificationOverlay.ChildrenOfType<CompositeDrawable>().FirstOrDefault(c => c.GetType().Name.Contains("ToastTray", StringComparison.OrdinalIgnoreCase));

                    if (toastTray != null)
                    {
                        ExtendedToolbarLog.Info($"NotificationLayoutManager: Found toastTray ({toastTray.GetHashCode()})");
                    }
                }
            }
        }

        private void updateLayout()
        {
            if (IsDisposed) return;

            if (notificationOverlay == null || mainContent == null || toastTray == null)
            {
                findNotificationComponents();
            }

            if (notificationOverlay == null) return;

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

                // 2. Позиционирование всплывающих тостов (toastTray + toastFlow)
                if (toastTray != null)
                {
                    var toastPos = settings.ToastPosition.Value;

                    Anchor anchor = toastPos switch
                    {
                        ToastPosition.TopLeft => Anchor.TopLeft,
                        ToastPosition.TopCentre => Anchor.TopCentre,
                        ToastPosition.TopRight => Anchor.TopRight,
                        ToastPosition.BottomLeft => Anchor.BottomLeft,
                        ToastPosition.BottomRight => Anchor.BottomRight,
                        _ => Anchor.TopRight
                    };

                    MarginPadding margin = toastPos switch
                    {
                        ToastPosition.TopLeft => new MarginPadding { Top = 50, Left = 15 },
                        ToastPosition.TopCentre => new MarginPadding { Top = 50 },
                        ToastPosition.TopRight => new MarginPadding { Top = 50, Right = 15 },
                        ToastPosition.BottomLeft => new MarginPadding { Bottom = 60, Left = 15 },
                        ToastPosition.BottomRight => new MarginPadding { Bottom = 60, Right = 15 },
                        _ => new MarginPadding { Top = 50, Right = 15 }
                    };

                    toastTray.Anchor = anchor;
                    toastTray.Origin = anchor;
                    toastTray.Position = Vector2.Zero;
                    toastTray.Margin = margin;

                    if (toastFlowField?.GetValue(toastTray) is Drawable toastFlow)
                    {
                        toastFlow.Anchor = anchor;
                        toastFlow.Origin = anchor;
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

            if (toastTray == null || mainContent == null)
            {
                findNotificationComponents();
                if (toastTray != null || mainContent != null)
                {
                    updateLayout();
                }
            }

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
            gameInstance = null;
            notificationOverlay = null;
            mainContent = null;
            toastTray = null;
            base.Dispose(isDisposing);
        }
    }
}
