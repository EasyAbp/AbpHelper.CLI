using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Expressions;
using Elsa.Results;
using Elsa.Services.Models;
using Microsoft.Extensions.Logging;

namespace EasyAbp.AbpHelper.Core.Steps.Common
{
    public class RunCommandStep : Step
    {
        public WorkflowExpression<string> Command
        {
            get => GetState<WorkflowExpression<string>>();
            set => SetState(value);
        }

        protected override async Task<ActivityExecutionResult> OnExecuteAsync(WorkflowExecutionContext context, CancellationToken cancellationToken)
        {
            var command = await context.EvaluateAsync(Command, cancellationToken);
            LogInput(() => command);
            var exitCode = RunCommand(command);
            if (exitCode != 0) throw new RunningCommandFailedException(exitCode);

            return Done();
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