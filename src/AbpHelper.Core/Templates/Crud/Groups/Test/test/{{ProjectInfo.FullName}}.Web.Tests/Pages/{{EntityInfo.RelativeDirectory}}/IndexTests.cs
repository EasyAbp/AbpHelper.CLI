{{ SKIP_GENERATE = (ProjectInfo.TemplateType == 'Module')}}
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace {{ ProjectInfo.FullName }}.Pages.{{ EntityInfo.RelativeNamespace}}
{
    public class Index_Tests : {{ ProjectInfo.Name }}WebTestBase
    {
        /*
        [Fact]
        public async Task Index_Page_Test()
        {
            // Arrange

            // Act
            var response = await GetResponseAsStringAsync("/{{ EntityInfo.Name }}");

            // Assert
            response.ShouldNotBeNull();
        }
        */
    }
}
