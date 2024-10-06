using Shouldly;
using System.Threading.Tasks;
using Volo.Abp.Modularity;
using Xunit;

namespace {{ EntityInfo.Namespace }};

public abstract class {{ EntityInfo.Name }}AppServiceTests<TStartupModule> : {{ ProjectInfo.Name }}ApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly I{{ EntityInfo.Name }}AppService _{{ EntityInfo.Name | abp.camel_case }}AppService;

    public {{ EntityInfo.Name }}AppServiceTests()
    {
        _{{ EntityInfo.Name | abp.camel_case }}AppService = GetRequiredService<I{{ EntityInfo.Name }}AppService>();
    }

    /*
    [Fact]
    public async Task Test1()
    {
        // Arrange

        // Act

        // Assert
    }
    */
}

