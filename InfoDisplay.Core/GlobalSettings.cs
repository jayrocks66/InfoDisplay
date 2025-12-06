using System;
using System.IO;
using System.Text.Json;

namespace InfoDisplay.Core
{
    public class GlobalSettings
    {

        public bool SlideAnimationsEnabled { get; set; } = true;
        public string FontFamilyName { get; set; } = "Segoe UI";

        /// <summary>
        /// Load settings from GlobalSettings.json.
        /// If no file, create a default one and save
        /// If file is corrupted, go back to default
        /// </summary>
        public static GlobalSettings Load()
        {
            // If file doesn't exist: default + save
            if (!File.Exists(AppPaths.GlobalSettingsJson))
            {
                var defaults = new GlobalSettings();
                defaults.Save();
                return defaults;
            }

            try
            {
                var json = File.ReadAllText(AppPaths.GlobalSettingsJson);
                var settings = JsonSerializer.Deserialize<GlobalSettings>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // fallback to default if null is returned
                return settings ?? new GlobalSettings();
            }
            catch
            {
                // fallback to default if JSON unreadable or corrupt
                return new GlobalSettings();
            }
        }

        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(AppPaths.GlobalSettingsJson, json);
        }
    }
}