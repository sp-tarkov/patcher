using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using PatchClient.Models;
using PatcherUtils;
using PatchGenerator.Helpers;
using PatchGenerator.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using PatcherUtils.Model;
using Timer = System.Timers.Timer;

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

        private bool _IndeterminateProgress = false;
        public bool IndeterminateProgress
        {
            get => _IndeterminateProgress;
            set => this.RaiseAndSetIfChanged(ref _IndeterminateProgress, value);
        }

        private string _ElapsedTimeDetails;
        public string ElapsedTimeDetails
        {
            get => _ElapsedTimeDetails;
            set => this.RaiseAndSetIfChanged(ref _ElapsedTimeDetails, value);
        }

        private LineItem[] lineItems;

        public ObservableCollection<PatchItem> PatchItemCollection { get; set; } = new ObservableCollection<PatchItem>();
        public ObservableCollection<PatchItem> PatchItemLegendCollection { get; set; } = new ObservableCollection<PatchItem>();

        private Stopwatch patchGenStopwatch = new Stopwatch();
        private Timer updateElapsedTimeTimer = new Timer(1000);

        private readonly PatchGenInfo generationInfo;
        public PatchGenerationViewModel(IScreen Host, PatchGenInfo GenerationInfo) : base(Host)
        {
            generationInfo = GenerationInfo;
            ElapsedTimeDetails = "Starting ...";

            updateElapsedTimeTimer.Elapsed += UpdateElapsedTimeTimer_Elapsed;

            foreach (KeyValuePair<string, IBrush> pair in PatchItemDefinitions.Colors)
            {
                PatchItemLegendCollection.Add(new PatchItem("")
                {
                    Name = pair.Key,
                    Color = pair.Value,
                });
            }

            this.WhenActivated((CompositeDisposable dissposables) =>
            {
                GeneratePatches();
            });
        }

        private void UpdateElapsedTimeTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ElapsedTimeDetails = $"Elapsed Patch Time: {patchGenStopwatch.Elapsed.ToString("hh':'mm':'ss")}";
            });
        }

        public void GeneratePatches()
        {
            Task.Run(() =>
            {
                string patchOutputFolder = Path.Join(generationInfo.PatchName.FromCwd(), LazyOperations.PatchFolder);

                LazyOperations.ExtractResourcesToTempDir(Assembly.GetExecutingAssembly());

                PatchHelper patcher = new PatchHelper(generationInfo.SourceFolderPath, generationInfo.TargetFolderPath, patchOutputFolder);

                patcher.ProgressChanged += Patcher_ProgressChanged;

                updateElapsedTimeTimer.Start();
                patchGenStopwatch.Start();

                var message = patcher.GeneratePatches();
                
                patchGenStopwatch.Stop();
                updateElapsedTimeTimer.Stop();

                if(message.ExitCode != PatcherExitCode.Success && generationInfo.AutoClose)
                {
                    PatchLogger.LogInfo("Exiting: Auto close on failure");
                    Environment.Exit((int)message.ExitCode);
                }

                PatchLogger.LogInfo("Printing summary info ...");
                PrintSummary();

                StringBuilder sb = new StringBuilder()
                .Append("Patches Generated in ")
                .Append($"{patchGenStopwatch.Elapsed.Hours} hr/s ")
                .Append($"{patchGenStopwatch.Elapsed.Minutes} min/s ")
                .Append($"{patchGenStopwatch.Elapsed.Seconds} sec/s");

                ProgressMessage = sb.ToString();

                File.Copy(LazyOperations.PatcherClientPath, $"{generationInfo.PatchName.FromCwd()}\\patcher.exe", true);
                
                PatchLogger.LogInfo("Copied patcher.exe to output folder");
                
                // if (generationInfo.AutoZip)
                // {
                //     PatchLogger.LogInfo("AutoZipping");
                //     IndeterminateProgress = true;
                //
                //     PatchItemCollection.Add(new PatchItem("Allowing Time for files to unlock ..."));
                //
                //     Thread.Sleep(2000);
                //
                //     PatchItemCollection.Add(new PatchItem("Zipping patcher ..."));
                //
                //     ProgressMessage = "Zipping patcher";
                //     
                //     IndeterminateProgress = false;
                //
                //     var progress = new Progress<int>(p =>
                //     {
                //         PatchPercent = p;
                //     });
                //
                //     LazyOperations.CompressDirectory(generationInfo.PatchName.FromCwd(), $"{generationInfo.PatchName}.7z".FromCwd(), progress);
                //
                //     PatchItemCollection.Add(new PatchItem("Done"));
                // }

                if (generationInfo.AutoClose)
                {
                    Environment.Exit((int)PatcherExitCode.Success);
                }
            });
        }

        private void Patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressMessage = $"{Progress}/{Total}";

                PatchPercent = Percent;

                PatchItemCollection.Add(new PatchItem(Message));

                lineItems = AdditionalLineItems;
            });
        }

        private void PrintSummary()
        {
            Dispatcher.UIThread?.InvokeAsync(() =>
            {
                if (Application.Current.Resources.TryGetResource("AKI_Brush_Yellow", out var color))
                {
                    if (color is IBrush brush)
                    {
                        PatchItemCollection.Add(new PatchItem("")
                        {
                            Name = new StringBuilder().AppendLine("Summary").AppendLine("----------").ToString(),
                            Color = brush
                        });

                        foreach (LineItem item in lineItems)
                        {
                            PatchItemCollection.Add(new PatchItem("")
                            {
                                Name = $"{item.ItemText}: {item.ItemValue}",
                                Color = brush
                            });
                        }
                    }
                }
            });
        }
    }

}
