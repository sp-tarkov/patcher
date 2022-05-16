using ReactiveUI;
using System.IO;

namespace PatchGenerator.Models
{
    public class PatchGenInfo : ReactiveObject
    {
        private void UpdateReadyToRun()
        {
            if (Directory.Exists(SourceFolderPath) && Directory.Exists(TargetFolderPath) && PatchName != "")
            {
                ReadyToRun = true;
                return;
            }

            ReadyToRun = false;
        }

        private string _PatchName = "";
        public string PatchName
        {
            get => _PatchName;
            set
            {
                this.RaiseAndSetIfChanged(ref _PatchName, value);
                UpdateReadyToRun();
            }
        }

        private string _SourceFolderPath = "";
        public string SourceFolderPath
        {
            get => _SourceFolderPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _SourceFolderPath, value);
                UpdateReadyToRun();
            }
        }

        private string _TargetFolderPath = "";
        public string TargetFolderPath
        {
            get => _TargetFolderPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _TargetFolderPath, value);
                UpdateReadyToRun();
            }
        }

        private bool _AutoZip = true;
        public bool AutoZip
        {
            get => _AutoZip;
            set => this.RaiseAndSetIfChanged(ref _AutoZip, value);
        }

        private bool _ReadyToRun = false;
        public bool ReadyToRun
        {
            get => _ReadyToRun;
            set => this.RaiseAndSetIfChanged(ref _ReadyToRun, value);
        }

        private bool _AutoClose = false;
        public bool AutoClose
        {
            get => _AutoClose;
            set => this.RaiseAndSetIfChanged(ref _AutoClose, value);
        }
    }
}
