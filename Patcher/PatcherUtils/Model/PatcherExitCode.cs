namespace PatchClient.Models
{
    public enum PatcherExitCode
    {
        ProgramClosed = 0,
        Success = 10,
        EftExeNotFound = 11,
        NoPatchFolder = 12,
        MissingFile = 13,
        MissingDir = 14,
        PatchFailed = 15
    }
}
