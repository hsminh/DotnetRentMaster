using RentMaster.Core.Backend.Auth.Interface;
using RentMaster.Core.Backend.Auth.service;

namespace RentMaster.Core.Backend.Auth
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}