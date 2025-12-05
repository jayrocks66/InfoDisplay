using InfoDisplay.Core;
using System.Windows;

namespace InfoDisplay.Config
{
    public partial class GlobalSettingsConfig : Window
    {
        private readonly GlobalSettings _settings;

        public GlobalSettingsConfig()
        {
            InitializeComponent();

            _settings = GlobalSettings.Load();
            DataContext = _settings;
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