using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebP.Net;

namespace WPFFIleConversion
{
    class ImageHandler
    {
        public bool UseLossy {  get; set; }
        public int LossyImageQuality { get; set; }
        public List<FileInfo> InputFiles { get; set; } = [];
        public string OutFolderPath { get; set; } = string.Empty;
        public EventHandler<(float percentage, int taskscompleted)> OnConvertingFile { get; set; } 

        public void ConvertFiles()
        {           
            int taskComp = 0;

            Task[] tasks = new Task[InputFiles.Count];
            for (int i = 0; i < InputFiles.Count; i++)
            {
                FileInfo file = InputFiles[i];

                tasks[i] = Task.Run(() =>
                {
                    string newName = file.Name.Replace(file.Extension, ".webp");
                    ConvertToWebPA(file.FullName, $"{this.OutFolderPath}//{newName}");
                    taskComp++;
                    float n = (float)taskComp / (float)tasks.Length;
                    this.OnConvertingFile?.Invoke(this, (n, taskComp));
                    
                });
            }
            Task.WhenAll(tasks);
        }

        private void ConvertToWebPA(string inputPath, string outputPath)
        {
            using (var image = System.Drawing.Image.FromFile(inputPath))
            {
                if (UseLossy)
                {
                    var webPData = EncodeLossy(new Bitmap(image), this.LossyImageQuality);
                    File.WriteAllBytes(outputPath, webPData);
                }
                else
                {
                    var webPData = EncodeLossless(new Bitmap(image));
                    File.WriteAllBytes(outputPath, webPData);
                }
            }
        }

        public Stream ConvertToWebPA(string inputPath)
        {
            using (var image = System.Drawing.Image.FromFile(inputPath))
            {
                var webPData = EncodeLossy(new Bitmap(image), this.LossyImageQuality);
                return new MemoryStream(webPData);                
            }
        }

        static byte[] EncodeLossy(Bitmap bitmap, float quality)
        {
            using var webp = new WebPObject(bitmap);
            return webp.GetWebPLossy(quality);
        }
        static byte[] EncodeLossless(Bitmap bitmap)
        {
            using var webp = new WebPObject(bitmap);
            return webp.GetWebPLossless();
        }
        static System.Drawing.Image DecodeWebP(byte[] webP)
        {
            using var webp = new WebPObject(webP);
            return webp.GetImage();
        }
    }
}
