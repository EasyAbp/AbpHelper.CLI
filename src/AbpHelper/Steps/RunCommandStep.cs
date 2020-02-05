using System;
using System.Threading.Tasks;
using AbpHelper.Utils;

namespace AbpHelper.Steps
{
    public class RunCommandStep : Step
    {
        public string Command { get; set; } = string.Empty;

        protected override Task RunStep()
        {
            LogInput(() => Command);
            var output = CmdHelper.RunCmdAndGetOutput(Command);
            Console.WriteLine(output);
            return Task.CompletedTask;
        }
    }
}