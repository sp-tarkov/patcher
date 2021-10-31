using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchGenerator
{
    public class GenStartupArgs
    {
        public bool ReadyToRun => OutputFolderName != "" && CompareFolderPath != "" && TargetFolderPath != "";
        public string OutputFolderName { get; private set; } = "";
        public string CompareFolderPath { get; private set; } = "";
        public string TargetFolderPath { get; private set; } = "";
        public bool AutoZip { get; private set; } = true;
        protected GenStartupArgs(string OutputFolderName, string CompareFolderPath, string TargetFolderPath, bool AutoZip)
        {
            this.OutputFolderName = OutputFolderName;
            this.CompareFolderPath = CompareFolderPath;
            this.TargetFolderPath = TargetFolderPath;
            this.AutoZip = AutoZip;
        }

        public static GenStartupArgs Parse(string[] Args)
        {
            if (Args == null || Args.Length == 0) return null;

            string ofn = "";
            string cfp = "";
            string tfp = "";
            bool az = true;

            foreach(string arg in Args)
            {
                if (arg.Split("::").Length != 2) return null;

                var argSplit = arg.Split("::");

                switch(argSplit[0])
                {
                    case "OutputFolderName":
                        {
                            ofn = argSplit[1];
                            break;
                        }
                    case "CompareFolderPath":
                        {
                            cfp = argSplit[1];
                            break;
                        }
                    case "TargetFolderPath":
                        {
                            tfp = argSplit[1];
                            break;
                        }
                    case "AutoZip":
                        {
                            az = bool.Parse(argSplit[1]);
                            break;
                        }
                }
            }

            return new GenStartupArgs(ofn, cfp, tfp, az);
        }
    }
}
