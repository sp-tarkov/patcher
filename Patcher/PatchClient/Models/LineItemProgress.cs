using PatcherUtils;
using ReactiveUI;
using System;

namespace PatchClient.Models
{
    public class LineItemProgress : ReactiveObject
    {
        private bool _Completed = false;
        public bool Completed
        {
            get => _Completed;
            set => this.RaiseAndSetIfChanged(ref _Completed, value);
        }

        public int Total { get; private set; } = 0;

        private string _Info = "";
        public string Info
        {
            get => _Info;
            set => this.RaiseAndSetIfChanged(ref _Info, value);
        }

        private int _Progress;
        public int Progress
        {
            get => _Progress;
            set => this.RaiseAndSetIfChanged(ref _Progress, value);
        }

        private string _ProgressInfo = "";
        public string ProgressInfo
        {
            get => _ProgressInfo;
            set => this.RaiseAndSetIfChanged(ref _ProgressInfo, value);
        }

        public void UpdateProgress(int RemainingCount)
        {
            if (Completed) return;

            int processed = Total - RemainingCount;

            Progress = (int)Math.Floor((double)processed / Total * 100);

            ProgressInfo = $"{processed} / {Total}";

            if (Progress == 100) Completed = true;
        }

        public LineItemProgress(LineItem Item)
        {
            Info = Item.ItemText;

            Total = Item.ItemValue;

            Progress = (int)Math.Floor((double)Item.ItemValue / Total * 100);
        }
    }
}
