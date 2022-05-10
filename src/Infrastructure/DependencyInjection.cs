using Application.Clients.CybersourceRestApiClient.Interfaces;
using Application.Clients.LocalGovImsPaymentApi;
using Infrastructure.Clients;
using Infrastructure.Clients.LocalGovImsPaymentApi;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<ILocalGovImsPaymentApiClient, LocalGovImsPaymentApiClient>();
            services.AddTransient<ICybersourceRestApiClient, CybersourceRestApiClient>();

            return services;
        }
    }
}
