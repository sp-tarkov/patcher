using PatchGenerator.Models;
using ReactiveUI;

namespace PatchGenerator.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        public PatchGenInfo GenerationInfo { get; set; } = new PatchGenInfo();

        public OptionsViewModel(IScreen Host) : base(Host)
        {
            GenerationInfo.SourceFolderPath = "Drop SOURCE folder here";
            GenerationInfo.TargetFolderPath = "Drop TARGET folder here";
        }

        public void GeneratePatches()
        {
            NavigateTo(new PatchGenerationViewModel(HostScreen, GenerationInfo));
        }
    }
}
