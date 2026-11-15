using System;
using Microsoft.Extensions.DependencyInjection;
using ServiceAbstractionLayer;
using ServiceLayer.MappingProfiles;

namespace ServiceLayer;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(SubjectProfile).Assembly);
        services.AddScoped<IServiceManager, ServiceManager>();
        return services;
    }
}
