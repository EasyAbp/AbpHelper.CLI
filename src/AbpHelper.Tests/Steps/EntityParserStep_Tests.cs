using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EasyAbp.AbpHelper.Core.Models;
using EasyAbp.AbpHelper.Core.Steps.Abp;
using EasyAbp.AbpHelper.Core.Steps.Common;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EasyApp.AbpHelper.Tests.Steps
{
    public class EntityParserStep_Tests : StepTestsBase
    {
        public EntityParserStep_Tests(ITestOutputHelper output)
        {
            _output = output;
            _step = GetRequiredService<EntityParserStep>();
        }

        private readonly ITestOutputHelper _output;

        private readonly EntityParserStep _step;

        private async Task UsingEntityFile(string code, Func<string, Task> action)
        {
            string file = null;
            try
            {
                file = Path.GetTempFileName();
                File.WriteAllText(file, code);
                await action(file);
            }
            finally
            {
                if (file != null) File.Delete(file);
            }
        }

        [Fact]
        public async Task Parse_Entity_With_PrimaryKey()
        {
            var code = @"
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.BookStore.Books
{
    public class Book : AuditedAggregateRoot<Guid>
    {
        public string Name { get; set; }
   
        public BookType Type { get; set; }
   
        public DateTime PublishDate { get; set; }
   
        public float Price { get; set; }
    }
}
";
            await UsingWorkflowContext(async ctx =>
            {
                await UsingEntityFile(code, async file =>
                {
                    // Arrange
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);
                    ctx.SetVariable("ProjectInfo", new ProjectInfo(@"c:\abp", "Acme.BookStore", TemplateType.Application, UiFramework.RazorPages, false));

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var info = ctx.GetVariable<EntityInfo>("EntityInfo");
                    info.Namespace.ShouldBe("Acme.BookStore.Books");
                    info.RelativeNamespace.ShouldBe("Books");
                    info.NamespaceLastPart.ShouldBe("Books");
                    info.RelativeDirectory.ShouldBe("Books");
                    info.Name.ShouldBe("Book");
                    info.NamePluralized.ShouldBe("Books");
                    info.BaseType.ShouldBe("AuditedAggregateRoot");
                    info.PrimaryKey.ShouldBe("Guid");
                    info.Properties.Count.ShouldBe(4);
                    info.Properties[0].Type.ShouldBe("string");
                    info.Properties[1].Type.ShouldBe("BookType");
                    info.Properties[2].Type.ShouldBe("DateTime");
                    info.Properties[3].Type.ShouldBe("float");
                    info.Properties[0].Name.ShouldBe("Name");
                    info.Properties[1].Name.ShouldBe("Type");
                    info.Properties[2].Name.ShouldBe("PublishDate");
                    info.Properties[3].Name.ShouldBe("Price");
                });
            });
        }

        [Fact]
        public async Task Parse_Entity_Without_PrimaryKey()
        {
            var code = @"
namespace Acme.BookStore.EasyAbp.BookStore.UserRoles
{
    public class UserRole : Entity
    {
        public Guid UserId { get; set; }

        public Guid RoleId { get; set; }
        
        public DateTime CreationTime { get; set; }

        public UserRole()
        {
                
        }
        
        public override object[] GetKeys()
        {
            return new object[] { UserId, RoleId };
        }
    }
}
";
            await UsingWorkflowContext(async ctx =>
            {
                await UsingEntityFile(code, async file =>
                {
                    // Arrange
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);
                    ctx.SetVariable("ProjectInfo", new ProjectInfo(@"c:\abp", "Acme.BookStore", TemplateType.Module, UiFramework.RazorPages, false));

                    // Act
                    await _step.ExecuteAsync(ctx, CancellationToken.None);

                    // Assert
                    var info = ctx.GetVariable<EntityInfo>("EntityInfo");
                    info.Namespace.ShouldBe("Acme.BookStore.EasyAbp.BookStore.UserRoles");
                    info.RelativeNamespace.ShouldBe("EasyAbp.BookStore.UserRoles");
                    info.NamespaceLastPart.ShouldBe("UserRoles");
                    info.RelativeDirectory.ShouldBe("EasyAbp/BookStore/UserRoles");
                    info.Name.ShouldBe("UserRole");
                    info.NamePluralized.ShouldBe("UserRoles");
                    info.BaseType.ShouldBe("Entity");
                    info.PrimaryKey.ShouldBeNull();
                    info.Properties.Count.ShouldBe(3);
                    info.Properties[0].Type.ShouldBe("Guid");
                    info.Properties[1].Type.ShouldBe("Guid");
                    info.Properties[2].Type.ShouldBe("DateTime");
                    info.Properties[0].Name.ShouldBe("UserId");
                    info.Properties[1].Name.ShouldBe("RoleId");
                    info.Properties[2].Name.ShouldBe("CreationTime");
                    info.CompositeKeyName.ShouldBe("UserRoleKey");
                    info.CompositeKeys[0].Name.ShouldBe("UserId");
                    info.CompositeKeys[1].Name.ShouldBe("RoleId");
                });
            });
        }

        [Fact]
        public async Task Parse_SyntaxError()
        {
            await UsingWorkflowContext(async ctx =>
            {
                await UsingEntityFile("invalid c# code", async file =>
                {
                    // Arrange
                    ctx.SetVariable(FileFinderStep.DefaultFileParameterName, file);

                    // Act
                    var ex = await Assert.ThrowsAsync<ParseException>(() => _step.ExecuteAsync(ctx, CancellationToken.None));

                    // Arrange
                    _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                    ex.ShouldNotBeNull();
                });
            });
        }
    }
}