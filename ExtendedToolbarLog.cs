using System;
using System.IO;
using osucc.Plugin;

namespace ExtendedToolbar
{
    /// <summary>
    /// Подробный логгер для плагина Extended Toolbar.
    /// </summary>
    public static class ExtendedToolbarLog
    {
        private static IOsuCcPluginHost? host;
        private static string? logFilePath;

        public static void Init(IOsuCcPluginHost pluginHost)
        {
            host = pluginHost;
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                logFilePath = Path.Combine(userProfile, "extended_toolbar_debug.log");

                Info("========================================");
                Info($"Extended Toolbar logging initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
                Info($"Plugin Directory: {host.PluginDirectory}");
                Info("========================================");
            }
            catch (Exception ex)
            {
                host.Log($"Failed to init file logger: {ex.Message}");
            }
        }

        public static void Info(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [INFO] {message}";
            host?.Log(message);
            writeToFile(line);
        }

        public static void Warn(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [WARN] {message}";
            host?.Log($"[WARN] {message}");
            writeToFile(line);
        }

        public static void Error(string message, Exception? ex = null)
        {
            string details = ex != null ? $"\nException: {ex.GetType().FullName}: {ex.Message}\nStack Trace:\n{ex.StackTrace}" : "";
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] [ERROR] {message}{details}";
            host?.Log($"[ERROR] {message} {ex?.Message}");
            writeToFile(line);
        }

        private static void writeToFile(string line)
        {
            if (string.IsNullOrEmpty(logFilePath))
                return;

            try
            {
                File.AppendAllText(logFilePath, line + "\r\n");
            }
            catch
            {
                // ignore file IO errors
            }
        }
    }
}
