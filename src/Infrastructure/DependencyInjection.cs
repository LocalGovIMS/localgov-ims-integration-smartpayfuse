using Application.Clients.LocalGovImsPaymentApi;
using Infrastructure.Clients.LocalGovImsPaymentApi;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<ILocalGovImsPaymentApiClient, LocalGovImsPaymentApiClient>();

            return services;
        }
    }
}
