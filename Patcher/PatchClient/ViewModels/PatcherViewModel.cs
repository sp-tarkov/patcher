using Avalonia;
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
                //Test();
                RunPatcher();
            });
        }

        /// <summary>
        /// A dumb testing method to see if things look right. Obsolete is used more like a warning here.
        /// </summary>
        [Obsolete]
        private void Test()
        {
            Task.Run(async () =>
            {
                LineItem x = new LineItem("test 1", 30);
                LineItem xx = new LineItem("test 2", 100);
                LineItem xxx = new LineItem("test 3", 70);

                LineItems.Add(new LineItemProgress(x));
                LineItems.Add(new LineItemProgress(xx));
                LineItems.Add(new LineItemProgress(xxx));

                for (int i = 0; i <= 100; i++)
                {
                    System.Threading.Thread.Sleep(20);
                    PatchPercent = i;
                    ProgressMessage = $"Patching @ {i}%";

                    foreach (var item in LineItems)
                    {
                        item.UpdateProgress(item.Total - i);
                    }
                }

                await NavigateToWithDelay(new MessageViewModel(HostScreen, "Test Complete"), 400);
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
