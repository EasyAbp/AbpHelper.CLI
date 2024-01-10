using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Modularity;

namespace {{ EntityInfo.Namespace }};

public class {{ EntityInfo.Name }}DomainTests<TStartupModule> : {{ ProjectInfo.Name }}DomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    public {{ EntityInfo.Name }}DomainTests()
    {
    }

    /*
    [Fact]
    public async Task Test1()
    {
        // Arrange

        // Assert

        // Assert
    }
    */
}