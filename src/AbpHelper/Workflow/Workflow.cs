using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AbpHelper.Steps;

namespace AbpHelper.Workflow
{
    public class Workflow
    {
        public Workflow(IList<Step> steps)
        {
            Steps = new ReadOnlyCollection<Step>(steps);
        }

        public IReadOnlyList<Step> Steps { get; }

        public async Task Run()
        {
            foreach (var step in Steps) await step.Run();
        }
    }
}