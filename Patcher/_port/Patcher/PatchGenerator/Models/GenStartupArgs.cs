namespace PatchGenerator.Models
{
    public class GenStartupArgs
    {
        public bool ReadyToRun => OutputFolderName != "" && SourceFolderPath != "" && TargetFolderPath != "";
        public string OutputFolderName { get; private set; } = "";
        public string SourceFolderPath { get; private set; } = "";
        public string TargetFolderPath { get; private set; } = "";
        public bool AutoZip { get; private set; } = true;
        protected GenStartupArgs(string OutputFolderName, string SourceFolderPath, string TargetFolderPath, bool AutoZip)
        {
            this.OutputFolderName = OutputFolderName;
            this.SourceFolderPath = SourceFolderPath;
            this.TargetFolderPath = TargetFolderPath;
            this.AutoZip = AutoZip;
        }

        public static GenStartupArgs Parse(string[] Args)
        {
            if (Args == null || Args.Length == 0) return null;

            string outputFolderPath = "";
            string sourceFolderPath = "";
            string targetFolderPath = "";
            bool autoZip = true;

            foreach (string arg in Args)
            {
                if (arg.Split("::").Length != 2) return null;

                var argSplit = arg.Split("::");

                switch (argSplit[0])
                {
                    case "OutputFolderName":
                        {
                            outputFolderPath = argSplit[1];
                            break;
                        }
                    case "SourceFolderPath":
                        {
                            sourceFolderPath = argSplit[1];
                            break;
                        }
                    case "TargetFolderPath":
                        {
                            targetFolderPath = argSplit[1];
                            break;
                        }
                    case "AutoZip":
                        {
                            autoZip = bool.Parse(argSplit[1]);
                            break;
                        }
                }
            }

            return new GenStartupArgs(outputFolderPath, sourceFolderPath, targetFolderPath, autoZip);
        }
    }
}
