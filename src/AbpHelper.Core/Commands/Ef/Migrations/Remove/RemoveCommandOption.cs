using EasyAbp.AbpHelper.Core.Attributes;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Remove
{
    public class RemoveCommandOption : MigrationsCommandOption
    {
        [Argument("ef-options", Description = "Other options to `dotnet ef migrations remove`")]
        public string[] EfOptions { get; set; } = null!;
    }
}