using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PatcherUtils
{
    public class PatchHelper
    {
        private string SourceFolder = "";
        private string TargetFolder = "";
        private string DeltaFolder = "";

        private int fileCountTotal;
        private int filesProcessed;

        private int deltaCount;
        private int newCount;
        private int delCount;
        private int existCount;

        private List<LineItem> AdditionalInfo = new List<LineItem>();

        /// <summary>
        /// Reports patch creation or application progress
        /// </summary>
        /// <remarks>Includes an array of <see cref="LineItem"/> with details for each type of patch</remarks>
        public event ProgressChangedHandler ProgressChanged;

        protected virtual void RaiseProgressChanged(int progress, int total, string Message = "", params LineItem[] AdditionalLineItems)
        {
            int percent = (int)Math.Floor((double)progress / total * 100);

            ProgressChanged?.Invoke(this, progress, total, percent, Message, AdditionalLineItems);
        }

        /// <summary>
        /// A helper class to create and apply patches to folders
        /// </summary>
        /// <param name="SourceFolder">The directory that will have patches applied to it.</param>
        /// <param name="TargetFolder">The directory to compare against during patch creation.</param>
        /// <param name="DeltaFolder">The directory where the patches are/will be located.</param>
        /// <remarks><paramref name="TargetFolder"/> can be null if you only plan to apply patches.</remarks>
        public PatchHelper(string SourceFolder, string TargetFolder, string DeltaFolder)
        {
            this.SourceFolder = SourceFolder;
            this.TargetFolder = TargetFolder;
            this.DeltaFolder = DeltaFolder;
        }

        /// <summary>
        /// Get the delta folder file path. 
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="SourceFolderPath"></param>
        /// <param name="FileExtension">The extension to append to the file</param>
        /// <returns>A file path inside the delta folder</returns>
        private string GetDeltaPath(string SourceFilePath, string SourceFolderPath, string FileExtension)
        {
            return Path.Join(DeltaFolder, $"{SourceFilePath.Replace(SourceFolderPath, "")}.{FileExtension}");
        }

        /// <summary>
        /// Check if two files have the same MD5 hash
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="TargetFilePath"></param>
        /// <returns>True if the hashes match</returns>
        private bool CompareFileHashes(string SourceFilePath, string TargetFilePath)
        {
            using (MD5 md5Service = MD5.Create())
            using (var sourceStream = File.OpenRead(SourceFilePath))
            using (var targetStream = File.OpenRead(TargetFilePath))
            {
                byte[] sourceHash = md5Service.ComputeHash(sourceStream);
                byte[] targetHash = md5Service.ComputeHash(targetStream);

                return Enumerable.SequenceEqual(sourceHash, targetHash);
            }
        }

        /// <summary>
        /// Apply a delta to a file using xdelta
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="DeltaFilePath"></param>
        private void ApplyDelta(string SourceFilePath, string DeltaFilePath)
        {
            string decodedPath = SourceFilePath + ".decoded";

            Process.Start(new ProcessStartInfo
            {
                FileName = LazyOperations.XDelta3Path,
                Arguments = $"-d -f -s \"{SourceFilePath}\" \"{DeltaFilePath}\" \"{decodedPath}\"",
                CreateNoWindow = true
            })
            .WaitForExit();

            if (File.Exists(decodedPath))
            {
                File.Move(decodedPath, SourceFilePath, true);
            }
        }

        /// <summary>
        /// Create a .delta file using xdelta
        /// </summary>
        /// <param name="SourceFilePath"></param>
        /// <param name="TargetFilePath"></param>
        /// <remarks>Used to patch an existing file with xdelta</remarks>
        private void CreateDelta(string SourceFilePath, string TargetFilePath)
        {
            FileInfo sourceFileInfo = new FileInfo(SourceFilePath);

            string deltaPath = GetDeltaPath(SourceFilePath, SourceFolder, "delta");

            Directory.CreateDirectory(deltaPath.Replace(sourceFileInfo.Name + ".delta", ""));

            //TODO - don't hardcode FileName

            Process.Start(new ProcessStartInfo
            {
                FileName = LazyOperations.XDelta3Path,
                Arguments = $"-0 -e -f -s \"{SourceFilePath}\" \"{TargetFilePath}\" \"{deltaPath}\"",
                CreateNoWindow = true
            })
            .WaitForExit();
        }

        /// <summary>
        /// Create a .del file
        /// </summary>
        /// <param name="SourceFile"></param>
        /// <remarks>Used to mark a file for deletion</remarks>
        private void CreateDelFile(string SourceFile)
        {
            FileInfo sourceFileInfo = new FileInfo(SourceFile);

            string deltaPath = GetDeltaPath(SourceFile, SourceFolder, "del");

            Directory.CreateDirectory(deltaPath.Replace(sourceFileInfo.Name + ".del", ""));

            File.Create(deltaPath);
        }

        /// <summary>
        /// Create a .new file
        /// </summary>
        /// <param name="TargetFile"></param>
        /// <remarks>Used to mark a file that needs to be added</remarks>
        private void CreateNewFile(string TargetFile)
        {
            FileInfo targetSourceInfo = new FileInfo(TargetFile);

            string deltaPath = GetDeltaPath(TargetFile, TargetFolder, "new");

            Directory.CreateDirectory(deltaPath.Replace(targetSourceInfo.Name + ".new", ""));

            targetSourceInfo.CopyTo(deltaPath, true);
        }

        /// <summary>
        /// Generate a full set of patches using the source and target folders specified during contruction./>
        /// </summary>
        /// <returns></returns>
        /// <remarks>Patches are created in the delta folder specified during contruction</remarks>
        public bool GeneratePatches()
        {
            //get all directory information needed
            DirectoryInfo sourceDir = new DirectoryInfo(SourceFolder);
            DirectoryInfo targetDir = new DirectoryInfo(TargetFolder);
            DirectoryInfo deltaDir = Directory.CreateDirectory(DeltaFolder);

            //make sure all directories exist
            if (!sourceDir.Exists || !targetDir.Exists || !deltaDir.Exists)
            {
                //One of the directories doesn't exist
                return false;
            }

            LazyOperations.CleanupTempDir();
            LazyOperations.PrepTempDir();

            List<FileInfo> SourceFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            fileCountTotal = SourceFiles.Count;

            AdditionalInfo.Clear();
            AdditionalInfo.Add(new LineItem("Delta Patch", 0));
            AdditionalInfo.Add(new LineItem("New Patch", 0));
            AdditionalInfo.Add(new LineItem("Del Patch", 0));
            AdditionalInfo.Add(new LineItem("File Exists", 0));

            filesProcessed = 0;

            RaiseProgressChanged(0, fileCountTotal, "Generating deltas...");

            foreach (FileInfo targetFile in targetDir.GetFiles("*", SearchOption.AllDirectories))
            {
                //find a matching source file based on the relative path of the file
                FileInfo sourceFile = SourceFiles.Find(f => f.FullName.Replace(sourceDir.FullName, "") == targetFile.FullName.Replace(targetDir.FullName, ""));

                //if the target file doesn't exist in the source files, the target file needs to be added.
                if (sourceFile == null)
                {
                    CreateNewFile(targetFile.FullName);

                    newCount++;
                    filesProcessed++;

                    RaiseProgressChanged(filesProcessed, fileCountTotal, $"{targetFile.FullName.Replace(TargetFolder, "...")}.new", AdditionalInfo.ToArray());

                    continue;
                }

                string extension = "";

                //if a matching source file was found, check the file hashes and get the delta.
                if (!CompareFileHashes(sourceFile.FullName, targetFile.FullName))
                {
                    CreateDelta(sourceFile.FullName, targetFile.FullName);
                    extension = ".delta";
                    deltaCount++;
                }
                else
                {
                    existCount++;
                }

                SourceFiles.Remove(sourceFile);

                filesProcessed++;

                AdditionalInfo[0].ItemValue = deltaCount;
                AdditionalInfo[1].ItemValue = newCount;
                AdditionalInfo[3].ItemValue = existCount;

                RaiseProgressChanged(filesProcessed, fileCountTotal, $"{targetFile.FullName.Replace(TargetFolder, "...")}{extension}", AdditionalInfo.ToArray());
            }

            //Any remaining source files do not exist in the target folder and can be removed.
            //reset progress info

            if (SourceFiles.Count == 0) return true;

            RaiseProgressChanged(0, SourceFiles.Count, "Processing .del files...");
            filesProcessed = 0;
            fileCountTotal = SourceFiles.Count;

            foreach (FileInfo delFile in SourceFiles)
            {
                CreateDelFile(delFile.FullName);

                delCount++;

                AdditionalInfo[2].ItemValue = delCount;

                filesProcessed++;
                RaiseProgressChanged(filesProcessed, fileCountTotal, $"{delFile.FullName.Replace(SourceFolder, "...")}.del", AdditionalInfo.ToArray());
            }

            return true;
        }

        /// <summary>
        /// Apply a set of patches using the source and delta folders specified during construction.
        /// </summary>
        /// <returns></returns>
        public string ApplyPatches()
        {
            //get needed directory information
            DirectoryInfo sourceDir = new DirectoryInfo(SourceFolder);
            DirectoryInfo deltaDir = new DirectoryInfo(DeltaFolder);

            //check directories exist
            if (!sourceDir.Exists || !deltaDir.Exists)
            {
                return "One of the supplied directories doesn't exist";
            }

            LazyOperations.CleanupTempDir();
            LazyOperations.PrepTempDir();

            List<FileInfo> SourceFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            List<FileInfo> deltaFiles = deltaDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            deltaCount = deltaFiles.Where(x => x.Extension == ".delta").Count();
            newCount = deltaFiles.Where(x => x.Extension == ".new").Count();
            delCount = deltaFiles.Where(x => x.Extension == ".del").Count();


            AdditionalInfo = new List<LineItem>()
            {
                new LineItem("Patches Remaining", deltaCount),
                new LineItem("New Files to Add", newCount),
                new LineItem("Files to Delete", delCount)
            };

            filesProcessed = 0;

            fileCountTotal = deltaFiles.Count;

            foreach (FileInfo deltaFile in deltaDir.GetFiles("*", SearchOption.AllDirectories))
            {
                switch (deltaFile.Extension)
                {
                    case ".delta":
                        {
                            //apply delta
                            FileInfo sourceFile = SourceFiles.Find(f => f.FullName.Replace(sourceDir.FullName, "") == deltaFile.FullName.Replace(deltaDir.FullName, "").Replace(".delta", ""));

                            if (sourceFile == null)
                            {
                                return $"Failed to find matching source file for '{deltaFile.FullName}'";
                            }

                            ApplyDelta(sourceFile.FullName, deltaFile.FullName);

                            deltaCount--;

                            break;
                        }
                    case ".new":
                        {
                            if (newCount == 2 || newCount == 1 || newCount == 0)
                            {

                            }

                            //copy new file
                            string destination = Path.Join(sourceDir.FullName, deltaFile.FullName.Replace(deltaDir.FullName, "").Replace(".new", ""));

                            File.Copy(deltaFile.FullName, destination, true);

                            newCount--;

                            break;
                        }
                    case ".del":
                        {
                            //remove unneeded file
                            string delFilePath = Path.Join(sourceDir.FullName, deltaFile.FullName.Replace(deltaDir.FullName, "").Replace(".del", ""));

                            File.Delete(delFilePath);

                            delCount--;

                            break;
                        }
                }

                AdditionalInfo[0].ItemValue = deltaCount;
                AdditionalInfo[1].ItemValue = newCount;
                AdditionalInfo[2].ItemValue = delCount;

                ++filesProcessed;
                RaiseProgressChanged(filesProcessed, fileCountTotal, deltaFile.Name, AdditionalInfo.ToArray());
            }

            LazyOperations.CleanupTempDir();

            Directory.Delete(LazyOperations.PatchFolder, true);

            return $"Patching Complete. You can delete the patcher.exe file.";
        }
    }
}
