using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Controller
{
    public class ControllerCommandOption : GenerateCommandOption
    {
        [Argument("name", Description = "The service name(without 'AppService' or 'IntegrationService' postfix)")]
        public string Name { get; set; } = null!;

        [Option("skip-build", Description = "Skip building the solution")]
        public bool SkipBuild { get; set; }

        [Option('i', "integration-service",
            Description = "Locate the .cs files with the postfix 'IntegrationService' instead of 'AppService'")]
        public bool IntegrationService { get; set; }
    }
}