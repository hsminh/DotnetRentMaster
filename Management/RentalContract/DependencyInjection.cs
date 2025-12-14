using Microsoft.Extensions.DependencyInjection;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Repositories;
using RentMaster.Management.RentalContract.Repositories;
using RentMaster.Management.RentalContract.Services;

namespace RentMaster.Management.RentalContract;

public static class DependencyInjection
{
    public static IServiceCollection AddRentalContractDependencies(this IServiceCollection services)
    {
        services.AddScoped<RentalContractRepository>();
        services.AddScoped<RentalContractService>((provider) =>
            new RentalContractService(
                provider.GetRequiredService<RentalContractRepository>(),
                provider.GetRequiredService<ApartmentRepository>(),
                provider.GetRequiredService<ApartmentRoomRepository>(),
                provider.GetRequiredService<AppDbContext>()
            )
        );

        return services;
    }
}
