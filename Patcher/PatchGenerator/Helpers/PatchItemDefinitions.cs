using Avalonia.Media;
using System.Collections.Generic;

namespace PatchGenerator.Helpers
{
    public static class PatchItemDefinitions
    {
        public static Dictionary<string, IBrush> Colors = new Dictionary<string, IBrush>()
        {
            {"delta", Brushes.MediumPurple },
            {"new", Brushes.Green },
            {"del", Brushes.IndianRed },
            {"exists", Brushes.DimGray }
        };
    }
}
