using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace InfoTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Dictionary<string, string> InfoToolsSettings { get; private set; } = new();

        private const string ConfigFileName = "resources/config.json";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadOrCreateSettings();
        }

        private static void LoadOrCreateSettings()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            if (!File.Exists(configPath))
            {
                // Create default config file
                InfoToolsSettings = new Dictionary<string, string>
                {
                    { "NavigationColor", "#2D2D30" }
                };
                Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
                File.WriteAllText(configPath, JsonSerializer.Serialize(InfoToolsSettings, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                string json = File.ReadAllText(configPath);
                try
                {
                    InfoToolsSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                catch
                {
                    InfoToolsSettings = new Dictionary<string, string>();
                }
            }
        }

        public static void SaveSettings()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            File.WriteAllText(configPath, JsonSerializer.Serialize(InfoToolsSettings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
