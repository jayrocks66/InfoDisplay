using System;
using System.Drawing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;

namespace InfoDisplay.Config
{
    /// <summary>
    /// Logica di interazione per GlobalSettingsConfig.xaml
    /// </summary>
    public partial class GlobalSettingsConfig : Window
    {
        InfoDisplay.GlobalSettings settings = new InfoDisplay.GlobalSettings();
        public GlobalSettingsConfig()
        {
            InitializeComponent();
            settings = new InfoDisplay.GlobalSettings().ReadGlobalSettings();
            DataContext = settings;
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            new InfoDisplay.GlobalSettings().SaveGlobalSettings((InfoDisplay.GlobalSettings)DataContext);
            this.Close();
        }


    }
}
