using Aki.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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

        private List<LineItem> AdditionalInfo = new List<LineItem>();

        public event ProgressChangedHandler ProgressChanged;

        protected virtual void RaiseProgressChanged(int progress, int total, string Message = "", params LineItem[] AdditionalLineItems)
        {
            int percent = (int)Math.Floor((double)progress / total * 100);

            ProgressChanged?.Invoke(this, progress, total, percent, Message, AdditionalLineItems);
        }

        public PatchHelper(string SourceFolder, string TargetFolder, string DeltaFolder)
        {
            this.SourceFolder = SourceFolder;
            this.TargetFolder = TargetFolder;
            this.DeltaFolder = DeltaFolder;
        }

        private string GetDeltaPath(string sourceFile, string sourceFolder, string extension)
        {
            return Path.Join(DeltaFolder, $"{sourceFile.Replace(sourceFolder, "")}.{extension}");
        }

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

            if(File.Exists(decodedPath))
            {
                File.Move(decodedPath, SourceFilePath, true);
            }
        }

        private void CreateDelta(string SourceFilePath, string TargetFilePath)
        {
            FileInfo sourceFileInfo = new FileInfo(SourceFilePath);

            string deltaPath = GetDeltaPath(SourceFilePath, SourceFolder, "delta");

            Directory.CreateDirectory(deltaPath.Replace(sourceFileInfo.Name+".delta", ""));

            //TODO - don't hardcode FileName

            Process.Start(new ProcessStartInfo
            {
                FileName = LazyOperations.XDelta3Path,
                Arguments = $"-0 -e -f -s \"{SourceFilePath}\" \"{TargetFilePath}\" \"{deltaPath}\"",
                CreateNoWindow = true
            })
            .WaitForExit();
        }

        private void CreateDelFile(string SourceFile)
        {
            FileInfo sourceFileInfo = new FileInfo(SourceFile);

            string deltaPath = GetDeltaPath(SourceFile, SourceFolder, "del");

            Directory.CreateDirectory(deltaPath.Replace(sourceFileInfo.Name+".del", ""));

            File.Create(deltaPath);
        }

        private void CreateNewFile(string TargetFile)
        {
            FileInfo targetSourceInfo = new FileInfo(TargetFile);

            string deltaPath = GetDeltaPath(TargetFile, TargetFolder, "new");

            Directory.CreateDirectory(deltaPath.Replace(targetSourceInfo.Name+".new", ""));

            targetSourceInfo.CopyTo(deltaPath, true);
        }

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

            List<FileInfo> SourceFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            fileCountTotal = SourceFiles.Count;

            AdditionalInfo.Clear();
            AdditionalInfo.Add(new LineItem("Delta Patch", "0"));
            AdditionalInfo.Add(new LineItem("New Patch", "0"));
            AdditionalInfo.Add(new LineItem("Del Patch", "0"));

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

                    RaiseProgressChanged(filesProcessed, fileCountTotal, targetFile.Name, AdditionalInfo.ToArray());

                    continue;
                }

                //if a matching source file was found, get the delta for it.
                CreateDelta(sourceFile.FullName, targetFile.FullName);

                SourceFiles.Remove(sourceFile);

                deltaCount++;
                filesProcessed++;

                AdditionalInfo[0].ItemValue = deltaCount.ToString();
                AdditionalInfo[1].ItemValue = newCount.ToString();

                RaiseProgressChanged(filesProcessed, fileCountTotal, targetFile.Name, AdditionalInfo.ToArray());
            }

            //Any remaining source files do not exist in the target folder and can be removed.
            //reset progress info
            RaiseProgressChanged(0, SourceFiles.Count, "Processing .del files...");
            filesProcessed = 0;
            fileCountTotal = SourceFiles.Count;

            foreach (FileInfo delFile in SourceFiles)
            {
                CreateDelFile(delFile.FullName);

                delCount++;

                AdditionalInfo[2].ItemValue = delCount.ToString();

                filesProcessed++;
                RaiseProgressChanged(filesProcessed, fileCountTotal, "", AdditionalInfo.ToArray());
            }

            return true;
        }

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

            List<FileInfo> SourceFiles = sourceDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            List<FileInfo> deltaFiles = deltaDir.GetFiles("*", SearchOption.AllDirectories).ToList();

            deltaCount = deltaFiles.Where(x => x.Extension == ".delta").Count();
            newCount = deltaFiles.Where(x => x.Extension == ".new").Count();
            delCount = deltaFiles.Where(x => x.Extension == ".del").Count();


            AdditionalInfo = new List<LineItem>()
            {
                new LineItem("Patches Remaining", deltaCount.ToString()),
                new LineItem("New Files to Add", newCount.ToString()),
                new LineItem("Files to Delete", delCount.ToString())
            };

            filesProcessed = 0;

            fileCountTotal = deltaFiles.Count;

            foreach (FileInfo deltaFile in deltaDir.GetFiles("*", SearchOption.AllDirectories))
            {
                switch(deltaFile.Extension)
                {
                    case ".delta":
                        {
                            //apply delta
                            FileInfo sourceFile = SourceFiles.Find(f => f.FullName.Replace(sourceDir.FullName, "") == deltaFile.FullName.Replace(deltaDir.FullName, "").Replace(".delta", ""));

                            if(sourceFile == null)
                            {
                                return $"Failed to find matching source file for '{deltaFile.FullName}'";
                            }

                            ApplyDelta(sourceFile.FullName, deltaFile.FullName);

                            deltaCount--;

                            break;
                        }
                    case ".new":
                        {
                            //copy new file
                            string destination = Path.Join(sourceDir.FullName, deltaFile.FullName.Replace(deltaDir.FullName, "").Replace(".new", ""));

                            File.Copy(deltaFile.FullName, destination);

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

                AdditionalInfo[0].ItemValue = deltaCount.ToString();
                AdditionalInfo[1].ItemValue = newCount.ToString();
                AdditionalInfo[2].ItemValue = delCount.ToString();

                ++filesProcessed;
                RaiseProgressChanged(filesProcessed, fileCountTotal, deltaFile.Name, AdditionalInfo.ToArray());
            }

            LazyOperations.CleanupTempDir();

            Directory.Delete(LazyOperations.PatchFolder, true);

            return $"Patching Complete. You can delete the patcher.exe file.";
        }
    }
}
