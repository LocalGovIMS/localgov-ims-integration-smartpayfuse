using Application.Behaviours;
using Application.Builders;
using Application.Cryptography;
using Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Application
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

            services.AddTransient<ICryptographyService, MD5CryptographyService>();
            services.AddTransient<IBuilder<PaymentBuilderArgs, Payment>, PaymentBuilder>();

            return services;
        }
    }
}
