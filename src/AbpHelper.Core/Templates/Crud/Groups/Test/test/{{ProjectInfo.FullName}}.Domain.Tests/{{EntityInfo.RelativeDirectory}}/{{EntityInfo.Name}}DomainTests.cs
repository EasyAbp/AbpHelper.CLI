using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace {{ EntityInfo.Namespace }};

public abstract class {{ EntityInfo.Name }}DomainTests<TStartupModule> : {{ ProjectInfo.Name }}TestBase<TStartupModule>
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