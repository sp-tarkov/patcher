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

        private static string SevenZExe = "7za.exe";
        /// <summary>
        /// The path to the 7za.exe file in the <see cref="TempDir"/>
        /// </summary>
        public static string SevenZExePath = $"{TempDir}\\{SevenZExe}";

        private static string PatcherClient = "patcher.exe";
        /// <summary>
        /// The path to the patcher.exe file in the <see cref="TempDir"/>
        /// </summary>
        public static string PatcherClientPath = $"{TempDir}\\{PatcherClient}";

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
                }
            }
        }

        /// <summary>
        /// Deletes the <see cref="TempDir"/> recursively
        /// </summary>
        /// <Returns>Returns true if the temp directory was deleted.</Returns>
        public static bool CleanupTempDir()
        {
            DirectoryInfo dir = new DirectoryInfo(TempDir);

            if(dir.Exists)
            {
                dir.Delete(true);
            }

            dir.Refresh();

            if(dir.Exists)
            {
                return false;
            }

            return true;
        }
    }
}
