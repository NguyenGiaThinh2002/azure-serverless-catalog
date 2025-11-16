using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Future: MediatR, FluentValidation, Application Services
        return services;
    }
}