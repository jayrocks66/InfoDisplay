using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace InfoDisplayConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //TODO: save serialize and leave updatetoken, add default image


        List<InfoDisplay.Slide> slides;

        public MainWindow()
        {
            InitializeComponent();
            new InfoDisplay.Slide().PurgeExpired();
            slides = new InfoDisplay.Slide().ReadSlides();
            if (slides.Count < 1) slides.Add(new InfoDisplay.Slide().GenerateDefaultSlide(0));
            cbx_Slides.ItemsSource = slides;
            checkRemoveButton();

        }

        private void cbx_Slides_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataContext = cbx_Slides.SelectedItem;
            if (cbx_Slides.SelectedIndex > -1)
            {
                if (((InfoDisplay.Slide)cbx_Slides.SelectedItem).Layout == 1)
                {
                    rbt_Layout1.IsChecked = true;
                }
                else
                {
                    rbt_Layout0.IsChecked = true;
                }
            }

        }

        private void btn_slideImgSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(dlg.FileName);
                    bi.EndInit();
                    slides[cbx_Slides.SelectedIndex].SetImageBase64FromBitmapImage(bi);
                }
                catch
                {
                    MessageBox.Show("Il file non è un immagine oppure il formato non è supportato. Per favore riprova", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                slideRefresh();
            }
        }
        private void btn_slideImgDelete_Click(object sender, RoutedEventArgs e)
        {
            slides[cbx_Slides.SelectedIndex].ImageBase64 = "";
            slideRefresh();
        }

        private void slideRefresh()
        {
            //FORCE REFRESH...
            int oldIndex = cbx_Slides.SelectedIndex;
            cbx_Slides.SelectedIndex = -1;
            cbx_Slides.SelectedIndex = oldIndex;
        }
        private void rbt_Layout0_Checked(object sender, RoutedEventArgs e)
        {
            slides[cbx_Slides.SelectedIndex].Layout = 0;
        }

        private void rbt_Layout1_Checked(object sender, RoutedEventArgs e)
        {
            slides[cbx_Slides.SelectedIndex].Layout = 1;
        }

        private void btn_AddSlide_Click(object sender, RoutedEventArgs e)
        {
            slides.Add(new InfoDisplay.Slide().GenerateDefaultSlide(slides.Count - 1));
            cbx_Slides.ItemsSource = null;
            cbx_Slides.ItemsSource = slides;
            cbx_Slides.SelectedIndex = slides.Count - 1;
            checkRemoveButton();
        }

        private void btn_RemoveSlide_Click(object sender, RoutedEventArgs e)
        {
            int oldSelectedIndex = cbx_Slides.SelectedIndex;
            slides.Remove((InfoDisplay.Slide)cbx_Slides.SelectedItem);
            cbx_Slides.ItemsSource = null;
            cbx_Slides.ItemsSource = slides;
            if (oldSelectedIndex > 0) cbx_Slides.SelectedIndex = oldSelectedIndex - 1;
            else cbx_Slides.SelectedIndex = 0;
            checkRemoveButton();
        }

        private void checkRemoveButton()
        {
            btn_RemoveSlide.IsEnabled = false;
            if (slides.Count > 1) btn_RemoveSlide.IsEnabled = true;
        }
        private void checkSaveButton()
        {
            btn_Save.IsEnabled = false;
            if (!Validation.GetHasError(tb_SlideDuration)) btn_Save.IsEnabled = true;
        }

        private void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation.GetHasError(tb_SlideDuration))
            {
                string jsonString = JsonSerializer.Serialize(slides);
                File.WriteAllText("Slides.json", jsonString);
                File.WriteAllText("UpdateToken", "");
            }
        }

        private void tb_SlideDuration_TextChanged(object sender, TextChangedEventArgs e)
        {
            checkSaveButton();
        }

        private void btn_PreviewSlide_Click(object sender, RoutedEventArgs e)
        {
            InfoDisplay.MainWindow newForm = new InfoDisplay.MainWindow(true, slides[cbx_Slides.SelectedIndex]); ;
            newForm.Show();
        }

        private void btn_GlobalSettingsConfig_Click(object sender, RoutedEventArgs e)
        {
            new GlobalSettingsConfig().ShowDialog();
        }

        private void dpt_ExpirationDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpt_ExpirationDate.SelectedDate < DateTime.Today.AddDays(1))
            {
                dpt_ExpirationDate.SelectedDate = DateTime.Today.AddDays(1);
            }
        }
    }
}
