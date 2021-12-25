using Avalonia.Media;
using PatcherUtils;
using PatchGenerator.Helpers;
using PatchGenerator.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PatchGenerator.ViewModels
{
    public class PatchGenerationViewModel : ViewModelBase
    {
        private bool _AutoScroll = true;
        public bool AutoScroll
        {
            get => _AutoScroll;
            set => this.RaiseAndSetIfChanged(ref _AutoScroll, value);
        }

        private string _ProgressMessage;
        public string ProgressMessage
        {
            get => _ProgressMessage;
            set => this.RaiseAndSetIfChanged(ref _ProgressMessage, value);
        }

        private int _PatchPercent;
        public int PatchPercent
        {
            get => _PatchPercent;
            set => this.RaiseAndSetIfChanged(ref _PatchPercent, value);
        }

        public ObservableCollection<PatchItem> PatchItemCollection { get; set; } = new ObservableCollection<PatchItem>();
        public ObservableCollection<PatchItem> PatchItemLegendCollection { get; set; } = new ObservableCollection<PatchItem>();

        private Stopwatch patchGenStopwatch = new Stopwatch();

        private readonly PatchGenInfo generationInfo;
        public PatchGenerationViewModel(PatchGenInfo GenerationInfo)
        {
            generationInfo = GenerationInfo;

            foreach (KeyValuePair<string, IBrush> pair in PatchItemDefinitions.Colors)
            {
                PatchItemLegendCollection.Add(new PatchItem("")
                {
                    Name = pair.Key,
                    Color = pair.Value,
                });
            }

            GeneratePatches();
        }

        public void GeneratePatches()
        {
            Task.Run(() =>
            {
                //Slight delay to avoid some weird race condition in avalonia core, seems to be a bug, but also maybe I'm just stupid, idk -waffle
                System.Threading.Thread.Sleep(200);

                string outputFolder = Path.Join(LazyOperations.PatchFolder, generationInfo.PatchName);

                PatchHelper patcher = new PatchHelper(generationInfo.SourceFolderPath, generationInfo.TargetFolderPath, outputFolder);

                patcher.ProgressChanged += Patcher_ProgressChanged;

                patchGenStopwatch.Start();

                patcher.GeneratePatches();

                patchGenStopwatch.Stop();

                StringBuilder sb = new StringBuilder()
                .Append("Patches Generated in ")
                .Append($"{patchGenStopwatch.Elapsed.Hours} hr/s ")
                .Append($"{patchGenStopwatch.Elapsed.Minutes} min/s ")
                .Append($"{patchGenStopwatch.Elapsed.Seconds} sec/s");

                ProgressMessage = sb.ToString();

                //TODO - need to fix this. Wrong folder, and need to copy client to output
                if (generationInfo.AutoZip)
                {
                    LazyOperations.StartZipProcess(outputFolder, $"{outputFolder}.zip");
                }
            });
        }

        private void Patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            ProgressMessage = $"{Progress}/{Total}";

            PatchPercent = Percent;

            PatchItemCollection.Add(new PatchItem(Message));
        }
    }

}
