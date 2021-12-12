using System;
using System.IO;

namespace PatcherUtils
{
    public static class RandomExtensions
    {
        public static string FromCwd(this string s)
        {
            return Path.Combine(Environment.CurrentDirectory, s);
        }
    }
}
