using PatcherUtils;
using ReactiveUI;
using System;

namespace PatchClient.Models
{
    public class LineItemProgress : ReactiveObject
    {
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

        public void UpdateProgress(int Count)
        {
            Progress = (int)Math.Floor((double)Count / total * 100);
        }

        public LineItemProgress(LineItem Item)
        {
            Info = Item.ItemText;

            total = Item.ItemValue;

            Progress = (int)Math.Floor((double)Item.ItemValue / total * 100);
        }
    }
}
