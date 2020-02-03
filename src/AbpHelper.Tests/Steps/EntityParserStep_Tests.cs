using System;
using System.IO;
using System.Threading.Tasks;
using AbpHelper.Models;
using AbpHelper.Steps;
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

        private const string EntityCode = @"
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
        public async Task Parse_OK()
        {
            await UsingEntityFile(EntityCode, async file =>
            {
                // Arrange
                _entityParserStep.SetParameter("FilePathName", file);

                // Act
                await _entityParserStep.Run();

                // Assert
                var info = _entityParserStep.GetParameter<EntityInfo>("EntityInfo");
                info.Namespace.ShouldBe("Acme.BookStore");
                info.ClassName.ShouldBe("Book");
                info.BaseType.ShouldBe("AuditedAggregateRoot<Guid>");
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
        public async Task Parse_SyntaxError()
        {
            await UsingEntityFile("invalid c# code", async file =>
            {
                // Arrange
                _entityParserStep.SetParameter("FilePathName", file);

                // Act
                var ex = await Assert.ThrowsAsync<ParseException>(() => _entityParserStep.Run());

                // Arrange
                ex.ShouldNotBeNull();
                _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
            });
        }
    }
}