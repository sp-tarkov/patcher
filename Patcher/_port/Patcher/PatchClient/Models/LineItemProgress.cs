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

        private int total = 0;

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

        public void UpdateProgress(int RemainingCount)
        {
            if (Completed) return; //this doesn't work right ... need to look at it.

            Progress = (int)Math.Floor((double)RemainingCount / total * 100);

            if (Progress == 100) Completed = true;
        }

        public LineItemProgress(LineItem Item)
        {
            Info = Item.ItemText;

            total = Item.ItemValue;

            Progress = (int)Math.Floor((double)Item.ItemValue / total * 100);
        }
    }
}
