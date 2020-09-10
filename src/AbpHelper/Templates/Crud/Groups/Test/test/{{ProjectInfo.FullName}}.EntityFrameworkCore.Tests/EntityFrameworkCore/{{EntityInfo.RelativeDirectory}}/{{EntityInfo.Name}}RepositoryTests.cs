{{- if Option.SkipCustomRepository
    if EntityInfo.CompositeKeyName
        repository = "IRepository<" + EntityInfo.Name + ">"
    else
        repository = "IRepository<" + EntityInfo.Name + ", " + EntityInfo.PrimaryKey + ">"
    end
else
    repository = "I" + EntityInfo.Name + "Repository"
end ~}}
using System;
using System.Threading.Tasks;
using {{ EntityInfo.Namespace }};
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace {{ ProjectInfo.FullName }}.EntityFrameworkCore.{{ EntityInfo.RelativeNamespace}}
{
    public class {{ EntityInfo.Name }}RepositoryTests : {{ ProjectInfo.Name }}EntityFrameworkCoreTestBase
    {
        private readonly {{ repository }} _{{ EntityInfo.Name | abp.camel_case }}Repository;

        public {{ EntityInfo.Name }}RepositoryTests()
        {
            _{{ EntityInfo.Name | abp.camel_case }}Repository = GetRequiredService<{{ repository }}>();
        }

        /*
        [Fact]
        public async Task Test1()
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Arrange

                // Act

                //Assert
            });
        }
        */
    }
}
