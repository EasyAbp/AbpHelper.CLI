using System.Linq;
using System.Reflection;
using Elsa.Options;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static ElsaOptionsBuilder AddAbpHelperActivities(this ElsaOptionsBuilder services)
        {
            var activityTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsAbstract)
                .Where(t => t.IsAssignableTo(typeof(IActivity)));

            foreach (var activityType in activityTypes)
            {
                services.AddActivity(activityType);
            }

            return services;
        }
    }
}