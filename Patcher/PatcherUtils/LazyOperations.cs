using System;
using PatcherUtils.Model;
using System.IO;
using System.Reflection;
using Aki.Common.Utils;
using SevenZip;

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

        private static string SevenZDll = "7z.dll";

        /// <summary>
        /// The path to the 7za.exe file in the <see cref="TempDir"/>
        /// </summary>
        public static string SevenZDllPath = $"{TempDir}\\{SevenZDll}";

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
        private static void StreamResourceOut(Assembly assembly, string ResourceName, string OutputFilePath)
        {
            FileInfo outputFile = new FileInfo(OutputFilePath);

            if (outputFile.Exists)
            {
                PatchLogger.LogInfo($"Deleting Existing Resource: {outputFile.Name}");
                outputFile.Delete();
            }

            if (!outputFile.Directory.Exists)
            {
                PatchLogger.LogInfo($"Creating Resource Directory: {outputFile.Directory.Name}");
                Directory.CreateDirectory(outputFile.Directory.FullName);
            }

            using (FileStream fs = File.Create(OutputFilePath))
            using (Stream s = assembly.GetManifestResourceStream(ResourceName))
            {
                s.CopyTo(fs);
                PatchLogger.LogInfo($"Resourced streamed out of assembly: {outputFile.Name}");
            }
        }

        /// <summary>
        /// Checks the resources in the assembly and streams them to the temp directory for later use.
        /// </summary>
        public static void ExtractResourcesToTempDir(Assembly assembly = null)
        {
            if(assembly == null) assembly = Assembly.GetExecutingAssembly();

            foreach (string resource in assembly.GetManifestResourceNames())
            {
                switch (resource)
                {
                    case string a when a.EndsWith(SevenZDll):
                        {
                            StreamResourceOut(assembly, resource, SevenZDllPath);
                            break;
                        }
                    case string a when a.EndsWith(PatcherClient):
                        {
                            StreamResourceOut(assembly, resource, PatcherClientPath);
                            break;
                        }
                    case string a when a.EndsWith(XDelta3EXE):
                        {
                            StreamResourceOut(assembly, resource, XDelta3Path);
                            break;
                        }
                }
            }
        }

        public static void CompressDirectory(string SourceDirectoryPath, string DestinationFilePath, IProgress<int> progress)
        {
            var outputFile = new FileInfo(DestinationFilePath);
            
            SevenZipBase.SetLibraryPath(SevenZDllPath);
            var compressor = new SevenZipCompressor()
            {
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMethod = CompressionMethod.Lzma2,
                CompressionLevel = CompressionLevel.Normal
            };

            compressor.Compressing += (_, args) =>
            {
                progress.Report(args.PercentDone);
            };

            using var outputStream = outputFile.OpenWrite();
            
            compressor.CompressDirectory(SourceDirectoryPath, outputStream);
            
            outputFile.Refresh();

            // failed to compress data
            if (!outputFile.Exists || outputFile.Length == 0)
            {
                Logger.LogError("Failed to compress patcher");
            }
        }

        /// <summary>
        /// Deletes the <see cref="TempDir"/> recursively
        /// </summary>
        public static void CleanupTempDir()
        {
            DirectoryInfo dir = new DirectoryInfo(TempDir);

            if (dir.Exists)
            {
                dir.Delete(true);
                PatchLogger.LogInfo("Temp directory delted");
            }
        }
    }
}
