using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace DraggableReorder
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDraggableReorder(this IServiceCollection services)
        {
            if (services.Any(sd => sd.ImplementationType == typeof(DraggableService<>)))
                return services;

            services.AddScoped(typeof(DraggableService<>));

            return services;
        }
    }
}
