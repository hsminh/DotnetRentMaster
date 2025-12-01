using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Repositories;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.ConsumerContact.Repositories;
using RentMaster.Management.ConsumerContact.Services;

namespace RentMaster.Management.ConsumerContact
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConsumerContactModule(this IServiceCollection services)
        {
            services.AddScoped<ConsumerContactRepository>();
            
            services.AddScoped<ConsumerContactService>(sp => 
                new ConsumerContactService(
                    sp.GetRequiredService<ConsumerContactRepository>(),
                    sp.GetRequiredService<AppDbContext>()
                )
            );
            
            return services;
        }
    }
}
