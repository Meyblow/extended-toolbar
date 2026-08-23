using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Overlays.Settings;
using osucc.Client;
using osucc.Plugin;
using ExtendedToolbar.Localisation;
using ExtendedToolbar.Models;
using ExtendedToolbar.Tweaks;
using ExtendedToolbar.Utils;

namespace ExtendedToolbar.UI
{
    public partial class ExtendedToolbarSettingsSubsection : SettingsSubsection
    {
        protected override LocalisableString Header => ExtendedToolbarStrings.Header;

        private readonly ExtendedToolbarSettings settings;

        public ExtendedToolbarSettingsSubsection(ExtendedToolbarSettings settings)
        {
            this.settings = settings;

            // =========================================================================
            // 📁 СЕКЦИЯ 1: ПРЕСЕТЫ И МАКЕТ (LAYOUT & PRESETS)
            // =========================================================================
            Add(new SettingsSectionHeader(ExtendedToolbarStrings.SectionLayoutPresets));

            var presets = ToolbarPresetManager.GetAvailablePresets();
            var activePreset = settings.ActivePresetName.Value;
            if (!presets.Contains(activePreset))
            {
                activePreset = presets.FirstOrDefault() ?? "Default";
            }

            var presetDropdown = new SettingsDropdown<string>
            {
                LabelText = ExtendedToolbarStrings.PresetDropdownLabel,
                Current = new Bindable<string>(activePreset),
                Items = presets
            };

            presetDropdown.Current.BindValueChanged(e =>
            {
                if (!string.IsNullOrEmpty(e.NewValue) && e.NewValue != settings.ActivePresetName.Value)
                {
                    settings.ActivePresetName.Value = e.NewValue;
                    ToolbarLayoutManager.Instance?.ApplyPreset(e.NewValue);
                }
            });

            settings.ActivePresetName.BindValueChanged(e =>
            {
                if (presetDropdown.Current.Value != e.NewValue)
                {
                    presetDropdown.Current.Value = e.NewValue;
                }
            });

            Add(presetDropdown);

            // Главное целевое действие (Edit Mode)
            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonEnterEditMode,
                Margin = new MarginPadding { Top = 4f, Bottom = 2f },
                Action = () => ToolbarLayoutManager.Instance?.EnterEditMode()
            });

            // Компактная сетка 2x2 для управления пресетами и обмена
            Add(new SettingsDoubleButtonRow(
                ExtendedToolbarStrings.ButtonSavePreset,
                () =>
                {
                    ToolbarLayoutManager.Instance?.ShowSavePresetDialog(savedName =>
                    {
                        var updatedPresets = ToolbarPresetManager.GetAvailablePresets();
                        presetDropdown.Items = updatedPresets;
                        presetDropdown.Current.Value = savedName;
                    });
                },
                ExtendedToolbarStrings.ButtonOpenPresetsFolder,
                ToolbarPresetManager.OpenPresetsFolder
            ));

