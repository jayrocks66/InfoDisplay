using InfoDisplay.Core;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace InfoDisplay.Config
{
    public partial class GlobalSettingsConfig : Window
    {
        private readonly GlobalSettings _settings;
        public List<string> AvailableFonts { get; }

        public GlobalSettingsConfig()
        {
            InitializeComponent();

            // Carica impostazioni
            _settings = GlobalSettings.Load();

            // Costruisci lista font di sistema (nomi) ordinata alfabeticamente
            AvailableFonts = Fonts.SystemFontFamilies
                                  .Select(f => f.Source)
                                  .Distinct()
                                  .OrderBy(name => name)
                                  .ToList();

            // Imposta DataContext a un oggetto anonimo che espone sia settings che lista font
            DataContext = new
            {
                Settings = _settings,
                Fonts = AvailableFonts
            };
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            DialogResult = true; // opzionale
            Close();
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}