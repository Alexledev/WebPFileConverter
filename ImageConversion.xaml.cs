using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using WebP.Net;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Concurrent;
using System.Windows.Controls.Primitives;

namespace WPFFIleConversion
{
    /// <summary>
    /// Interaction logic for ImageConversion.xaml
    /// </summary>
    public partial class ImageConversion : Page
    {
        readonly string[] imageFileExtensions = [".png", ".jpg"];
        string inFolderPath = string.Empty;

        public IEnumerable<object> Qualities => Utility.GetQualityNum();

        private readonly ImageHandler imageHandler = new();

        public ImageConversion()
        {
            InitializeComponent();

            EnableConvertButton(false);
            QualitySelectBox.ItemsSource = Qualities;
            QualitySelectBox.SelectedIndex = 3;

            IProgress<(float percentage, int tasksDone)> progress = new Progress<(float percentage, int tasksDone)>(value =>
            {
                ProgBar.Value = value.percentage;
                ProgText.Text = $"{value.tasksDone}/{imageHandler.InputFiles.Count}";

                if (value.percentage == 1)
                {
                    ConvertButton.IsEnabled = true;
                    InButton.IsEnabled = true;
                    OutButton.IsEnabled = true;
                }
                else
                {
                    ConvertButton.IsEnabled = false;
                    InButton.IsEnabled = false;
                    OutButton.IsEnabled = false;
                }
            });

            imageHandler.OnConvertingFile += (sender, val) =>
            {
                progress.Report(val);
            };
        }

        private void EnableConvertButton(bool enable)
        {
            ConvertButton.IsEnabled = enable;
        }

        private void InButton_Click(object sender, RoutedEventArgs e)
        {
            EnableConvertButton(false);

            imageHandler.InputFiles.Clear();
            InPathDisplay.Text = "";
            InListBox.Items.Clear();

            if (UseFileButton.IsChecked == true)
            {
                OpenFileInput();
            }
            else
            {
                OpenFolderInput();
            }

            if (imageHandler.InputFiles.Count > 0 && !string.IsNullOrWhiteSpace(imageHandler.OutFolderPath))
            {
                EnableConvertButton(true);
            }
        }

        private void OpenFileInput()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.FileName = "File Selection";
                openFileDialog.Filter = "Image files (*.png, *.jpg)|*.png;*.jpg";
                var dr = openFileDialog.ShowDialog();

                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (string fileName in openFileDialog.FileNames.AsEnumerable())
                    {
                        imageHandler.InputFiles.Add(new FileInfo(fileName));
                        AddToDisplayList(Path.GetFileName(fileName));
                    }

                    string directoryPath = Path.GetDirectoryName(openFileDialog.FileName)!;
                    InPathDisplay.Text = directoryPath;
                }
            }
        }

        private void OpenFolderInput()
        {
            Microsoft.Win32.OpenFolderDialog dlg = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                string folderName = dlg.FolderName;
                InPathDisplay.Text = folderName;

                DirectoryInfo dirInf = new DirectoryInfo(folderName);
                IEnumerable<FileInfo> files = dirInf.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where((fileInf) => imageFileExtensions.Contains(Path.GetExtension(fileInf.FullName), StringComparer.OrdinalIgnoreCase));

                foreach (var fileInf in files)
                {
                    AddToDisplayList(fileInf.Name);
                }

                imageHandler.InputFiles.AddRange(files);
            }
        }

        private void AddToDisplayList(string name)
        {
            InListBox.Items.Add(name);
        }

        private void OutButton_Click(object sender, RoutedEventArgs e)
        {
            EnableConvertButton(false);

            Microsoft.Win32.OpenFolderDialog dlg = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                // Open document 
                string folderName = dlg.FolderName;
                OutPathDisplay.Text = folderName;
                imageHandler.OutFolderPath = folderName;
            }

            if (imageHandler.InputFiles.Count > 0 && !string.IsNullOrWhiteSpace(imageHandler.OutFolderPath))
            {
                EnableConvertButton(true);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            ProgBar.Value = 0;
            imageHandler.ConvertFiles();
        }

        private void UseLosslessButton_Checked(object sender, RoutedEventArgs e)
        {
            imageHandler.UseLossy = false;

            if (QualitySelectBox != null)
                QualitySelectBox.IsEnabled = false;
        }

        private void UseLossyButton_Checked(object sender, RoutedEventArgs e)
        {
            imageHandler.UseLossy = true;

            if (QualitySelectBox != null)
                QualitySelectBox.IsEnabled = true;
        }

        private void QualitySelectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QualitySelectBox != null)
            {
                if (QualitySelectBox.SelectedItem != null)
                    imageHandler.LossyImageQuality = Int32.Parse((string)QualitySelectBox.SelectedItem);
            }
        }

    }
}
