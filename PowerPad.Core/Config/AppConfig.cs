using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PowerPad.Config
{
    public class AppConfig
    {
        public string WorkspacePath { get; set; } = string.Empty;

        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowerNotes", "config.json");

        public static AppConfig Load()
        {
            if (File.Exists(ConfigFilePath))
            {
                return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigFilePath)) ?? new AppConfig();
            }
            return new AppConfig();
        }

        public void Save()
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
