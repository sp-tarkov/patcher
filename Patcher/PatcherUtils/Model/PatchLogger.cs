using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PatcherUtils.Model
{
    public static class PatchLogger
    {
        private static string LogFilePath = "Patcher.log".FromCwd();
        private static void LogToFile(string message)
        {
            try
            {
                File.AppendAllLines(LogFilePath, new[] { message });
            }
            catch(Exception)
            {
                //TODO - proper logging at some point or whatever idk... -waffle
            }
        }

        private static string GetTimestamp()
        {
            return DateTime.Now.ToString("MM/dd/yyyy - hh:mm:ss tt]");
        }

        public static void LogOSInfo()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                LogToFile($"{GetTimestamp()}[OS]: Windows");

            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                LogToFile($"{GetTimestamp()}[OS]: Linux");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                LogToFile($"{GetTimestamp()}[OS]: OSX");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                LogToFile($"{GetTimestamp()}[OS]: FreeBSD");


            LogToFile($"{GetTimestamp()}[OS]: {RuntimeInformation.OSDescription}");
        }

        public static void LogDebug(string message) => LogToFile($"{GetTimestamp()}[DEBUG]: {message}");
        public static void LogInfo(string message) => LogToFile($"{GetTimestamp()}[INFO]: {message}");
        public static void LogError(string message) => LogToFile($"{GetTimestamp()}[ERROR]: {message}");
        public static void LogException(Exception ex) => LogToFile($"{GetTimestamp()}[EXCEPTION]: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
    }
}
