using Avalonia.Threading;
using PatchClient.Models;
using PatcherUtils;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;

namespace PatchClient.ViewModels
{
    public class PatcherViewModel : ViewModelBase
    {
        private bool initLineItemProgress = true;

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


        public PatcherViewModel(IScreen Host) : base(Host)
        {
            this.WhenActivated((CompositeDisposable disposables) =>
            {
                //check if escapefromtarkov.exe is present
                if(!File.Exists(Path.Join(Directory.GetCurrentDirectory(), "escapefromtarkov.exe")))
                {
                    NavigateTo(new MessageViewModel(HostScreen, "EscapeFromTarkov.exe was not found. Please ensure you have copied the patcher to your SPT folder."));
                    return;
                }

                //check if patch folder is present
                if(!Directory.Exists(LazyOperations.PatchFolder))
                {
                    NavigateTo(new MessageViewModel(HostScreen, $"{LazyOperations.PatchFolder} folder is missing. Please copy it to\n'{Environment.CurrentDirectory}'\nand try patching again."));
                    return;
                }

                RunPatcher();
            });
        }

        private void RunPatcher()
        {
            Task.Run(async() =>
            {
                LazyOperations.ExtractResourcesToTempDir(Assembly.GetExecutingAssembly());

                PatchHelper patcher = new PatchHelper(Environment.CurrentDirectory, null, LazyOperations.PatchFolder);

                patcher.ProgressChanged += patcher_ProgressChanged;

                string message = patcher.ApplyPatches();

                LazyOperations.CleanupTempDir();

                Directory.Delete(LazyOperations.PatchFolder, true);

                await NavigateToWithDelay(new MessageViewModel(HostScreen, message), 400);
            });
        }

        private void patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (LineItem item in AdditionalLineItems)
                {

                    if (initLineItemProgress)
                    {
                        LineItems.Add(new LineItemProgress(item));
                    }

                    LineItems.FirstOrDefault(x => x.Info == item.ItemText).UpdateProgress(item.ItemValue);
                }

                initLineItemProgress = false;

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
