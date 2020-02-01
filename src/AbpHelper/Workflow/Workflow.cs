using System.Collections.Generic;
using System.Threading.Tasks;
using AbpHelper.Steps;

namespace AbpHelper.Workflow
{
    public class Workflow
    {
        private readonly IList<IStep> _steps;

        public Workflow(IList<IStep> steps)
        {
            _steps = steps;
        }

        public async Task Run()
        {
            foreach (var step in _steps) await step.Run();
        }
    }
}