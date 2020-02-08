using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AbpHelper.Steps
{
    public class LoopStep<T> : Step
    {
        public Func<IEnumerable<T>> LoopOn { get; set; }
        public Func<T, Task> LoopBody { get; set; }

        protected override async Task RunStep()
        {
            foreach (var loop in LoopOn()) await LoopBody(loop);
        }
    }
}