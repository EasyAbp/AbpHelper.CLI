using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class EmptyStep : Step
    {
        protected override Task RunStep()
        {
            return Task.CompletedTask;
        }
    }
}