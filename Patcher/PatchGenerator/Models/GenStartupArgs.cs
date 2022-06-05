namespace PatchGenerator.Models
{
    public class GenStartupArgs
    {
        public bool ReadyToRun => OutputFolderName != "" && SourceFolderPath != "" && TargetFolderPath != "";
        public string OutputFolderName { get; private set; } = "";
        public string SourceFolderPath { get; private set; } = "";
        public string TargetFolderPath { get; private set; } = "";
        public bool AutoClose { get; private set; } = false;
        public bool AutoZip { get; private set; } = true;
        protected GenStartupArgs(string outputFolderName, string sourceFolderPath, string targetFolderPath, bool autoZip, bool autoClose)
        {
            OutputFolderName = outputFolderName;
            SourceFolderPath = sourceFolderPath;
            TargetFolderPath = targetFolderPath;
            AutoZip = autoZip;
            AutoClose = autoClose;
        }

        public static GenStartupArgs Parse(string[] Args)
        {
            if (Args == null || Args.Length == 0) return null;

            string outputFolderPath = "";
            string sourceFolderPath = "";
            string targetFolderPath = "";
            bool autoZip = true;
            bool autoClose = false;

            foreach (string arg in Args)
            {
                if (arg.Split("::").Length != 2) return null;

                var argSplit = arg.Split("::");

                switch (argSplit[0].ToLower())
                {
                    case "OutputFolderName".ToLower():
                        {
                            outputFolderPath = argSplit[1];
                            break;
                        }
                    case "SourceFolderPath".ToLower():
                        {
                            sourceFolderPath = argSplit[1];
                            break;
                        }
                    case "TargetFolderPath".ToLower():
                        {
                            targetFolderPath = argSplit[1];
                            break;
                        }
                    case "AutoZip".ToLower():
                        {
                            autoZip = bool.Parse(argSplit[1]);
                            break;
                        }
                    case "AutoClose".ToLower():
                        {
                            autoClose = bool.Parse(argSplit[1]);
                            break;
                        }
                }
            }

            return new GenStartupArgs(outputFolderPath, sourceFolderPath, targetFolderPath, autoZip, autoClose);
        }
    }
}
