using PatchGenerator.Models;
using Splat;

namespace PatchGenerator.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        public PatchGenInfo GenerationInfo { get; set; } = new PatchGenInfo();

        private ViewNavigator navigator => Locator.Current.GetService<ViewNavigator>();

        public OptionsViewModel()
        {
            GenerationInfo.SourceFolderPath = "Drop SOURCE folder here";
            GenerationInfo.TargetFolderPath = "Drop TARGET folder here";
        }

        public void GeneratePatches()
        {
            navigator.SelectedViewModel = new PatchGenerationViewModel(GenerationInfo);
        }
    }
}
