using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public interface IStep<in TInput, TOutput>
    {
        Task<TOutput> Run(TInput input);
    }
}