            Add(new SettingsDoubleButtonRow(
                ExtendedToolbarStrings.ButtonCopyCode,
                () =>
                {
                    var config = ToolbarLayoutManager.Instance?.GetCurrentConfig() ?? ToolbarLayoutConfig.CreateDefault();
                    string code = config.ExportCode();
                    var clipboard = ExtendedToolbarPlugin.Instance?.Host?.GetDependency<Clipboard>();
                    clipboard?.SetText(code);
                    ExtendedToolbarPlugin.Instance?.Host?.Notify(ExtendedToolbarStrings.NotifyClipboardCopied, NotificationKind.Success);
                },
                ExtendedToolbarStrings.ButtonImportCode,
                () =>
                {
                    var clipboard = ExtendedToolbarPlugin.Instance?.Host?.GetDependency<Clipboard>();
                    string? code = clipboard?.GetText();
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        ExtendedToolbarPlugin.Instance?.Host?.Notify(ExtendedToolbarStrings.NotifyClipboardEmpty, NotificationKind.Warning);
                        return;
                    }

                    var config = ToolbarLayoutConfig.ImportCode(code);
                    if (config != null)
                    {
                        ToolbarLayoutManager.Instance?.ApplyConfig(config);
                        settings.ActivePresetName.Value = "Imported Layout";
                        ExtendedToolbarPlugin.Instance?.Host?.Notify(ExtendedToolbarStrings.NotifyImportSuccess, NotificationKind.Success);
                    }
                    else
                    {
                        ExtendedToolbarPlugin.Instance?.Host?.Notify(ExtendedToolbarStrings.NotifyImportInvalid, NotificationKind.Error);
                    }
                }
            ));

            // Кнопка сброса
            Add(new DangerousSettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonResetToDefault,
                Margin = new MarginPadding { Top = 2f, Bottom = 6f },
                Action = () =>
                {
                    ToolbarLayoutManager.Instance?.ResetToDefault();
                    presetDropdown.Current.Value = "Default";
                }
            });

            // =========================================================================
            // 🏝️ СЕКЦИЯ 2: ПЛАВАЮЩИЙ ОСТРОВ И ГЕОМЕТРИЯ (FLOATING ISLAND & GEOMETRY)
            // =========================================================================
            Add(new SettingsSectionHeader(ExtendedToolbarStrings.SectionFloatingIsland));

            Add(new SettingsCheckbox
            {
                LabelText = ExtendedToolbarStrings.FloatingIslandCheckbox,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.FloatingIslandMode
            });

            var widthBindable = new BindableFloat(settings.ToolbarWidth.Value)
            {
                MinValue = 0.3f,
                MaxValue = 1f,
                Precision = 0.005f
            };
            widthBindable.BindValueChanged(e => settings.ToolbarWidth.Value = e.NewValue);
            settings.ToolbarWidth.BindValueChanged(e => widthBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarWidthSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = widthBindable,
                DisplayAsPercentage = true,
                KeyboardStep = 0.005f
            });

            var heightBindable = new BindableFloat(settings.ToolbarHeight.Value)
            {
                MinValue = 26f,
                MaxValue = 40f,
                Precision = 1f
            };
            heightBindable.BindValueChanged(e => settings.ToolbarHeight.Value = e.NewValue);
            settings.ToolbarHeight.BindValueChanged(e => heightBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarHeightSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = heightBindable,
                KeyboardStep = 1f
            });

            var cornerRadiusBindable = new BindableFloat(settings.ToolbarCornerRadius.Value)
            {
                MinValue = 0f,
                MaxValue = 24f,
                Precision = 1f
            };
            cornerRadiusBindable.BindValueChanged(e => settings.ToolbarCornerRadius.Value = e.NewValue);
            settings.ToolbarCornerRadius.BindValueChanged(e => cornerRadiusBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarCornerRadiusSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = cornerRadiusBindable,
                KeyboardStep = 1f
            });

            var opacityBindable = new BindableFloat(settings.ToolbarBackgroundOpacity.Value)
            {
                MinValue = 0f,
                MaxValue = 1f,
                Precision = 0.05f
            };
            opacityBindable.BindValueChanged(e => settings.ToolbarBackgroundOpacity.Value = e.NewValue);
            settings.ToolbarBackgroundOpacity.BindValueChanged(e => opacityBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.BackgroundOpacitySlider,
                Margin = new MarginPadding { Top = 4f },
                Current = opacityBindable,
                DisplayAsPercentage = true,
                KeyboardStep = 0.05f
            });

            var offsetXBindable = new BindableFloat(settings.ToolbarOffsetX.Value)
            {
                MinValue = -200f,
                MaxValue = 200f,
                Precision = 1f
            };
            offsetXBindable.BindValueChanged(e => settings.ToolbarOffsetX.Value = e.NewValue);
            settings.ToolbarOffsetX.BindValueChanged(e => offsetXBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarOffsetXSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = offsetXBindable,
                KeyboardStep = 1f
            });

            var offsetYBindable = new BindableFloat(settings.ToolbarOffsetY.Value)
            {
                MinValue = -50f,
                MaxValue = 50f,
                Precision = 1f
            };
            offsetYBindable.BindValueChanged(e => settings.ToolbarOffsetY.Value = e.NewValue);
            settings.ToolbarOffsetY.BindValueChanged(e => offsetYBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarOffsetYSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = offsetYBindable,
                KeyboardStep = 1f
            });

            var spacingBindable = new BindableFloat(settings.ToolbarSpacing.Value)
            {
                MinValue = 0f,
                MaxValue = 24f,
                Precision = 1f
            };
            spacingBindable.BindValueChanged(e => settings.ToolbarSpacing.Value = e.NewValue);
            settings.ToolbarSpacing.BindValueChanged(e => spacingBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.ToolbarSpacingSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = spacingBindable,
                KeyboardStep = 1f
            });

            Add(new SettingsCheckbox
            {
                LabelText = ExtendedToolbarStrings.AdaptScreensToIslandCheckbox,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.AdaptScreensToIsland
            });

            Add(new SettingsCheckbox
            {
                LabelText = ExtendedToolbarStrings.SeamlessRulesetSelectorCheckbox,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.SeamlessRulesetSelector
            });

            // =========================================================================
            // ✨ СЕКЦИЯ 3: ФОНОВЫЕ ЭФФЕКТЫ И ЗАТЕМНЕНИЕ (BACKGROUND EFFECTS)
            // =========================================================================
            Add(new SettingsSectionHeader(ExtendedToolbarStrings.SectionBackgroundEffects));

            var darkGlowBindable = new BindableFloat(settings.TopScreenDarkGlow.Value)
            {
                MinValue = 0f,
                MaxValue = 1f,
                Precision = 0.05f
            };
            darkGlowBindable.BindValueChanged(e => settings.TopScreenDarkGlow.Value = e.NewValue);
            settings.TopScreenDarkGlow.BindValueChanged(e => darkGlowBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.TopScreenDarkGlowSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = darkGlowBindable,
                DisplayAsPercentage = true,
                KeyboardStep = 0.05f
            });

            // =========================================================================
            // 🔔 СЕКЦИЯ 4: УВЕДОМЛЕНИЯ И ШТОРКА (NOTIFICATIONS & SIDEBAR)
            // =========================================================================
            Add(new SettingsSectionHeader(ExtendedToolbarStrings.SectionNotifications));

            Add(new ToastPositionMonitorSelector(settings.ToastPosition));

            Add(new SettingsEnumDropdown<NotificationSidebarPosition>
            {
                LabelText = ExtendedToolbarStrings.NotificationSidebarPositionDropdown,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.NotificationSidebarPosition
            });

            var maxToastsBindable = new BindableInt(settings.MaxVisibleToasts.Value)
            {
                MinValue = 1,
                MaxValue = 5,
                Precision = 1
            };
            maxToastsBindable.BindValueChanged(e => settings.MaxVisibleToasts.Value = e.NewValue);
            settings.MaxVisibleToasts.BindValueChanged(e => maxToastsBindable.Value = e.NewValue);

            Add(new SettingsSlider<int>
            {
                LabelText = ExtendedToolbarStrings.MaxVisibleToastsSlider,
                Margin = new MarginPadding { Top = 4f },
                Current = maxToastsBindable,
                KeyboardStep = 1
            });

            // =========================================================================
            // 👤 СЕКЦИЯ 5: ПРОФИЛЬ И РАЗДЕЛИТЕЛИ (PROFILE & SPACERS)
            // =========================================================================
            Add(new SettingsSectionHeader(ExtendedToolbarStrings.SectionProfileSpacers));

            Add(new SettingsEnumDropdown<UserProfileDisplayMode>
            {
                LabelText = ExtendedToolbarStrings.ProfileModeDropdown,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.UserProfileDisplayMode
            });

            Add(new SettingsEnumDropdown<ProfileStatsPosition>
            {
                LabelText = ExtendedToolbarStrings.ProfileStatsPositionDropdown,
                Margin = new MarginPadding { Top = 4f },
                Current = settings.ProfileStatsPosition
            });

            Add(new SettingsEnumDropdown<SpacerStyle>
            {
                LabelText = ExtendedToolbarStrings.SpacerStyleDropdown,
                Margin = new MarginPadding { Top = 4f, Bottom = 8f },
                Current = settings.SpacerStyle
            });
        }
    }
}
