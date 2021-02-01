using System;
using System.Linq;
using System.Reflection;
using Elsa.Services.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EasyAbp.AbpHelper.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAllActivities(this IServiceCollection services)
        {
            var activityTypes = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.IsAssignableTo(typeof(IActivity)))
                ;
            foreach (var activity in activityTypes)
                services.AddTransient(activity)
                    .AddTransient(sp => (IActivity) sp.GetRequiredService(activity))
                    ;

            return services;
        }
    }
}