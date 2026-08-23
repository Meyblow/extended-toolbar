using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.UI
{
    /// <summary>
    /// Интерактивный UI-виджет мини-монитора для выбора положения всплывающих уведомлений на экране.
    /// </summary>
    public partial class ToastPositionMonitorSelector : CompositeDrawable
    {
        public Bindable<ToastPosition> Current { get; }

        private readonly Dictionary<ToastPosition, ZoneBox> zoneBoxes = new();

        public ToastPositionMonitorSelector(Bindable<ToastPosition> current)
        {
            Current = current;
            RelativeSizeAxes = Axes.X;
            Height = 155f;
            Padding = new MarginPadding { Horizontal = 20f, Vertical = 4f };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Container screenContainer;

            InternalChild = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = 260,
                Height = 145,
                Children = new Drawable[]
                {
                    // Ножка монитора (Neck)
                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Y = -6,
                        Width = 16,
                        Height = 20,
                        Masking = true,
                        CornerRadius = 3,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.FromHex("#1e1e28")
                        }
                    },
                    // Подставка монитора (Base)
                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Width = 84,
                        Height = 8,
                        Masking = true,
                        CornerRadius = 4,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.FromHex("#262634")
                            },
                            new Box
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Height = 1.5f,
                                Colour = Colour4.White.Opacity(0.12f)
                            }
                        }
                    },
                    // Экран монитора (Screen Display)
                    screenContainer = new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Width = 250,
                        Height = 125,
                        Masking = true,
                        CornerRadius = 8,
                        BorderThickness = 2.5f,
                        BorderColour = Colour4.FromHex("#2c2c3c"),
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.FromHex("#101016")
                            },
                            // Верхняя панель задач монитора (имитация тулбара)
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 10,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colour4.FromHex("#181822")
                                    },
                                    new Box
                                    {
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        RelativeSizeAxes = Axes.X,
                                        Height = 1,
                                        Colour = Colour4.White.Opacity(0.08f)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            // Зоны экрана: TopLeft, TopCentre, TopRight, BottomLeft, BottomRight
            var zoneTopLeft = new ZoneBox(ToastPosition.TopLeft, "↖", Current, colours)
            {
                Position = new Vector2(5, 14),
                Size = new Vector2(74, 48)
            };
            var zoneTopCentre = new ZoneBox(ToastPosition.TopCentre, "⬆", Current, colours)
            {
                Position = new Vector2(85, 14),
                Size = new Vector2(76, 48)
            };
            var zoneTopRight = new ZoneBox(ToastPosition.TopRight, "↗", Current, colours)
            {
                Position = new Vector2(167, 14),
                Size = new Vector2(74, 48)
            };

            var zoneBottomLeft = new ZoneBox(ToastPosition.BottomLeft, "↙", Current, colours)
            {
                Position = new Vector2(5, 68),
                Size = new Vector2(116, 48)
            };
            var zoneBottomRight = new ZoneBox(ToastPosition.BottomRight, "↘", Current, colours)
            {
                Position = new Vector2(125, 68),
                Size = new Vector2(116, 48)
            };

            zoneBoxes[ToastPosition.TopLeft] = zoneTopLeft;
            zoneBoxes[ToastPosition.TopCentre] = zoneTopCentre;
            zoneBoxes[ToastPosition.TopRight] = zoneTopRight;
            zoneBoxes[ToastPosition.BottomLeft] = zoneBottomLeft;
            zoneBoxes[ToastPosition.BottomRight] = zoneBottomRight;

            screenContainer.AddRange(new Drawable[]
            {
                zoneTopLeft,
                zoneTopCentre,
                zoneTopRight,
                zoneBottomLeft,
                zoneBottomRight
            });

            Current.BindValueChanged(_ => updateSelection(), true);
        }

        private void updateSelection()
        {
            foreach (var kv in zoneBoxes)
            {
                kv.Value.SetSelected(kv.Key == Current.Value);
            }
        }

        /// <summary>
        /// Интерактивная зона внутри экрана монитора.
        /// </summary>
        private partial class ZoneBox : OsuClickableContainer
        {
            private readonly ToastPosition position;
            private readonly Bindable<ToastPosition> currentBindable;
            private readonly OsuColour colours;

            private Box backgroundBox = null!;
            private Container borderContainer = null!;
            private FillFlowContainer toastsPreview = null!;
            private SpriteText hintIcon = null!;

            public ZoneBox(ToastPosition position, string iconText, Bindable<ToastPosition> currentBindable, OsuColour colours)
            {
                this.position = position;
                this.currentBindable = currentBindable;
                this.colours = colours;

                Action = () => currentBindable.Value = position;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    borderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        BorderThickness = 1.5f,
                        BorderColour = Colour4.White.Opacity(0.1f),
                        Children = new Drawable[]
                        {
                            backgroundBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colour4.White.Opacity(0.04f)
                            },
                            hintIcon = new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = position switch
                                {
                                    ToastPosition.TopLeft => "Top Left",
                                    ToastPosition.TopCentre => "Top Centre",
                                    ToastPosition.TopRight => "Top Right",
                                    ToastPosition.BottomLeft => "Bottom Left",
                                    ToastPosition.BottomRight => "Bottom Right",
                                    _ => ""
                                },
                                Font = OsuFont.GetFont(size: 9, weight: FontWeight.SemiBold),
                                Colour = Colour4.White.Opacity(0.35f)
                            },
                            toastsPreview = new FillFlowContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 3),
                                Alpha = 0,
                                Children = new Drawable[]
                                {
                                    new Container
                                    {
                                        Width = 52,
                                        Height = 5,
                                        Masking = true,
                                        CornerRadius = 2.5f,
                                        Child = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.Pink
                                        }
                                    },
                                    new Container
                                    {
                                        Width = 42,
                                        Height = 4,
                                        Masking = true,
                                        CornerRadius = 2,
                                        Child = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.PinkLight
                                        }
                                    },
                                    new Container
                                    {
                                        Width = 32,
                                        Height = 4,
                                        Masking = true,
                                        CornerRadius = 2,
                                        Child = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colours.PinkLight.Opacity(0.7f)
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            }

            public void SetSelected(bool selected)
            {
                if (selected)
                {
                    borderContainer.BorderColour = colours.Pink;
                    backgroundBox.FadeColour(colours.Pink.Opacity(0.2f), 150);
                    hintIcon.FadeOut(120);
                    toastsPreview.FadeIn(180);
                    borderContainer.ScaleTo(1.03f, 150, Easing.OutQuint);
                }
                else
                {
                    borderContainer.BorderColour = Colour4.White.Opacity(0.12f);
                    backgroundBox.FadeColour(Colour4.White.Opacity(0.04f), 150);
                    hintIcon.FadeIn(120);
                    toastsPreview.FadeOut(120);
                    borderContainer.ScaleTo(1f, 150, Easing.OutQuint);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (currentBindable.Value != position)
                {
                    backgroundBox.FadeColour(Colour4.White.Opacity(0.12f), 100);
                    borderContainer.BorderColour = Colour4.White.Opacity(0.35f);
                }
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (currentBindable.Value != position)
                {
                    backgroundBox.FadeColour(Colour4.White.Opacity(0.04f), 150);
                    borderContainer.BorderColour = Colour4.White.Opacity(0.12f);
                }
                base.OnHoverLost(e);
            }
        }
    }
}
