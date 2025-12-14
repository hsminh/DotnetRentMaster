using RentMaster.Data;
using RentMaster.Management.RealEstate.Services;

namespace RentMaster.Management.ConsumerFavorite
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConsumerFavoriteModule(this IServiceCollection services)
        {
            services.AddScoped<Repositories.ConsumerFavoriteRepository>();
            
            services.AddScoped<Services.ConsumerFavoriteService>(sp => 
                new Services.ConsumerFavoriteService(
                    sp.GetRequiredService<ApartmentService>(),
                    sp.GetRequiredService<ApartmentRoomService>(),
                    sp.GetRequiredService<Repositories.ConsumerFavoriteRepository>(),
                    sp.GetRequiredService<AppDbContext>()
                )
            );
            
            return services;
        }
    }
}
