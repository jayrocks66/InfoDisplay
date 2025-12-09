using InfoDisplay.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace InfoDisplay.Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        private List<Slide> slides;
        private GlobalSettings settings;
        private readonly DropShadowEffect slideBoxesShadowEffect = new DropShadowEffect();

        private CancellationTokenSource _cts;
        private Task _slideTask;
        private Task _timeTask;
        private Task _updateTask;

        //TODO: Customizable colors through style classes, rewrite animations, show images when calling from config

        public MainWindow(bool previewMode = false, Slide previewSlide = null)
        {
            settings = GlobalSettings.Load();
            new Slide().PurgeExpired();

            this.FontFamily = ResolveFontFamily(settings.FontFamilyName);

            InitializeComponent();
            if (previewMode == true)
            {
                ChangeLayout(previewSlide.Layout);
                tb_SlideContent.Text = previewSlide.Content;
                tb_SlideTitle.Text = previewSlide.Title;
                img_SlideImage.Source = previewSlide.Image;
                btn_ClosePreview.Visibility = Visibility.Visible;

            }
            else
            {
                slides = new Slide().ReadSlides();
                if (slides == null || slides.Count == 0)
                {
                    slides = new List<Slide>
                    {
                        new Slide
                        {
                            Title = "Nessuna slide configurata",
                            Content = "Apri InfoDisplayConfig e aggiungi almeno una slide.",
                            Layout = 1,
                            Duration = 15000 
                        }
                    };
                }

                brd_ContentBox.Effect = brd_ImgBox1.Effect = brd_TitleBox.Effect = slideBoxesShadowEffect;

                _cts = new CancellationTokenSource();
                var ct = _cts.Token;

                _slideTask = RunSlideLoopAsync(ct);
                _timeTask = RunTimeLoopAsync(ct);
                _updateTask = RunUpdateLoopAsync(ct);
            }
        }

        private async Task RunUpdateLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (File.Exists(AppPaths.UpdateToken))
                    {
                        try
                        {
                            File.Delete(AppPaths.UpdateToken);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(
                                "Errore durante la cancellazione del file UpdateToken. " +
                                "Verificare che l'app abbia i permessi di scrittura nella directory in cui viene eseguita, " +
                                "altrimenti si verificherà quest'errore ad ogni tentativo di aggiornamento dei contenuti.\r\n\r\n" +
                                "Exception dump:\r\n" + e.Message,
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            Environment.Exit(1);
                        }

                        slides = new Slide().ReadSlides();
                        settings = GlobalSettings.Load();

                        if (slides == null || slides.Count == 0)
                        {
                            slides = new List<Slide>
                    {
                        new Slide(
                            "Nessuna slide configurata",
                            "Apri InfoDisplay.Config e crea una nuova slide.",
                            0)
                    };
                        }
                    }

                    await Task.Delay(10000, ct);
                }
            }
            catch (TaskCanceledException)
            {
                // normale in chiusura
            }
        }
        public async void PurgeExpired(CancellationToken cancellationToken)
        {
            while (true)
            {
                bool somethingWasPurged = new Slide().PurgeExpired();
                if (somethingWasPurged)
                {
                    slides = new Slide().ReadSlides();
                }
                await Task.Delay(10000);
            }
        }

        private async Task RunTimeLoopAsync(CancellationToken ct)
        {
            try
            {
                var now = DateTime.Now;

                // Inizializza subito la data e l'ora
                tb_liveDate.Text = now.ToString("ddd dd/MM/yyyy");
                tb_liveTime.Text = now.ToString("t");

                var lastDate = now.Date;

                while (!ct.IsCancellationRequested)
                {
                    now = DateTime.Now;

                    tb_liveTime.Text = now.ToString("t");

                    if (now.Date != lastDate)
                    {
                        tb_liveDate.Text = now.ToString("ddd dd/MM/yyyy");
                        lastDate = now.Date;
                    }

                    await Task.Delay(1000, ct);
                }
            }
            catch (TaskCanceledException){}
        }


        private async Task RunSlideLoopAsync(CancellationToken ct)
        {
            // Protezione ulteriore: se per qualche motivo slides è vuota
            if (slides == null || slides.Count == 0)
                return;

            int index = -1;
            int previousLayout = slides.Last().Layout;
            ChangeLayout(slides.First().Layout);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    index++;
                    if (index >= slides.Count)
                        index = 0;

                    await FadeOutBoxes();

                    if (previousLayout != slides[index].Layout)
                        ChangeLayout(slides[index].Layout);

                    tb_SlideTitle.Text = slides[index].Title;
                    tb_SlideContent.Text = slides[index].Content;
                    img_SlideImage.Source = slides[index].Image;

                    await FadeInBoxes();

                    await Task.Delay(slides[index].Duration, ct);

                    previousLayout = slides[index].Layout;
                }
            }
            catch (TaskCanceledException)
            {
                // interrotto alla chiusura
            }
        }

        public void ChangeLayout(int layout)
        {
            if (layout == 1)
            {
                brd_ImgBox1.Visibility = Visibility.Hidden;
                img_SlideImage.Visibility = Visibility.Hidden;
                grd_TitleAndContent.SetValue(Grid.ColumnProperty, 1);
                grd_TitleAndContent.SetValue(Grid.ColumnSpanProperty, 3);
            }
            else
            {
                brd_ImgBox1.Visibility = Visibility.Visible;
                img_SlideImage.Visibility = Visibility.Visible;
                grd_TitleAndContent.SetValue(Grid.ColumnProperty, 3);
                grd_TitleAndContent.SetValue(Grid.ColumnSpanProperty, 1);
            }
        }

        public async Task FadeOutBoxes()
        {
            if (settings.SlideAnimationsEnabled)
            {
                while (brd_ContentBox.Opacity > 0)
                {
                    brd_ContentBox.Opacity = brd_ImgBox1.Opacity = brd_TitleBox.Opacity = tb_SlideContent.Opacity = tb_SlideTitle.Opacity = img_SlideImage.Opacity = brd_ContentBox.Opacity - 0.01;
                    slideBoxesShadowEffect.Opacity = slideBoxesShadowEffect.Opacity - 0.01;
                    await Task.Delay(1);
                }
                await Task.Delay(300);
            }
            else
            {
                brd_ContentBox.Opacity = brd_ImgBox1.Opacity = brd_TitleBox.Opacity = tb_SlideContent.Opacity = tb_SlideTitle.Opacity = img_SlideImage.Opacity = 0;
                slideBoxesShadowEffect.Opacity = 0;
                await Task.Delay(800);
            }


        }

        public async Task FadeInBoxes()
        {
            if (settings.SlideAnimationsEnabled)
            {
                while (brd_ContentBox.Opacity < 0.75)
                {
                    brd_ContentBox.Opacity = brd_ImgBox1.Opacity = brd_TitleBox.Opacity = tb_SlideContent.Opacity = tb_SlideTitle.Opacity = img_SlideImage.Opacity = brd_ContentBox.Opacity + 0.01;
                    slideBoxesShadowEffect.Opacity = slideBoxesShadowEffect.Opacity + 0.01;
                    await Task.Delay(1);
                }
                while (tb_SlideTitle.Opacity < 1)
                {
                    tb_SlideTitle.Opacity = tb_SlideContent.Opacity = img_SlideImage.Opacity = brd_ImgBox1.Opacity = tb_SlideContent.Opacity + 0.01;
                    await Task.Delay(1);
                }
            }
            else
            {
                brd_ContentBox.Opacity = brd_ImgBox1.Opacity = brd_TitleBox.Opacity = tb_SlideContent.Opacity = tb_SlideTitle.Opacity = img_SlideImage.Opacity = 0.75;
                slideBoxesShadowEffect.Opacity = 0.75;
                tb_SlideTitle.Opacity = tb_SlideContent.Opacity = img_SlideImage.Opacity = brd_ImgBox1.Opacity = 1;
            }


        }

        public async void SlideThread(CancellationToken cancellationToken)
        {
            int index = -1;
            int previousLayout = slides.Last<Slide>().Layout;
            ChangeLayout(slides.First<Slide>().Layout);

            while (true)
            {
                index++;
                if (index >= slides.Count) { index = 0; }

                await FadeOutBoxes();

                if (previousLayout != slides[index].Layout) ChangeLayout(slides[index].Layout);

                tb_SlideTitle.Text = slides[index].Title;
                tb_SlideContent.Text = slides[index].Content;
                img_SlideImage.Source = slides[index].Image;

                await FadeInBoxes();

                await Task.Delay(slides[index].Duration, cancellationToken);

                previousLayout = slides[index].Layout;
            }
        }

        private void btn_ClosePreview_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private static FontFamily ResolveFontFamily(string fontName)
        {
            const string fallback = "Segoe UI";

            if (string.IsNullOrWhiteSpace(fontName))
                return new FontFamily(fallback);

            var match = Fonts.SystemFontFamilies
                             .FirstOrDefault(f =>
                                 string.Equals(f.Source, fontName, System.StringComparison.OrdinalIgnoreCase));

            if (match != null)
                return match;

            return new FontFamily(fallback);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
