using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Generate.Methods
{
    public class MethodsCommandOption : GenerateCommandOption
    {
        [Option('s', "service-name",
            Description = "The service name(without 'AppService' or 'IntegrationService' postfix)")]
        public string ServiceName { get; set; } = null!;

        [Argument("method-names", Description = "The method names")]
        public string[] MethodNames { get; set; } = null!;

        [Argument("no-input", Description = "Not to generate input DTO file and parameter")]
        public bool NoInput { get; set; }

        [Argument("no-output", Description = "Not to generate output DTO file and parameter")]
        public bool NoOutput { get; set; }

        [Option('i', "integration-service",
            Description = "Locate the .cs files with the postfix 'IntegrationService' instead of 'AppService'")]
        public bool IntegrationService { get; set; }
    }
}