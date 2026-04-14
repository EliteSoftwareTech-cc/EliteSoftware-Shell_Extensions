using System;
using System.Diagnostics;

namespace WimMergeEngine
{
    public static class ProcessHelper
    {
        public static void RunCommand(string fileName, string arguments, ILogger logger)
        {
            logger?.Log($"Running: {fileName} {arguments}");

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (s, e) => { if (e.Data != null) logger?.Log(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) logger?.Log($"ERROR: {e.Data}"); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Command '{fileName}' failed with exit code {process.ExitCode}.");
                }
            }
        }
    }
}
