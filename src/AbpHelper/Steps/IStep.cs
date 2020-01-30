using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public interface IStep
    {
        Task Execute();
    }
}