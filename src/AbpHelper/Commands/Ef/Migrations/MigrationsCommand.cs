using System;
using EasyAbp.AbpHelper.Commands.Ef.Migrations.Add;

namespace EasyAbp.AbpHelper.Commands.Ef.Migrations
{
    public class MigrationsCommand : CommandBase
    {
        public MigrationsCommand(IServiceProvider serviceProvider) : base(serviceProvider, "migrations", "run `dotnet ef migrations` command")
        {
            AddCommand<AddCommand>();
        }
    }
}