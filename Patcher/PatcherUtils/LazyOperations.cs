using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PatcherUtils
{
    public class LazyOperations
    {
        /// <summary>
        /// A directory to store temporary data.
        /// </summary>
        public static string TempDir = "PATCHER_TEMP".FromCwd();


        /// <summary>
        /// The folder that the patches will be stored in
        /// </summary>
        public static string PatchFolder = "Aki_Patches";

        private static string SevenZExe = "7za.exe";

        /// <summary>
        /// The path to the 7za.exe file in the <see cref="TempDir"/>
        /// </summary>
        public static string SevenZExePath = $"{TempDir}\\{SevenZExe}";

        private static string PatcherClient = "PatchClient.exe";
        /// <summary>
        /// The path to the patcher.exe file in the <see cref="TempDir"/>
        /// </summary>
        public static string PatcherClientPath = $"{TempDir}\\{PatcherClient}";

        private static string XDelta3EXE = "xdelta3.exe";

        /// <summary>
        /// The path to the xdelta3.exe flie in the <see cref="TempDir"/>
        /// </summary>
        public static string XDelta3Path = $"{TempDir}\\{XDelta3EXE}";

        /// <summary>
        /// Streams embedded resources out of the assembly
        /// </summary>
        /// <param name="ResourceName"></param>
        /// <param name="OutputFilePath"></param>
        /// <remarks>The resource will not be streamed out if the <paramref name="OutputFilePath"/> already exists</remarks>
        private static void StreamResourceOut(string ResourceName, string OutputFilePath)
        {
            FileInfo outputFile = new FileInfo(OutputFilePath);

            if (outputFile.Exists) return;

            if (!outputFile.Directory.Exists)
            {
                Directory.CreateDirectory(outputFile.Directory.FullName);
            }

            using (FileStream fs = File.Create(OutputFilePath))
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
            {
                s.CopyTo(fs);
            }
        }

        /// <summary>
        /// Checks the resources in the assembly and streams them to the temp directory for later use.
        /// </summary>
        public static void PrepTempDir()
        {
            foreach(string resource in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                switch(resource)
                {
                    case string a when a.EndsWith(SevenZExe):
                        {
                            StreamResourceOut(resource, SevenZExePath);
                            break;
                        }
                    case string a when a.EndsWith(PatcherClient):
                        {
                            StreamResourceOut(resource, PatcherClientPath);
                            break;
                        }
                    case string a when a.EndsWith(XDelta3EXE):
                        {
                            StreamResourceOut(resource, XDelta3Path);
                            break;
                        }
                }
            }
        }

        public static void StartZipProcess(string SourcePath, string DestinationPath)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo()
            {
                FileName = SevenZExePath,
                Arguments = $"a {DestinationPath} {SourcePath}"
            };

            Process.Start(procInfo);
        }

        /// <summary>
        /// Deletes the <see cref="TempDir"/> recursively
        /// </summary>
        public static void CleanupTempDir()
        {
            DirectoryInfo dir = new DirectoryInfo(TempDir);

            if(dir.Exists)
            {
                dir.Delete(true);
            }
        }
    }
}
