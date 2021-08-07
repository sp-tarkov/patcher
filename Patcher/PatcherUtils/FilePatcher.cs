using System;
using System.IO;
using Aki.Common.Utils;
using Aki.ByteBanger;
using System.Collections.Generic;
using System.Linq;

namespace PatcherUtils
{
    public class FilePatcher
    {
        public string TargetBase;
        public string PatchBase;
        private int fileCount;
        private int fileIt;

        private int diffCount;
        private int newCount;
        private int delCount;

        private List<LineItem> AdditionalInfo;


        public event ProgressChangedHandler ProgressChanged;

        protected virtual void RaiseProgressChanged(int progress, int total, string Message = "", params LineItem[] AdditionalLineItems)
        {
            int percent = (int)Math.Floor((double)progress / total * 100);

            ProgressChanged?.Invoke(this, progress, total, percent, Message, AdditionalLineItems);
        }

        public bool Patch(string targetfile, string patchfile)
        {
            byte[] target = VFS.ReadFile(targetfile);
            byte[] patch = VFS.ReadFile(patchfile);

            PatchResult result = PatchUtil.Patch(target, PatchInfo.FromBytes(patch));

            switch (result.Result)
            {
                case PatchResultType.Success:
                    VFS.WriteFile(targetfile, result.PatchedData);
                    return true;

                case PatchResultType.AlreadyPatched:
                case PatchResultType.InputChecksumMismatch:
                case PatchResultType.InputLengthMismatch:
                    return true;

                case PatchResultType.OutputChecksumMismatch:
                default:
                    return false;
            }
        }

        private bool PatchAll(string targetpath, string patchpath)
        {
            DirectoryInfo di = new DirectoryInfo(patchpath);

            foreach (FileInfo file in di.GetFiles())
            {
                FileInfo target = null;

                switch (file.Extension)
                {
                    // patch
                    case ".bpf":
                        {
                            target = new FileInfo(VFS.Combine(targetpath, file.Name.Replace(".bpf", "")));

                            if (!Patch(target.FullName, file.FullName))
                            {
                                // patch failed
                                return false;
                            }

                            diffCount--;
                        }
                        break;

                    // add new files
                    case ".new":
                        {
                            target = new FileInfo(VFS.Combine(targetpath, file.Name.Replace(".new", "")));
                            VFS.WriteFile(target.FullName, Zlib.Decompress(VFS.ReadFile(file.FullName)));
                            newCount--;
                        }
                        break;

                    // delete old files
                    case ".del":
                        {
                            target = new FileInfo(VFS.Combine(targetpath, file.Name.Replace(".del", "")));
                            target.IsReadOnly = false;
                            target.Delete();
                            delCount--;
                        }
                        break;
                }

                AdditionalInfo[0].ItemValue = diffCount.ToString();
                AdditionalInfo[1].ItemValue = newCount.ToString();
                AdditionalInfo[2].ItemValue = delCount.ToString();

                ++fileIt;
                RaiseProgressChanged(fileIt, fileCount, target.Name, AdditionalInfo.ToArray());
            }

            foreach (DirectoryInfo directory in di.GetDirectories())
            {
                PatchAll(VFS.Combine(targetpath, directory.Name), directory.FullName);
            }

            di.Refresh();

            if (di.GetFiles().Length == 0 && di.GetDirectories().Length == 0)
            {
                // remove empty folders
                di.Delete();
            }

            return true;
        }

        public bool Run()
        {
            fileCount = VFS.GetFilesCount(PatchBase);

            FileInfo[] files = new DirectoryInfo(PatchBase).GetFiles("*.*", SearchOption.AllDirectories);

            diffCount = files.Where(x => x.Extension == ".bpf").Count();
            newCount = files.Where(x => x.Extension == ".new").Count();
            delCount = files.Where(x => x.Extension == ".del").Count();

            AdditionalInfo = new List<LineItem>()
            {
                new LineItem("Patches Remaining", diffCount.ToString()),
                new LineItem("New Files to Inflate", newCount.ToString()),
                new LineItem("Files to Delete", delCount.ToString())
            };

            fileIt = 0;
            return PatchAll(TargetBase, PatchBase);
        }
    }
}
