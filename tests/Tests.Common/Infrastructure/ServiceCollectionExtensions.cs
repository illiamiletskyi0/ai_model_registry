using Microsoft.Extensions.DependencyInjection;

namespace Tests.Common.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RemoveServiceByType(this IServiceCollection services, Type serviceType)
    {
        var descriptors = services
            .Where(descriptor => descriptor.ServiceType == serviceType)
            .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        return services;
    }
}
