using Avalonia.Threading;
using PatchClient.Models;
using PatcherUtils;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;

namespace PatchClient.ViewModels
{
    public class PatcherViewModel : ViewModelBase
    {
        private bool _initLineItemProgress = true;
        private bool _autoClose = false;
        private bool _debugOutput = false;
        private Stopwatch _patchStopwatch;
        private Timer _udpatePatchElapsedTimer = new Timer(1000);

        public ObservableCollection<LineItemProgress> LineItems { get; set; } = new ObservableCollection<LineItemProgress>();

        private string _ProgressMessage = "";
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

        private string _PatchMessage = "";
        public string PatchMessage
        {
            get => _PatchMessage;
            set => this.RaiseAndSetIfChanged(ref _PatchMessage, value);
        }

        private string _ElapsedPatchTimeDetails;
        public string ElapsedPatchTimeDetails
        {
            get => _ElapsedPatchTimeDetails;
            set => this.RaiseAndSetIfChanged(ref _ElapsedPatchTimeDetails, value);
        }


        public PatcherViewModel(IScreen Host, bool autoClose, bool debugOutput) : base(Host)
        {
            _autoClose = autoClose;
            _debugOutput = debugOutput;
            ElapsedPatchTimeDetails = "Starting ...";
            _udpatePatchElapsedTimer.Elapsed += _udpatePatchElapsedTimer_Elapsed;

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                //check if escapefromtarkov.exe is present
                if(!File.Exists(Path.Join(Directory.GetCurrentDirectory(), "escapefromtarkov.exe")))
                {
                    if (_autoClose)
                    {
                        Environment.Exit((int)PatcherExitCode.EftExeNotFound);
                        return;
                    }

                    NavigateTo(new MessageViewModel(HostScreen, "EscapeFromTarkov.exe was not found. Please ensure you have copied the patcher to your SPT folder."));
                    return;
                }

                //check if patch folder is present
                if(!Directory.Exists(LazyOperations.PatchFolder))
                {
                    if (_autoClose)
                    {
                        Environment.Exit((int)PatcherExitCode.NoPatchFolder);
                        return;
                    }

                    NavigateTo(new MessageViewModel(HostScreen, $"{LazyOperations.PatchFolder} folder is missing. Please copy it to\n'{Environment.CurrentDirectory}'\nand try patching again."));
                    return;
                }

                RunPatcher();
            });
        }

        private void _udpatePatchElapsedTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ElapsedPatchTimeDetails = $"Elapsed Patch Time: {_patchStopwatch.Elapsed.ToString("hh':'mm':'ss")}";
            });
        }

        private void RunPatcher()
        {
            Task.Run(async() =>
            {
                LazyOperations.ExtractResourcesToTempDir(Assembly.GetExecutingAssembly());

                PatchHelper patcher = new PatchHelper(Environment.CurrentDirectory, null, LazyOperations.PatchFolder, _debugOutput);

                patcher.ProgressChanged += patcher_ProgressChanged;

                _udpatePatchElapsedTimer.Start();
                _patchStopwatch = Stopwatch.StartNew();

                var patchMessage = patcher.ApplyPatches();

                _patchStopwatch.Stop();
                _udpatePatchElapsedTimer.Stop();

                LazyOperations.CleanupTempDir();

                Directory.Delete(LazyOperations.PatchFolder, true);

                if(_autoClose)
                {
                    Environment.Exit((int)patchMessage.ExitCode);
                }

                await NavigateToWithDelay(new MessageViewModel(HostScreen, patchMessage.Message), 400);
            });
        }

        private void patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (LineItem item in AdditionalLineItems)
                {

                    if (_initLineItemProgress)
                    {
                        LineItems.Add(new LineItemProgress(item));
                    }

                    LineItems.FirstOrDefault(x => x.Info == item.ItemText).UpdateProgress(item.ItemValue);
                }

                _initLineItemProgress = false;

                PatchPercent = Percent;

                if (!string.IsNullOrWhiteSpace(Message))
                {
                    PatchMessage = Message;
                }

                ProgressMessage = $"Patching: {Progress} / {Total} - {Percent}%";
            });
        }
    }
}
