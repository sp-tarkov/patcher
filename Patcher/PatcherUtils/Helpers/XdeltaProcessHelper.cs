using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using PatcherUtils.Model;

namespace PatcherUtils.Helpers;

public class XdeltaProcessHelper
{
    private readonly int _timeout = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
    private string _args;
    private string _sourcePath;
    private string _deltaPath;
    private string _decodedPath;
    private bool _isDebug;
    
    public XdeltaProcessHelper(string args, string sourcePath, string deltaPath, string decodedPath, bool isDebug)
    {
        _args = args;
        _sourcePath = sourcePath;
        _deltaPath = deltaPath;
        _decodedPath = decodedPath;
        _isDebug = isDebug;
    }

    public bool Run() => _isDebug ? RunDebug() : RunNormal();
    
    private bool RunNormal()
    {
        try
        {
            using var proc = new Process();

            proc.StartInfo = new ProcessStartInfo
            {
                FileName = LazyOperations.XDelta3Path,
                Arguments = $"{_args} \"{_sourcePath}\" \"{_deltaPath}\" \"{_decodedPath}\"",
                CreateNoWindow = true
            };

            proc.Start();

            if (!proc.WaitForExit(_timeout))
            {
                PatchLogger.LogError("xdelta3 process timed out");
                PatchLogger.LogDebug($"xdelta exit code: {proc.ExitCode}");
                return false;
            }

            PatchLogger.LogDebug($"xdelta exit code: {proc.ExitCode}");
            return true;
        }
        catch (Exception ex)
        {
            PatchLogger.LogException(ex);
            return false;
        }
    }

    private bool DebugPathsCheck()
    {
        try
        {
            var stream = File.Open(_sourcePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
            stream.Dispose();
            PatchLogger.LogDebug($"File is openable: {_sourcePath}");
        }
        catch (Exception ex)
        {
            PatchLogger.LogException(ex);
            return false;
        }

        try
        {
            var stream = File.Open(_deltaPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            stream.Close();
            stream.Dispose();
            PatchLogger.LogDebug($"File is openable: {_deltaPath}");
        }
        catch (Exception ex)
        {
            PatchLogger.LogException(ex);
            return false;
        }

        return true;
    }

    private bool RunDebug()
    {
        if (!DebugPathsCheck())
        {
            return false;
        }
        
        using var proc = new Process();

        proc.StartInfo = new ProcessStartInfo
        {
            FileName = LazyOperations.XDelta3Path,
            Arguments = $"{_args} \"{_sourcePath}\" \"{_deltaPath}\" \"{_decodedPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        using AutoResetEvent outputWaitHandle = new AutoResetEvent(false);
        using AutoResetEvent errorWaitHandle = new AutoResetEvent(false);

        proc.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                outputWaitHandle.Set();
            }
            else
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        proc.ErrorDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                errorWaitHandle.Set();
            }
            else
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        proc.Start();

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        if (!proc.WaitForExit(_timeout) || !outputWaitHandle.WaitOne(_timeout) || !errorWaitHandle.WaitOne(_timeout))
        {
            PatchLogger.LogError("xdelta3 process timed out");
            PatchLogger.LogDebug($"xdelta exit code: {proc.ExitCode}");
            return false;
        }
        
        PatchLogger.LogDebug("__xdelta stdout__");
        PatchLogger.LogDebug(outputBuilder.ToString());
        PatchLogger.LogDebug("__xdelta stderr__");
        PatchLogger.LogDebug(errorBuilder.ToString());
        PatchLogger.LogDebug($"xdelta exit code: {proc.ExitCode}");

        return true;
    }
}