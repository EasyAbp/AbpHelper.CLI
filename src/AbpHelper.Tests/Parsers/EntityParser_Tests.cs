using System.Threading.Tasks;
using AbpHelper.Parsers;
using Shouldly;
using Xunit;

namespace AbpHelper.Tests.Parsers
{
    public class EntityParser_Tests : AbpHelperTestBase
    {
        private IEntityParser _entityParser;
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

        public EntityParser_Tests()
        {
            _entityParser = GetRequiredService<IEntityParser>();
        }
        
       [Fact] 
       public async Task Parse_Test()
       {
           // Arrange
           // Act
           var info = await _entityParser.Parse(EntityCode);
           
           // Assert
           info.Namespace.ShouldBe("Acme.BookStore");
       }
    }
}