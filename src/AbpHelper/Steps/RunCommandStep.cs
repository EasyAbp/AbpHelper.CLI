using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AbpHelper.Steps
{
    public class RunCommandStep : Step
    {
        public string Command { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            LogInput(() => Command);
            var exitCode = RunCommand(Command);
            if (exitCode != 0) throw new RunningCommandFailedException(exitCode);

            return Task.CompletedTask;
        }

        private int RunCommand(string command)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(GetFileName())
                {
                    Arguments = GetArguments(command),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.OutputDataReceived += (sender, args) =>
                {
                    if (!args.Data.IsNullOrEmpty()) Logger.LogDebug(args.Data);
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!args.Data.IsNullOrEmpty()) Logger.LogError(args.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        /// <summary>
        ///     Copied from ABP CLI source
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string GetArguments(string command)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "-c \"" + command + "\"";

            //Windows default.
            return "/C \"" + command + "\"";
        }

        /// <summary>
        ///     Copied from ABP CLI source
        /// </summary>
        /// <returns></returns>
        public static string GetFileName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                //TODO: Test this. it should work for both operation systems.
                return "/bin/bash";

            //Windows default.
            return "cmd.exe";
        }
    }

    public class RunningCommandFailedException : Exception
    {
        public RunningCommandFailedException(int exitCode)
        {
            ExitCode = exitCode;
        }

        public int ExitCode { get; }
    }
}