using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PatcherUtils;
using PatchGenerator.Extensions;

namespace PatchGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string compareFolder = "";
        private string targetFolder = "";
        private string outputFolderName = "";

        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetStopWatchTime()
        {
            return $"Hours: {stopwatch.Elapsed.Hours} - Mins: {stopwatch.Elapsed.Minutes} - Secs: {stopwatch.Elapsed.Seconds} - MilliSecs: {stopwatch.Elapsed.Milliseconds}";
        }

        private static bool FileDropCheck(DragEventArgs args, ref string str)
        {
            if (!args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return false;
            }

            string[] paths = (string[])args.Data.GetData(DataFormats.FileDrop);

            if (paths.Length != 1) return false;

            if (!Directory.Exists(paths[0]))
            {
                return false;
            }

            str = paths[0];

            return true;
        }

        private void CompareLabel_Drop(object sender, DragEventArgs e)
        {
            if (FileDropCheck(e, ref compareFolder))
            { 
                CompareLabel.Content = $"Compare Folder:\n{compareFolder}";
                CompareLabel.BorderBrush = Brushes.DarkCyan;
            }
            else
            {
                MessageBox.Show("Dropped File/s could not be used. Make sure you only drop one folder.");
            }
        }

        private void TargetLabel_Drop(object sender, DragEventArgs e)
        {
            if(FileDropCheck(e, ref targetFolder))
            {
                TargetLabel.Content = $"Target Folder:\n{targetFolder}";
                TargetLabel.BorderBrush = Brushes.DarkCyan;
            }
            else 
            {
                MessageBox.Show("Dropped File/s could not be used. Make sure you only drop one folder.");
            }
        }

        private void GeneratePatches(string patchBase)
        {
            //create temp data
            GenProgressBar.DispatcherSetIndetermination(true);
            GenProgressMessageLabel.DispaatcherSetContent("Extracting temp data ...");

            LazyOperations.CleanupTempDir();
            LazyOperations.PrepTempDir();

            GenProgressBar.DispatcherSetIndetermination(false);

            //generate patches
            FileCompare bc = new FileCompare(targetFolder, compareFolder, patchBase);

            bc.ProgressChanged += Bc_ProgressChanged;

            if (!bc.CompareAll())
            {
                MessageBox.Show("Failed to generate diffs.", ":(", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //Copy patch client to output folder
            File.Copy(LazyOperations.PatcherClientPath, $"{outputFolderName}\\patcher.exe", true);

            //compress patch output folder to 7z file
            LazyOperations.StartZipProcess(outputFolderName, $"{outputFolderName}.7z".FromCwd());

            GenProgressBar.DispatcherSetValue(100);
            GenProgressMessageLabel.DispaatcherSetContent("Done");
        }

        private void Bc_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            string additionalInfo = "";
            foreach (LineItem item in AdditionalLineItems)
            {
                additionalInfo += $"{item.ItemText}: {item.ItemValue}\n";
            }


            GenProgressBar.DispatcherSetValue(Percent);

            if (!string.IsNullOrWhiteSpace(Message))
            {
                GenProgressMessageLabel.DispaatcherSetContent(Message);
            }

            GenProgressInfoLabel.DispaatcherSetContent($"[{Progress}/{Total}]");

            AdditionalInfoBlock.DispatcherSetText(additionalInfo);
        }

        private void GenButton_Click(object sender, RoutedEventArgs e)
        {
            GenButton.IsEnabled = false;
            CompareLabel.IsEnabled = false;
            TargetLabel.IsEnabled = false;
            FileNameBox.IsEnabled = false;

            string InfoNeededMessage = "You must set the following: ";
            bool infoNeeded = false;

            if(string.IsNullOrWhiteSpace(FileNameBox.Text))
            {
                InfoNeededMessage += "\n[Output File Name]";
                FileNameBox.BorderBrush = Brushes.Red;
                infoNeeded = true;
            }

            if(string.IsNullOrWhiteSpace(compareFolder))
            {
                InfoNeededMessage += "\n[COMPARE Folder]";
                CompareLabel.BorderBrush = Brushes.Red;
                infoNeeded = true;
            }

            if(string.IsNullOrWhiteSpace(targetFolder))
            {
                InfoNeededMessage += "\n[TARGET Folder]";
                TargetLabel.BorderBrush = Brushes.Red;
                infoNeeded = true;
            }

            if (infoNeeded)
            {
                MessageBox.Show(InfoNeededMessage, "Info Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                GenButton.IsEnabled = true;
                CompareLabel.IsEnabled = true;
                TargetLabel.IsEnabled = true;
                FileNameBox.IsEnabled = true;
                return;
            }

            void SetEndingInfo(string info)
            {
                GenButton.DispatcherSetEnabled(true);
                CompareLabel.DispatcherSetEnabled(true);
                TargetLabel.DispatcherSetEnabled(true);
                FileNameBox.DispatcherSetEnabled(true);

                GenProgressMessageLabel.DispaatcherSetContent("");
                GenProgressInfoLabel.DispaatcherSetContent(info);
            }


            Task.Run(() =>
            {
                stopwatch.Reset();
                stopwatch.Start();

                try
                {
                    GeneratePatches(Path.Combine(outputFolderName.FromCwd(), LazyOperations.PatchFolder));
                    stopwatch.Stop();
                    SetEndingInfo($"Patches Generated in: {GetStopWatchTime()}");
                }
                catch(Exception ex)
                {
                    stopwatch.Stop();
                    SetEndingInfo(ex.Message);
                }
            });
        }

        private void FileNameBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FileNameBox.BorderBrush = Brushes.Gainsboro;

            if (outputFolderName == FileNameBox.Text) return;

            outputFolderName = Regex.Replace(FileNameBox.Text, "[^A-Za-z0-9.\\-_]", "");

            FileNameBox.Text = outputFolderName;
        }
    }
}
