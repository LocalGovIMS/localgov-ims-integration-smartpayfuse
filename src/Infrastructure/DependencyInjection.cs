using Application.Clients.CybersourceRestApiClient.Interfaces;
using Infrastructure.Clients;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Data;
using Application.Data;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<ICybersourceRestApiClient, CybersourceRestApiClient>();
            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));

            return services;
        }
    }
}
