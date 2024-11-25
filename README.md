# Patcher

Allows for generating and applying patches to software.
Currently used for downgrading EFT.

## Requirements
- .net 8

## Development Stuff
- VS 2022 w/ Avalonia Extension
- git-lfs

## PatchClient Parameters
The patch client only has one parameter at this time (`autoclose`) which can be passed to the patch client like this:

(this parameter is not case sensitive)

`patcher.exe autoclose`

- AutoClose: Whether or not the patch client should automatically close and return an exit code.

## PatchGenerator Parameters
Parameters are passed as strings like `param::value`: Example: 

(parameters are not case sensitive)

`"TargetFolderPath::C:\path\to\folder"`
- OutputFolderName: The folder to save the generated patches to.
- SourceFolderPath: The source folder for (the files expected to be patched).
- TargetFolderPath: The target folder (the expected result of patching).
- AutoZip: Whether or not the patch generator should automatically compress the patches after generation.
- AutoClose: Whether or not the patch generator should automatically close and return an exit code.

## Exit Codes
```cs
public enum PatcherExitCode
{
    ProgramClosed  = 0,  // program was closed by user
    Success        = 10, // patching or patch generation succeeded
    EftExeNotFound = 11, // EFT exe was not found during patching (patch client only)
    NoPatchFolder  = 12, // no patch folder was found during patching (patch client only)
    MissingFile    = 13, // a matching file could not be found during patching (patch client only) 
    MissingDir     = 14, // a directory could not be found during patch generation (source/target/output) (patch generator only)
    PatchFailed    = 15  // a patch file failed (patch client only)
}
```
