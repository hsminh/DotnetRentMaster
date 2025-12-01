using Microsoft.Extensions.DependencyInjection.Extensions;
using Payments.VnPay.Models;
using Payments.VnPay.Services;

namespace Payments.VnPay;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVnPay(this IServiceCollection services, Action<VnPayConfig> configure)
    {
        services.Configure(configure);

        services.AddHttpClient("VnPay", (serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<VnPayConfig>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        });

        // Register VNPay services
        services.TryAddScoped<IVnPayService, VnPayService>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        return services;
    }
}
