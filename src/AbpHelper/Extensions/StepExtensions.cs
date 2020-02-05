using AbpHelper.Steps;

namespace AbpHelper.Extensions
{
    public static class StepExtensions
    {
        public static T Get<T>(this Step step)
        {
            return step.GetParameter<T>(typeof(T).Name);
        }
    }
}