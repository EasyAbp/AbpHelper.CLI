using System;
using System.Threading.Tasks;
using AbpHelper.Parsers;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace AbpHelper.Tests.Parsers
{
    public class EntityParser_Tests : AbpHelperTestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly IEntityParser _entityParser;

        #region Entity Code for Test

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

        #endregion

        public EntityParser_Tests(ITestOutputHelper output)
        {
            _output = output;
            _entityParser = GetRequiredService<IEntityParser>();
        }

        [Fact]
        public async Task Parse_OK()
        {
            // Arrange
            string sourceCode = EntityCode;
            
            // Act
            var info = await _entityParser.Parse(sourceCode);

            // Assert
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
        }
        
        [Fact]
        public async Task Parse_SyntaxError()
        {
            // Arrange
            string sourceCode = "not valid c# code";
            
            // Act
            var ex = await Assert.ThrowsAsync<ParseException>(() => _entityParser.Parse(sourceCode));
            
            // Arrange
            ex.ShouldNotBeNull();
            _output.WriteLine(string.Join(Environment.NewLine, ex.Errors));
        }
    }
}