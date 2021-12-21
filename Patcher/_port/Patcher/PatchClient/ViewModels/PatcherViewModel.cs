using Avalonia;
using PatchClient.Models;
using PatcherUtils;
using Splat;
using System;
using ReactiveUI;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace PatchClient.ViewModels
{
    public class PatcherViewModel : ViewModelBase
    {
        private bool initLineItemProgress = true;

        public ObservableCollection<LineItemProgress> LineItems { get; set; } = new ObservableCollection<LineItemProgress>();

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

        private string _PatchMessage;
        public string PatchMessage
        {
            get => _PatchMessage;
            set => this.RaiseAndSetIfChanged(ref _PatchMessage, value);
        }

        private ViewNavigator navigator => Locator.Current.GetService<ViewNavigator>();

        public PatcherViewModel()
        {
            RunPatcher();
        }

        private void RunPatcher()
        {
            Task.Run(() =>
            {
                PatchHelper patcher = new PatchHelper(Environment.CurrentDirectory, null, LazyOperations.PatchFolder);

                patcher.ProgressChanged += patcher_ProgressChanged;

                string message = patcher.ApplyPatches();

                navigator.SelectedViewModel = new MessageViewModel(message).WithDelay(400);
            });
        }

        private void patcher_ProgressChanged(object Sender, int Progress, int Total, int Percent, string Message = "", params LineItem[] AdditionalLineItems)
        {
            foreach (LineItem item in AdditionalLineItems)
            {

                if(initLineItemProgress)
                {
                    if (item.ItemValue <= 0) continue;

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
        }
    }
}
