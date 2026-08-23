using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using osucc.Data;
using ExtendedToolbar.Models;

namespace ExtendedToolbar.Utils
{
    /// <summary>
    /// Управляет JSON-пресетами расположения тулбара, используя IOsuCcStorage плагина (presets/*.json)
    /// или безопасный fallback на локальную директорию плагина.
    /// Предоставляет эталонный защищённый пресет "Default".
    /// </summary>
    public static class ToolbarPresetManager
    {
        public const string DefaultPresetName = "Default";
        private static IOsuCcStorage? storage;

        private static string FallbackPresetsDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu", "osu-cc", "plugins", "extended-toolbar", "presets");

        public static void Init(IOsuCcStorage? pluginStorage)
        {
            storage = pluginStorage;
            EnsureDefaultPresetsExist();
        }

        public static void EnsureDefaultPresetsExist()
        {
            try
            {
                if (storage != null)
                {
                    if (!storage.Exists($"presets/{DefaultPresetName}.json"))
                        storage.WriteJson($"presets/{DefaultPresetName}.json", ToolbarLayoutConfig.CreateDefault());

                    cleanLegacyPreset("Default (Ванильный)");
                    cleanLegacyPreset("Default / Vanilla");
                    cleanLegacyPreset("Centered (По центру)");
                }
                else
                {
                    if (!Directory.Exists(FallbackPresetsDirectory))
                        Directory.CreateDirectory(FallbackPresetsDirectory);

                    string def = Path.Combine(FallbackPresetsDirectory, $"{DefaultPresetName}.json");
                    if (!File.Exists(def))
                        ToolbarLayoutConfig.CreateDefault().Save(def);

                    cleanLegacyFallbackPreset("Default (Ванильный)");
                    cleanLegacyFallbackPreset("Default / Vanilla");
                    cleanLegacyFallbackPreset("Centered (По центру)");
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Failed to ensure default presets exist", ex);
            }
        }

        private static void cleanLegacyPreset(string name)
        {
            try
            {
                string path = $"presets/{name}.json";
                if (storage != null && storage.Exists(path))
                {
                    string? full = storage.GetFullPath(path);
                    if (full != null && File.Exists(full))
                        File.Delete(full);
                }
            }
            catch { }
        }

        private static void cleanLegacyFallbackPreset(string name)
        {
            try
            {
                string full = Path.Combine(FallbackPresetsDirectory, $"{name}.json");
                if (File.Exists(full))
                    File.Delete(full);
            }
            catch { }
        }

        public static List<string> GetAvailablePresets()
        {
            EnsureDefaultPresetsExist();
            var list = new List<string>();

            try
            {
                if (storage != null)
                {
                    var files = storage.GetFiles("presets", "*.json");
                    foreach (var file in files)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        if (isLegacyIgnored(name))
                            continue;

                        if (!name.Equals(DefaultPresetName, StringComparison.OrdinalIgnoreCase))
                            list.Add(name);
                    }
                }
                else if (Directory.Exists(FallbackPresetsDirectory))
                {
                    foreach (var file in Directory.GetFiles(FallbackPresetsDirectory, "*.json"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        if (isLegacyIgnored(name))
                            continue;

                        if (!name.Equals(DefaultPresetName, StringComparison.OrdinalIgnoreCase))
                            list.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Failed to list presets", ex);
            }

            list.Insert(0, DefaultPresetName);
            return list;
        }

        private static bool isLegacyIgnored(string name) =>
            name.Equals("Default (Ванильный)", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Default / Vanilla", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Centered (По центру)", StringComparison.OrdinalIgnoreCase);

        public static ToolbarLayoutConfig LoadPreset(string presetName)
        {
            EnsureDefaultPresetsExist();

            if (string.IsNullOrWhiteSpace(presetName) || presetName.Equals(DefaultPresetName, StringComparison.OrdinalIgnoreCase))
            {
                return ToolbarLayoutConfig.CreateDefault();
            }

            try
            {
                if (storage != null)
                {
                    string relativePath = $"presets/{presetName}.json";
                    if (storage.Exists(relativePath))
                    {
                        var loaded = storage.ReadJson<ToolbarLayoutConfig>(relativePath);
                        if (loaded != null) return loaded;
                    }
                }
                else
                {
                    string fullPath = Path.Combine(FallbackPresetsDirectory, $"{presetName}.json");
                    if (File.Exists(fullPath))
                        return ToolbarLayoutConfig.Load(fullPath);
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error($"Failed to load preset '{presetName}'", ex);
            }

            return ToolbarLayoutConfig.CreateDefault();
        }

        public static bool SaveCustomPreset(string presetName, ToolbarLayoutConfig config)
        {
            if (string.IsNullOrWhiteSpace(presetName) || presetName.Equals(DefaultPresetName, StringComparison.OrdinalIgnoreCase))
            {
                ExtendedToolbarLog.Warn($"SaveCustomPreset: Preset '{presetName}' is protected and cannot be overwritten.");
                return false;
            }

            try
            {
                if (storage != null)
                {
                    string relativePath = $"presets/{presetName}.json";
                    storage.WriteJson(relativePath, config);
                }
                else
                {
                    if (!Directory.Exists(FallbackPresetsDirectory))
                        Directory.CreateDirectory(FallbackPresetsDirectory);

                    string fullPath = Path.Combine(FallbackPresetsDirectory, $"{presetName}.json");
                    config.Save(fullPath);
                }
                ExtendedToolbarLog.Info($"SaveCustomPreset: Saved preset '{presetName}'");
                return true;
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error($"SaveCustomPreset failed for '{presetName}'", ex);
                return false;
            }
        }

        public static void OpenPresetsFolder()
        {
            try
            {
                string dir = storage?.GetFullPath("presets") ?? FallbackPresetsDirectory;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Failed to open presets folder", ex);
            }
        }
    }
}
