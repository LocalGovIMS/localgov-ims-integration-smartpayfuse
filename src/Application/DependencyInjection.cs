using Application.Behaviours;
using Application.Builders;
using Application.Cryptography;
using Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Application.Entities;


namespace Application
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            services.AddTransient<ICryptographyService, MD5CryptographyService>();
            services.AddTransient<IBuilder<PaymentBuilderArgs, SmartPayFusePayment>, PaymentBuilder>();

            AddLocalGovImsApiClients(services, configuration);

            return services;
        }

        private static IServiceCollection AddLocalGovImsApiClients(this IServiceCollection services, IConfiguration configuration)
        {
            var localGovImsApiBaseUrl = configuration.GetValue<string>("LocalGovImsApiUrl");

            services.AddTransient<LocalGovImsApiClient.Api.IFundMetadataApi>(s => new LocalGovImsApiClient.Api.FundMetadataApi(localGovImsApiBaseUrl));
            services.AddTransient<LocalGovImsApiClient.Api.IFundsApi>(s => new LocalGovImsApiClient.Api.FundsApi(localGovImsApiBaseUrl));
            services.AddTransient<LocalGovImsApiClient.Api.IMethodOfPaymentsApi>(s => new LocalGovImsApiClient.Api.MethodOfPaymentsApi(localGovImsApiBaseUrl));
            services.AddTransient<LocalGovImsApiClient.Api.IPendingTransactionsApi>(s => new LocalGovImsApiClient.Api.PendingTransactionsApi(localGovImsApiBaseUrl));
            services.AddTransient<LocalGovImsApiClient.Api.IProcessedTransactionsApi>(s => new LocalGovImsApiClient.Api.ProcessedTransactionsApi(localGovImsApiBaseUrl));

            return services;
        }
    }
}
