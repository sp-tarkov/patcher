namespace PatchClient.Models
{
    public enum PatcherExitCode
    {
        Success = 0,
        EftExeNotFound = 10,
        NoPatchFolder = 11,
        MissingFile = 12,
        MissingDir = 13
    }
}
