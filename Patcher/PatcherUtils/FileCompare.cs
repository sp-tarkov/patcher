// NOTES:
// - redo search pattern;
//   - compare both directories against eachother, not just one to the other
//   - add ability to handle missing directories

using System.IO;
using Aki.Common.Utils;
using Aki.ByteBanger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PatcherUtils
{
    public class FileCompare
    {
        public string PatchBase;
        public string TargetBase;
        public string CompareBase;
        private int fileCount;
        private int fileIt;

        private int diffCount = 0;
        private int newCount = 0;
        private int delCount = 0;
        private int matchCount = 0;

        private List<FileInfo> TargetPaths;
        private List<FileInfo> ComparePaths;
        private List<LineItem> AdditionalInfo = new List<LineItem>();

        /// <summary>
        /// Provides patch generation progress changes
        /// </summary>
        public event ProgressChangedHandler ProgressChanged;
        protected virtual void RaiseProgressChanged(int progress, int total, string Message = "", params LineItem[] AdditionalLineItems)
        {
            int percent = (int)Math.Floor((double)progress / total * 100);

            ProgressChanged?.Invoke(this, progress, total, percent, Message, AdditionalLineItems);
        }

        /// <summary>
        /// Compare a target file to an assumed compareable file.
        /// </summary>
        /// <param name="targetFile">The known target path</param>
        /// <param name="assumedCompareFile">The assumed comparable file path</param>
        /// <returns>True if a comparison was made | False if a comparison could not be made</returns>
        private bool Compare(string targetFile, string assumedCompareFile)
        {
            string patchFilePath = targetFile.Replace(TargetBase, PatchBase);
            //we know our target file exists
            byte[] targetData = VFS.ReadFile(targetFile);

            if(!File.Exists(assumedCompareFile))
            {
                //save the data we won't have in our target as new
                VFS.WriteFile($"{patchFilePath}.new", Zlib.Compress(targetData, ZlibCompression.Maximum));
                newCount++;
                return true;
            }

            //now our compare file is known to exist
            byte[] compareData = VFS.ReadFile(assumedCompareFile);

            // get diffs
            DiffResult result = PatchUtil.Diff(compareData, targetData);

            switch (result.Result)
            {
                case DiffResultType.Success:
                    VFS.WriteFile($"{patchFilePath}.bpf", result.PatchInfo.ToBytes());
                    diffCount++;
                    return true;

                case DiffResultType.FilesMatch:
                    matchCount++;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Compares the base folders and generates patch files.
        /// </summary>
        /// <returns>True if patches were generated successfully | False if patch generation failed</returns>
        public bool CompareAll()
        {
            DirectoryInfo targetDirInfo = new DirectoryInfo(TargetBase);
            DirectoryInfo compareDirInfo = new DirectoryInfo(CompareBase);

            AdditionalInfo.Add(new LineItem("Diff Patch", "0"));
            AdditionalInfo.Add(new LineItem("New Patch", "0"));
            AdditionalInfo.Add(new LineItem("Del Patch", "0"));
            AdditionalInfo.Add(new LineItem("Files Match", "0"));

            if (!targetDirInfo.Exists || !compareDirInfo.Exists)
            {
                Console.WriteLine("Target or Compare folder does not exist");
                return false;
            }

            //Get all the files recursively
            TargetPaths = new List<FileInfo>(targetDirInfo.GetFiles("*.*", SearchOption.AllDirectories));
            ComparePaths = new List<FileInfo>(compareDirInfo.GetFiles("*.*", SearchOption.AllDirectories));

            RaiseProgressChanged(0, fileCount, "Generating diffs...");

            /* Comparing Target files -> Compare files
             *     - Exists        = Diff (.bfd file)
             *     - Doesn't Exist = New (.new file)
             * 
             * Once everything has been compared from one side, any remaining paths in our ComparePaths
             * are things that don't exist in our target and can be deleted (.del file)
             */

            for (int x = 0; x < TargetPaths.Count; x++)
            {
                FileInfo file = TargetPaths[x];

                string assumedComparePath = file.DirectoryName.Replace(TargetBase, CompareBase);

                if (!Compare(file.FullName, VFS.Combine(assumedComparePath, file.Name)))
                {
                    return false;
                }

                //remove any existing files from our ComparePaths
                FileInfo assumedFile = new FileInfo(VFS.Combine(assumedComparePath, file.Name));
                if (assumedFile.Exists && ComparePaths.Exists(x => x.FullName == assumedFile.FullName))
                {
                    ComparePaths.Remove(ComparePaths.Where(x => x.FullName == assumedFile.FullName).FirstOrDefault());
                }


                AdditionalInfo[0].ItemValue = diffCount.ToString();
                AdditionalInfo[1].ItemValue = newCount.ToString();
                AdditionalInfo[3].ItemValue = matchCount.ToString();

                fileIt++;
                RaiseProgressChanged(fileIt, fileCount, "", AdditionalInfo.ToArray());
            }

            
            if (ComparePaths.Count == 0)
            {
                //if there are no files to delete, just return true
                return true;
            }

            //progress reset for files that need to be deleted
            RaiseProgressChanged(0, ComparePaths.Count, "Processing .del files...");
            fileIt = 0;
            fileCount = ComparePaths.Count;

            //the paths remaining in ComparePaths don't exist in our target and need to be removed during patching.
            foreach (FileInfo file in ComparePaths)
            {
                //add del files                               replace root dir with patch base
                string patchFilePath = file.FullName.Replace(CompareBase, PatchBase);
                VFS.WriteFile($"{patchFilePath}.del", new byte[0]);

                delCount++;
                AdditionalInfo[2].ItemValue = delCount.ToString();

                fileIt++;
                RaiseProgressChanged(fileIt, fileCount, "", AdditionalInfo.ToArray());
            }

            return true;
        }

        public FileCompare(string TargetBase, string CompareBase, string PatchBase)
        {
            this.TargetBase = TargetBase;
            this.CompareBase = CompareBase;
            this.PatchBase = PatchBase;

            fileCount = VFS.GetFilesCount(TargetBase);
            fileIt = 0;
        }
    }
}
