using Avalonia.Media;
using PatchGenerator.Helpers;
using ReactiveUI;
using System.Linq;

namespace PatchGenerator.Models
{
    public class PatchItem : ReactiveObject
    {
        private string _Name = "";
        public string Name
        {
            get => _Name;
            set => this.RaiseAndSetIfChanged(ref _Name, value);
        }

        private IBrush _Color;
        public IBrush Color
        {
            get => _Color;
            set => this.RaiseAndSetIfChanged(ref _Color, value);
        }

        public PatchItem(string Name)
        {
            this.Name = Name.Replace(".new", "").Replace(".delta", "").Replace(".del", "");

            IBrush color;

            if (PatchItemDefinitions.Colors.TryGetValue(Name.Split('.').Last(), out color))
            {
                Color = color;
            }
            else
            {
                Color = PatchItemDefinitions.Colors["exists"];
            }
        }
    }
}
