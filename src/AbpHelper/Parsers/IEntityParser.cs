using System.Threading.Tasks;
using AbpHelper.Models;
using Volo.Abp.DependencyInjection;

namespace AbpHelper.Parsers
{
    public interface IEntityParser : ITransientDependency
    {
        Task<EntityInfo> Parse(string sourceText);
    }
}