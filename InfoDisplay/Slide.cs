using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace InfoDisplay
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
            List<Slide> ls = new List<Slide>();
            if (File.Exists("Slides.json")) ls = JsonSerializer.Deserialize<List<Slide>>(File.ReadAllText("Slides.json")).OrderBy(o => o.Order).ToList();
            return ls;
        }
        [JsonIgnore]
        public BitmapImage Image
        {
            get
            {
                BitmapImage bi = new BitmapImage();
                if (ImageBase64 != "")
                {

                    byte[] binaryData = Convert.FromBase64String(ImageBase64);
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(binaryData);
                    bi.EndInit();

                }
                return bi;

            }
            set
            {

            }
        }
        public void SetImageBase64FromBitmapImage(BitmapImage bi)
        {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bi));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            ImageBase64 = Convert.ToBase64String(data);
            Image = bi;
        }
        public bool PurgeExpired()
        {
            List<Slide> ls = new List<Slide>();
            bool somethingWasPurged = false;
            if (File.Exists("Slides.json")) ls = JsonSerializer.Deserialize<List<Slide>>(File.ReadAllText("Slides.json")).OrderBy(o => o.Order).ToList();
            foreach (Slide slide in ls.ToList())
                {
                if (slide.ExpirationDateEnabled && slide.ExpirationDate <= DateTime.Today)
                    {
                        ls.Remove(slide);
                        somethingWasPurged = true;
                    }
                }
            string jsonString = JsonSerializer.Serialize(ls);
            File.WriteAllText("Slides.json", jsonString);
            return somethingWasPurged;
        }
        public Slide GenerateDefaultSlide(int order)
        {
            return new Slide("Nuova Slide", "", order);
        }
    }

}
