using System;
using System.IO;

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

        public static void LogInfo(string message) => LogToFile($"{GetTimestamp()}[INFO]: {message}");
        public static void LogError(string message) => LogToFile($"{GetTimestamp()}[ERROR]: {message}");
        public static void LogException(Exception ex) => LogToFile($"{GetTimestamp()}[EXCEPTION]: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}");
    }
}
