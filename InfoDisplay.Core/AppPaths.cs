using System.IO;
using System;

public static class AppPaths
{
    public static string BaseDirectory
    {
        get
        {
#if DEBUG
            //THIS WAS DONE WITH GPT
            // In debug: cartella "Data" nella root della solution (comune a tutti i progetti).
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // exeDir = ...\InfoDisplay.Player\bin\Debug\netcoreapp3.1\
            // projectDir = ...\InfoDisplay.Player\
            var projectDir = Directory.GetParent(exeDir)    // netcoreapp3.1
                                      .Parent              // Debug
                                      .Parent              // bin
                                      .Parent!.FullName;   // InfoDisplay.Player

            // solutionDir = cartella che contiene tutti i progetti (dove sta la .sln)
            var solutionDir = Directory.GetParent(projectDir)!.FullName;

            var dataDir = Path.Combine(solutionDir, "Data");
            Directory.CreateDirectory(dataDir);
            return dataDir;
#else
            // RELEASE
            return AppDomain.CurrentDomain.BaseDirectory;
#endif
        }
    }

    public static string SlidesJson =>
        Path.Combine(BaseDirectory, "Slides.json");

    public static string UpdateToken =>
        Path.Combine(BaseDirectory, "UpdateToken");

    public static string GlobalSettingsJson =>
        Path.Combine(BaseDirectory, "GlobalSettings.json");
}