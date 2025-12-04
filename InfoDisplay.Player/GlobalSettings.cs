using System.Drawing;
using System.IO;
using System.Text.Json;


namespace InfoDisplay
{
    public class GlobalSettings
    {
        public GlobalSettings() { }
        public bool SlideAnimationsEnabled { get; set; }
        public GlobalSettings ReadGlobalSettings()
        {
            GlobalSettings gs = new GlobalSettings();
            if (File.Exists("GlobalSettings.json")) gs = (InfoDisplay.GlobalSettings)JsonSerializer.Deserialize(File.ReadAllText("GlobalSettings.json"), typeof(InfoDisplay.GlobalSettings));
            return gs;
        }

        public GlobalSettings SaveGlobalSettings(GlobalSettings gs)
        {
            File.WriteAllText("GlobalSettings.json", JsonSerializer.Serialize(gs));
            File.WriteAllText("UpdateToken", "");
            return gs;
        }
    }
}
