using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Controller
{
    public class ControllerCommandOption : GenerateCommandOption
    {
        [Argument("name", Description = "The service name(without 'AppService' postfix)")]
        public string Name { get; set; } = null!;

        [Option("skip-build", Description = "Skip building the solution")]
        public bool SkipBuild { get; set; }
    }
}