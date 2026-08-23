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

            // ==========================================
            // РАЗДЕЛ 1: ТУЛБАР И ПРЕСЕТЫ
            // ==========================================
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

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonEnterEditMode,
                Margin = new MarginPadding { Top = 6f },
                Action = () => ToolbarLayoutManager.Instance?.EnterEditMode()
            });

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonSavePreset,
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    ToolbarLayoutManager.Instance?.ShowSavePresetDialog(savedName =>
                    {
                        var updatedPresets = ToolbarPresetManager.GetAvailablePresets();
                        presetDropdown.Items = updatedPresets;
                        presetDropdown.Current.Value = savedName;
                    });
                }
            });

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonCopyCode,
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
                {
                    var config = ToolbarLayoutManager.Instance?.GetCurrentConfig() ?? ToolbarLayoutConfig.CreateDefault();
                    string code = config.ExportCode();
                    var clipboard = ExtendedToolbarPlugin.Instance?.Host?.GetDependency<Clipboard>();
                    clipboard?.SetText(code);
                    ExtendedToolbarPlugin.Instance?.Host?.Notify(ExtendedToolbarStrings.NotifyClipboardCopied, NotificationKind.Success);
                }
            });

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonImportCode,
                Margin = new MarginPadding { Top = 6f },
                Action = () =>
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
            });

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonOpenPresetsFolder,
                Margin = new MarginPadding { Top = 6f },
                Action = ToolbarPresetManager.OpenPresetsFolder
            });

            Add(new SettingsButton
            {
                Text = ExtendedToolbarStrings.ButtonResetToDefault,
                Margin = new MarginPadding { Top = 6f, Bottom = 6f },
                Action = () =>
                {
                    ToolbarLayoutManager.Instance?.ResetToDefault();
                    presetDropdown.Current.Value = "Default";
                }
            });

            // ==========================================
            // РАЗДЕЛ 2: ВИЗУАЛЬНЫЙ СТИЛЬ (AESTHETICS)
            // ==========================================
            Add(new SettingsCheckbox
            {
                LabelText = ExtendedToolbarStrings.FloatingIslandCheckbox,
                Margin = new MarginPadding { Top = 10f },
                Current = settings.FloatingIslandMode
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
                Margin = new MarginPadding { Top = 6f },
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
                Margin = new MarginPadding { Top = 6f },
                Current = opacityBindable,
                DisplayAsPercentage = true,
                KeyboardStep = 0.05f
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
                Margin = new MarginPadding { Top = 6f },
                Current = heightBindable,
                KeyboardStep = 1f
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
                Margin = new MarginPadding { Top = 6f },
                Current = widthBindable,
                DisplayAsPercentage = true,
                KeyboardStep = 0.005f
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
                Margin = new MarginPadding { Top = 6f },
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
                Margin = new MarginPadding { Top = 6f },
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
                Margin = new MarginPadding { Top = 6f },
                Current = spacingBindable,
                KeyboardStep = 1f
            });

            Add(new SettingsCheckbox
            {
                LabelText = ExtendedToolbarStrings.NeonGlowLineCheckbox,
                Margin = new MarginPadding { Top = 6f },
                Current = settings.NeonGlowLine
            });

            var glowOffsetBindable = new BindableFloat(settings.NeonGlowOffset.Value)
            {
                MinValue = -5f,
                MaxValue = 15f,
                Precision = 1f
            };
            glowOffsetBindable.BindValueChanged(e => settings.NeonGlowOffset.Value = e.NewValue);
            settings.NeonGlowOffset.BindValueChanged(e => glowOffsetBindable.Value = e.NewValue);

            Add(new SettingsSlider<float>
            {
                LabelText = ExtendedToolbarStrings.NeonGlowOffsetSlider,
                Margin = new MarginPadding { Top = 6f },
                Current = glowOffsetBindable,
                KeyboardStep = 1f
            });

            Add(new SettingsEnumDropdown<ToolbarAccentColor>
            {
                LabelText = ExtendedToolbarStrings.NeonAccentColorDropdown,
                Margin = new MarginPadding { Top = 6f },
                Current = settings.ToolbarAccentColor
            });

            // ==========================================
            // РАЗДЕЛ 3: РАЗДЕЛИТЕЛИ (СПЕЙСЕРЫ)
            // ==========================================
            Add(new SettingsEnumDropdown<SpacerStyle>
            {
                LabelText = ExtendedToolbarStrings.SpacerStyleDropdown,
                Margin = new MarginPadding { Top = 10f },
                Current = settings.SpacerStyle
            });

            // ==========================================
            // РАЗДЕЛ 4: ПРОФИЛЬ ПОЛЬЗОВАТЕЛЯ
            // ==========================================
            Add(new SettingsEnumDropdown<UserProfileDisplayMode>
            {
                LabelText = ExtendedToolbarStrings.ProfileModeDropdown,
                Margin = new MarginPadding { Top = 10f },
                Current = settings.UserProfileDisplayMode
            });

            Add(new SettingsEnumDropdown<ProfileStatsPosition>
            {
                LabelText = ExtendedToolbarStrings.ProfileStatsPositionDropdown,
                Margin = new MarginPadding { Top = 6f },
                Current = settings.ProfileStatsPosition
            });
        }
    }
}
