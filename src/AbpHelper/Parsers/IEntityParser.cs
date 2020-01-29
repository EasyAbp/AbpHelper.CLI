using System.Threading.Tasks;
using AbpHelper.Dtos;
using Volo.Abp.DependencyInjection;

namespace AbpHelper.Parsers
{
    public interface IEntityParser : ITransientDependency
    {
        Task<EntityInfo> Parse(string text);
    }
}