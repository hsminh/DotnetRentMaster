using RentMaster.Ai.Interfaces;
using RentMaster.Ai.Services;

namespace RentMaster.Ai
{
    public static class DependencyInjection
    {
        public static IServiceCollection AiModule(this IServiceCollection services)
        {
            services.AddScoped<IGoogleAiService, GoogleAiService>();
            return services;
        }
    }
}