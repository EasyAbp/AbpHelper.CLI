using System.Threading.Tasks;
using AbpHelper.Dtos;

namespace AbpHelper.Parsers
{
    public interface IEntityParser
    {
        Task<EntityInfo> Parse(string text);
    }
}