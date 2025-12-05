using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace InfoDisplay.Core
{
    public class Slide
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public string ImageBase64 { get; set; }
        public int Duration { get; set; }
        public int Layout { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool ExpirationDateEnabled { get; set; }

        public Slide() { }

        public Slide(string title, string content, int order, string imageBase64 = "", int duration = 10000, int layout = 0, DateTime expirationDate = default, bool expirationDateEnabled = false)
        {
            Title = title;
            Content = content;
            Order = order;
            ImageBase64 = imageBase64;
            Duration = duration;
            Layout = layout;
            ExpirationDate = expirationDate;
            ExpirationDateEnabled = expirationDateEnabled;
        }

        public List<Slide> ReadSlides()
        {
            var ls = new List<Slide>();

            // No file -> no slide
            if (!File.Exists(AppPaths.SlidesJson))
                return ls;

            try
            {
                var json = File.ReadAllText(AppPaths.SlidesJson);
                var slides = JsonSerializer.Deserialize<List<Slide>>(json);

                if (slides == null)
                    return ls;

                return slides.OrderBy(o => o.Order).ToList();
            }
            catch
            {
                //Ignore if unreadable/corrupted
                return ls;
            }
        }

        [JsonIgnore]
        public BitmapImage Image
        {
            get
            {
                var bi = new BitmapImage();

                if (!string.IsNullOrEmpty(ImageBase64))
                {
                    try
                    {
                        byte[] binaryData = Convert.FromBase64String(ImageBase64);
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(binaryData);
                        bi.EndInit();
                    }
                    catch
                    {
                        // Se l'immagine è corrotta, torno un BitmapImage "vuoto" e basta
                    }
                }

                return bi;
            }
            set
            {
                // lasciato intenzionalmente vuoto, usi SetImageBase64FromBitmapImage
            }
        }

        public void SetImageBase64FromBitmapImage(BitmapImage bi)
        {
            byte[] data;
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bi));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            ImageBase64 = Convert.ToBase64String(data);
            Image = bi;
        }

        public bool PurgeExpired()
        {
            var ls = new List<Slide>();
            var somethingWasPurged = false;

            if (File.Exists(AppPaths.SlidesJson))
            {
                try
                {
                    var json = File.ReadAllText(AppPaths.SlidesJson);
                    var slides = JsonSerializer.Deserialize<List<Slide>>(json);

                    if (slides != null)
                        ls = slides.OrderBy(o => o.Order).ToList();
                }
                catch
                {
                    // If file is corrupter, don't even try purging:
                    // Configurator will overwrite on next save.
                    return false;
                }
            }

            foreach (var slide in ls.ToList())
            {
                if (slide.ExpirationDateEnabled && slide.ExpirationDate <= DateTime.Today)
                {
                    ls.Remove(slide);
                    somethingWasPurged = true;
                }
            }

            if (somethingWasPurged)
            {
                var jsonString = JsonSerializer.Serialize(ls, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(AppPaths.SlidesJson, jsonString);
            }

            return somethingWasPurged;
        }

        public Slide GenerateDefaultSlide(int order)
        {
            return new Slide("Nuova Slide", "", order);
        }
    }
}