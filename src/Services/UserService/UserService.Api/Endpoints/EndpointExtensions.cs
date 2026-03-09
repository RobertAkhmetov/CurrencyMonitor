using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace UserService.Api.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        var descriptors = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IEndpoint)))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
            .ToArray();

        services.TryAddEnumerable(descriptors);
        return services;
    }

    public static IApplicationBuilder MapEndpoints(this WebApplication app, IEndpointRouteBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        var builder = routeGroupBuilder ?? app;

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
