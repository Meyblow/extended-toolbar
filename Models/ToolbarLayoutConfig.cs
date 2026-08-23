using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExtendedToolbar.Models
{
    public class ToolbarItemConfig
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("hidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("width")]
        public float? CustomWidth { get; set; }
    }

    public class ToolbarLayoutConfig
    {
        private const string CodePrefix = "ET_LAYOUT_v1:";
        private static readonly JsonSerializerOptions serializerOptions = new() { WriteIndented = true };

        [JsonPropertyName("left")]
        public List<ToolbarItemConfig> Left { get; set; } = new();

        [JsonPropertyName("center")]
        public List<ToolbarItemConfig> Center { get; set; } = new();

        [JsonPropertyName("right")]
        public List<ToolbarItemConfig> Right { get; set; } = new();

        public static ToolbarLayoutConfig CreateDefault()
        {
            return new ToolbarLayoutConfig
            {
                Left = new List<ToolbarItemConfig>
                {
                    new() { Id = "settings" },
                    new() { Id = "home" },
                    new() { Id = "rulesets" }
                },
                Center = new List<ToolbarItemConfig>(),
                Right = new List<ToolbarItemConfig>
                {
                    new() { Id = "rankings" },
                    new() { Id = "news" },
                    new() { Id = "changelog" },
                    new() { Id = "wiki" },
                    new() { Id = "beatmap_listing" },
                    new() { Id = "chat" },
                    new() { Id = "social" },
                    new() { Id = "music" },
                    new() { Id = "user_profile" },
                    new() { Id = "clock" },
                    new() { Id = "notifications" }
                }
            };
        }

        public static ToolbarLayoutConfig CreateCentered()
        {
            return new ToolbarLayoutConfig
            {
                Left = new List<ToolbarItemConfig>
                {
                    new() { Id = "settings" },
                    new() { Id = "home" },
                    new() { Id = "rulesets" }
                },
                Center = new List<ToolbarItemConfig>
                {
                    new() { Id = "clock" },
                    new() { Id = "user_profile" },
                    new() { Id = "music" }
                },
                Right = new List<ToolbarItemConfig>
                {
                    new() { Id = "chat" },
                    new() { Id = "social" },
                    new() { Id = "notifications" },
                    new() { Id = "rankings" },
                    new() { Id = "news" },
                    new() { Id = "changelog" },
                    new() { Id = "wiki" },
                    new() { Id = "beatmap_listing" }
                }
            };
        }

        private static readonly JsonSerializerOptions compactSerializerOptions = new() { WriteIndented = false };

        public string ExportCode()
        {
            string json = JsonSerializer.Serialize(this, compactSerializerOptions);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return CodePrefix + Convert.ToBase64String(bytes);
        }

        public static ToolbarLayoutConfig? ImportCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            code = code.Trim();
            // Accept both ET_LAYOUT_v1: and legacy OT_LAYOUT_v1:
            if (code.StartsWith("OT_LAYOUT_v1:", StringComparison.OrdinalIgnoreCase))
                code = CodePrefix + code.Substring("OT_LAYOUT_v1:".Length);

            if (!code.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                string base64 = code.Substring(CodePrefix.Length).Trim();
                byte[] bytes = Convert.FromBase64String(base64);
                string json = Encoding.UTF8.GetString(bytes);
                return JsonSerializer.Deserialize<ToolbarLayoutConfig>(json);
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error("Failed to import layout code", ex);
                return null;
            }
        }

        public static ToolbarLayoutConfig Load(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<ToolbarLayoutConfig>(json);
                    if (config != null)
                        return config;
                }
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error($"Failed to load layout from {filePath}, using default", ex);
            }

            return CreateDefault();
        }

        public void Save(string filePath)
        {
            try
            {
                string dir = Path.GetDirectoryName(filePath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(this, serializerOptions);
                File.WriteAllText(filePath, json);
                ExtendedToolbarLog.Info($"Toolbar layout successfully saved to {filePath}");
            }
            catch (Exception ex)
            {
                ExtendedToolbarLog.Error($"Failed to save layout to {filePath}", ex);
            }
        }
    }
}
