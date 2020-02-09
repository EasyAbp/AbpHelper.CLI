using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Steps;
using Elsa.Expressions;
using Elsa.Services.Models;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AbpHelper.Tests.Parsers
{
    public class EntityParserStep_Tests : AbpHelperTestBase
    {
        public EntityParserStep_Tests(ITestOutputHelper output)
        {
            _output = output;
            _entityParserStep = GetRequiredService<EntityParserStep>();
        }

        private readonly ITestOutputHelper _output;

        private readonly EntityParserStep _entityParserStep;

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

namespace Acme.BookStore
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
            await UsingEntityFile(code, async file =>
            {
                // Arrange
                _entityParserStep.EntityFile = new LiteralExpression(file); 

                // Act
                await _entityParserStep.ExecuteAsync()

                // Assert
                var info = _entityParserStep.GetParameter<EntityInfo>("EntityInfo");
                info.Namespace.ShouldBe("Acme.BookStore");
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
        }

        [Fact]
        public async Task Parse_Entity_Without_PrimaryKey()
        {
            var code = @"
namespace Acme.BookStore
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
            await UsingEntityFile(code, async file =>
            {
                // Arrange
                _entityParserStep.SetParameter(FileFinderStep.DefaultFilesParameterName, file);

                // Act
                await _entityParserStep.Run();

                // Assert
                var info = _entityParserStep.GetParameter<EntityInfo>("EntityInfo");
                info.Namespace.ShouldBe("Acme.BookStore");
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
            });
        }

        [Fact]
        public async Task Parse_SyntaxError()
        {
            await UsingEntityFile("invalid c# code", async file =>
            {
                // Arrange
                _entityParserStep.SetParameter(FileFinderStep.DefaultFilesParameterName, file);

                // Act
                var ex = await Assert.ThrowsAsync<ParseException>(() => _entityParserStep.Run());

                // Arrange
                _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
                ex.ShouldNotBeNull();
            });
        }
    }
}