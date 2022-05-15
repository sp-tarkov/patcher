using PatchClient.Models;

namespace PatcherUtils.Model
{
    public class PatchMessage
    {
        public string Message { get; private set; }
        public PatcherExitCode ExitCode { get; private set; }

        public PatchMessage(string message, PatcherExitCode exitCode)
        {
            Message = message;
            ExitCode = exitCode;
        }
    }
}
