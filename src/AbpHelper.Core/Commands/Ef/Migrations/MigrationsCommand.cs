using System;
using EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Add;
using EasyAbp.AbpHelper.Core.Commands.Ef.Migrations.Remove;

namespace EasyAbp.AbpHelper.Core.Commands.Ef.Migrations
{
    public class MigrationsCommand : CommandBase
    {
        public MigrationsCommand(IServiceProvider serviceProvider) : base(serviceProvider, "migrations", "run `dotnet ef migrations` command")
        {
            AddCommand<AddCommand>();
            AddCommand<RemoveCommand>();
        }
    }
}