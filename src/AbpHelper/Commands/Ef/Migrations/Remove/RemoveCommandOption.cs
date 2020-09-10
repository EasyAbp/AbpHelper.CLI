using EasyAbp.AbpHelper.Attributes;

namespace EasyAbp.AbpHelper.Commands.Ef.Migrations.Remove
{
    public class RemoveCommandOption : MigrationsCommandOption
    {
        [Argument("ef-options", Description = "Other options to `dotnet ef migrations remove`")]
        public string[] EfOptions { get; set; } = null!;
    }
}