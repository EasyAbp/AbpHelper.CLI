using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Volo.Abp.Modularity;

namespace {{ EntityInfo.Namespace }};

public class {{ EntityInfo.Name }}AppServiceTests<TStartupModule> : {{ ProjectInfo.Name }}ApplicationTestBase<TStartupModule>
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

