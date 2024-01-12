using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace WPFFIleConversion
{
    /// <summary>
    /// Interaction logic for ImagePreview.xaml
    /// </summary>
    public partial class ImagePreview : Page
    {
        FileInfo previewImage;
        public IEnumerable<object> Qualities => Utility.GetQualityNum();
        ImageHandler imageHandler = new ImageHandler();
        Stream stream;

        public ImagePreview()
        {
            InitializeComponent();
            imageHandler.OutFolderPath = Path.GetTempPath();
            imageHandler.InputFiles = new List<FileInfo>() { previewImage! };
            imageHandler.UseLossy = true;
            imageHandler.LossyImageQuality = Convert.ToInt32(QualitySlider.Value);

            OGImage.SourceUpdated += OGImage_SourceUpdated;
            OGImage.SizeChanged += OGImage_SizeChanged;
            //double[] qualitiesDoubles = new double[Qualities.Count()];
            //for (int i = 0; i < qualitiesDoubles.Length; i++)
            //{
            //    qualitiesDoubles[i] = Convert.ToDouble(Qualities.ElementAt(i));
            //}

            //QualitySlider.Minimum = qualitiesDoubles.Min();
            //QualitySlider.Maximum = qualitiesDoubles.Max();
            //QualitySlider.Ticks = new DoubleCollection(qualitiesDoubles);
        }

        private void OGImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {            
            PrevImage.Width = OGImage.ActualWidth;
            PrevImage.Height = OGImage.ActualHeight;
        }

        private void OGImage_SourceUpdated(object? sender, DataTransferEventArgs e)
        {
            PrevImage.Width = OGImage.ActualWidth;
            PrevImage.Height = OGImage.ActualHeight;
        }

        private void PrevInButton_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = false;
                openFileDialog.FileName = "File Selection";
                openFileDialog.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
                var dr = openFileDialog.ShowDialog();

                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    previewImage = new FileInfo(openFileDialog.FileName);
                    PrevFilePathDisplay.Text = previewImage.FullName;

                    BitmapImage myBitmapImage = new BitmapImage();

                    // BitmapImage.UriSource must be in a BeginInit/EndInit block
                    myBitmapImage.BeginInit();
                    myBitmapImage.UriSource = new Uri(previewImage.FullName);
                    myBitmapImage.EndInit();
                    OGImage.Source = myBitmapImage;

                    imageHandler.InputFiles = new List<FileInfo>() { previewImage! };
                    ConvertButton.IsEnabled = true;
                    OGText.Text = $"Original | {ConvertToKB(previewImage.Length)} KB";
                }
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            stream = imageHandler.ConvertToWebPA(previewImage.FullName);

            BitmapImage myBitmapImage = new BitmapImage();

            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = stream;
            myBitmapImage.EndInit();

            PrevImage.Source = myBitmapImage;

            ConvertButton.IsEnabled = true;
            PrevText.Text = $"Preview | {ConvertToKB(stream.Length)} KB (-{GetPercentageDecrease(previewImage.Length, stream.Length)}%)";
            SaveButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFolderDialog dlg = new();

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                BitmapImage bitmap = (BitmapImage)PrevImage.Source;

                using (Stream stream = bitmap.StreamSource)
                {                    
                    stream.Position = 0;
                    string newName = previewImage.Name.Replace(previewImage.Extension, ".webp");

                    using (FileStream destination = File.Create(dlg.FolderName + $"//{newName}"))
                    {
                        stream.CopyTo(destination);
                    }
                }
            }
        }

        private double ConvertToKB(double value)
        {
            return Math.Round(value / 1024, 3);
        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            imageHandler.LossyImageQuality = Convert.ToInt32(QualitySlider.Value);
        }

        private double GetPercentageDecrease(double original, double after)
        {
            return Math.Round((1 - after / original) * 100);
        }
    }
}
